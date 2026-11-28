using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 검 사냥 단계의 결투 한 판을 관리한다.
    /// 봉인된 둥지에 들어가면 DragonSpawnList에서 드래곤을 무작위로 정하고(깜짝 등장) 등장 연출 → 결투.
    /// 이기면 드래곤 패배 모션 → 재료 드랍 → 전리품 창 → [확인] → 페이드 아웃 후 그 둥지를 정화하고 검 사냥으로. 지면 바로 게임 오버.
    /// </summary>
    public class DuelSession : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("게임 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("이 숲에서 나올 수 있는 드래곤 목록")]
        [SerializeField] DragonSpawnList spawnList;
        [Tooltip("결투 중 드래곤 체력")]
        [SerializeField] DragonHealth dragonHealth;
        [Tooltip("등장 연출 (비워 두면 연출 없이 바로 결투)")]
        [SerializeField] EncounterIntroSequence introSequence;
        [Tooltip("결투 화면 (승리를 누르면 임시 승패 버튼을 숨겨요)")]
        [SerializeField] DuelScreenUI duelScreen;
        [Tooltip("결투 화면의 드래곤 그림 (이기면 패배 모션을 보여 준 뒤 전리품으로)")]
        [SerializeField] DragonBattlerView battler;

        [Tooltip("결투 화면의 검사 그림 (이기면 승리 자세, 지면 쓰러진 자세)")]
        [SerializeField] HeroBattlerView hero;

        [Tooltip("카드 결투 진행 (비워 두면 예전처럼 임시 승패 버튼으로만 끝내요)")]
        [SerializeField] CardBattleController cardBattle;
        [Tooltip("스테이지 배율 (등장 연출의 드래곤 체력도 배율 값으로 보여 줘요)")]
        [SerializeField] DragonScaling scaling;

        [Header("드래곤 공격 → 검사 반응 (카드 결투 단계에서 PlayDragonAttack 호출)")]
        [Tooltip("드래곤 공격 모션이 시작되고 검사 반응이 나오기까지 기다리는 시간 (초)")]
        [Min(0f)]
        [SerializeField] float reactionDelay = 0.25f;

        [Header("전리품")]
        [Tooltip("드랍표로 얻을 재료를 굴려요")]
        [SerializeField] LootRoller lootRoller;
        [Tooltip("얻은 재료를 쌓아 둘 인벤토리")]
        [SerializeField] PlayerInventory inventory;
        [Tooltip("전리품 창 (비워 두면 창 없이 바로 숲으로 돌아가요)")]
        [SerializeField] LootResultUI lootResult;

        [Header("테스트 (에디터에서만 동작)")]
        [Tooltip("넣어 두면 무작위 대신 항상 이 드래곤이 나와요. 비우면 무작위")]
        [SerializeField] DragonSpecies testFixedSpecies;
        [Tooltip("켜면 드랍표의 모든 재료가 100% 확률로 나와요")]
        [SerializeField] bool testDropEverything;

        readonly HashSet<DragonSpecies> excluded = new HashSet<DragonSpecies>();

        /// <summary>지금 결투가 보스전인지</summary>
        public bool IsBossDuel { get; private set; }

        /// <summary>이번 판 일반 결투에서 뺄 드래곤 (보스). null이면 빼지 않음. DragonSpawnList 에셋은 그대로</summary>
        public void SetExcludedSpecies(DragonSpecies species)
        {
            excluded.Clear();
            if (species != null) excluded.Add(species);
        }

        /// <summary>등장 직전, 체력을 정하기 전 (StageProgress가 이번 결투의 스테이지 배율을 정함)</summary>
        public event Action<DragonSpecies> EncounterPreparing;
        /// <summary>등장 연출이 시작될 때 (이번에 나온 드래곤)</summary>
        public event Action<DragonSpecies> EncounterStarted;
        /// <summary>결투가 끝날 때 (true = 승리)</summary>
        public event Action<bool> DuelEnded;
        /// <summary>승리해서 재료를 얻었을 때 (드래곤, 얻은 재료 목록)</summary>
        public event Action<DragonSpecies, IReadOnlyList<LootEntry>> LootGranted;

        /// <summary>이번 결투에 나온 드래곤</summary>
        public DragonSpecies CurrentSpecies { get; private set; }
        /// <summary>이번 결투가 벌어진 둥지 칸</summary>
        public Vector2Int CurrentNest { get; private set; }
        public bool InDuel => phaseController != null && phaseController.Phase == GamePhase.Duel;

        DragonSpecies TestFixedSpecies => Application.isEditor ? testFixedSpecies : null;
        bool TestDropEverything => Application.isEditor && testDropEverything;

        DragonSpecies lastSpecies;
        bool ending;

        /// <summary>봉인된 둥지에 들어가 등장 연출을 시작한다. 시작했으면 true.</summary>
        public bool TryStart(ForestTile nestTile)
        {
            if (nestTile == null || phaseController == null || phaseController.Phase != GamePhase.SwordHunt) return false;
            var nest = nestTile.GridPosition;
            if (board == null || !board.IsSealedNest(nest)) return false;

            var species = TestFixedSpecies;
            if (species != null && excluded.Contains(species)) species = null; // 테스트 고정 드래곤이 이번 판 보스면 무작위로
            if (species == null && spawnList != null) species = spawnList.PickRandom(lastSpecies, excluded);
            if (species == null)
            {
                Debug.LogWarning("[Dramon] DuelSession: DragonSpawnList에 나올 수 있는 드래곤이 없어요.", this);
                return false;
            }

            IsBossDuel = false;
            CurrentNest = nest;
            CurrentSpecies = species;
            lastSpecies = species;
            ending = false;

            board.ActivateSealedNest(nest, species.NestSprites);
            EncounterPreparing?.Invoke(species);
            if (dragonHealth != null) dragonHealth.ResetTo(scaling != null ? scaling.MaxHealth(species) : species.MaxHealth);
            phaseController.EnterEncounter();
            EncounterStarted?.Invoke(species);

            if (introSequence != null) introSequence.PlayIntro(species, nestTile.transform.position, BeginDuel);
            else BeginDuel();
            return true;
        }

        /// <summary>
        /// 보스전 시작 (StageProgress·BossFight가 호출): 봉인된 굴이 아니어도 그 칸(숲 가운데)에서 등장 연출을 시작한다.
        /// displayName = 메시지에 쓸 이름 ("숲의 주인 – 화염 드래곤")
        /// </summary>
        public bool StartBoss(DragonSpecies species, ForestTile tile, string displayName)
        {
            if (species == null || tile == null || phaseController == null || phaseController.Phase != GamePhase.SwordHunt) return false;
            IsBossDuel = true;
            CurrentNest = tile.GridPosition;
            CurrentSpecies = species;
            ending = false;
            EncounterPreparing?.Invoke(species);
            if (dragonHealth != null) dragonHealth.ResetTo(scaling != null ? scaling.MaxHealth(species) : species.MaxHealth);
            phaseController.EnterEncounter();
            EncounterStarted?.Invoke(species);
            if (introSequence != null) introSequence.PlayIntro(species, tile.transform.position, BeginDuel, displayName);
            else BeginDuel();
            return true;
        }

        /// <summary>결투 승리 (임시 버튼 / 카드 결투가 호출): 드래곤 패배 모션 → 재료 드랍 → 전리품 창 → 숲으로.</summary>
        public void Win()
        {
            if (!InDuel || ending) return;
            ending = true;
            if (duelScreen != null) duelScreen.SetButtonsVisible(false);

            if (hero != null) hero.ShowVictory(); // 전리품 창까지 유지
            if (battler != null) battler.ShowDefeat(GrantLoot);
            else GrantLoot();
        }

        /// <summary>패배 모션이 끝난 뒤: 재료를 굴려 인벤토리에 넣고 전리품 창을 띄운다.</summary>
        void GrantLoot()
        {
            if (!InDuel) return; // 그 사이 게임 오버·다시 시작이 됐으면 무시
            var loot = lootRoller != null ? lootRoller.Roll(CurrentSpecies, TestDropEverything) : new List<LootEntry>();
            if (inventory != null) inventory.AddAll(loot);
            LootGranted?.Invoke(CurrentSpecies, loot);

            if (lootResult != null) lootResult.Show(CurrentSpecies, loot, LeaveDuel);
            else LeaveDuel();
        }

        /// <summary>결투 패배: 재료 없이 바로 게임 오버 (임시 버튼 / 카드 결투가 호출)</summary>
        public void Lose()
        {
            if (!InDuel || ending) return;
            ending = true;
            if (duelScreen != null) duelScreen.SetButtonsVisible(false);
            if (hero != null) hero.ShowDefeated(FinishLose); // 피격 → 쓰러짐을 잠깐 보여 준 뒤
            else FinishLose();
        }

        void FinishLose()
        {
            if (!InDuel) return; // 그 사이 다시 시작이 됐으면 무시
            phaseController.TriggerGameOver(GameOverReason.LostDuel);
            ending = false;
            DuelEnded?.Invoke(false);
        }

        /// <summary>
        /// 드래곤 공격 (카드 결투 단계에서 호출): 드래곤 공격 모션 + 조금 뒤 검사의 속성 반응(그 드래곤의 플레이어 피격 반응 → 자세 회복 → 대기).
        /// </summary>
        public void PlayDragonAttack()
        {
            if (!InDuel || ending) return;
            if (battler != null) battler.ShowAttack();
            if (hero == null) return;
            if (reactionDelay > 0f && isActiveAndEnabled) StartCoroutine(ReactLater());
            else hero.PlayDragonBreathReaction(CurrentSpecies);
        }

        System.Collections.IEnumerator ReactLater()
        {
            yield return new WaitForSeconds(reactionDelay);
            if (InDuel && !ending && hero != null) hero.PlayDragonBreathReaction(CurrentSpecies);
        }

        void BeginDuel()
        {
            phaseController.EnterDuel();
            if (cardBattle != null && cardBattle.UseCardBattle) cardBattle.Begin(CurrentSpecies); // 등장 연출 끝 → 카드 결투
        }

        /// <summary>전리품 창의 [확인] 뒤: 결투 화면 페이드 아웃 → 숲으로.</summary>
        void LeaveDuel()
        {
            if (!InDuel) return; // 그 사이 게임 오버·다시 시작이 됐으면 무시
            if (introSequence != null) introSequence.PlayOutro(FinishWin);
            else FinishWin();
        }

        void FinishWin()
        {
            if (board != null) board.PurifyNest(CurrentNest, CurrentSpecies != null ? CurrentSpecies.NestSprites : null);
            phaseController.ReturnToSwordHunt();
            ending = false;
            DuelEnded?.Invoke(true);
        }
    }
}
