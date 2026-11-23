using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 검 사냥 단계의 결투 한 판을 관리한다.
    /// 봉인된 둥지에 들어가면 DragonSpawnList에서 드래곤을 무작위로 정하고(깜짝 등장) 등장 연출 → 결투.
    /// 이기면 페이드 아웃 후 그 둥지를 정화하고 검 사냥으로, 지면 바로 게임 오버.
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

        /// <summary>등장 연출이 시작될 때 (이번에 나온 드래곤)</summary>
        public event Action<DragonSpecies> EncounterStarted;
        /// <summary>결투가 끝날 때 (true = 승리)</summary>
        public event Action<bool> DuelEnded;

        /// <summary>이번 결투에 나온 드래곤</summary>
        public DragonSpecies CurrentSpecies { get; private set; }
        /// <summary>이번 결투가 벌어진 둥지 칸</summary>
        public Vector2Int CurrentNest { get; private set; }
        public bool InDuel => phaseController != null && phaseController.Phase == GamePhase.Duel;

        DragonSpecies lastSpecies;
        bool ending;

        /// <summary>봉인된 둥지에 들어가 등장 연출을 시작한다. 시작했으면 true.</summary>
        public bool TryStart(ForestTile nestTile)
        {
            if (nestTile == null || phaseController == null || phaseController.Phase != GamePhase.SwordHunt) return false;
            var nest = nestTile.GridPosition;
            if (board == null || !board.IsSealedNest(nest)) return false;

            var species = spawnList != null ? spawnList.PickRandom(lastSpecies) : null;
            if (species == null)
            {
                Debug.LogWarning("[Dramon] DuelSession: DragonSpawnList에 나올 수 있는 드래곤이 없어요.", this);
                return false;
            }

            CurrentNest = nest;
            CurrentSpecies = species;
            lastSpecies = species;
            ending = false;

            board.ActivateSealedNest(nest, species.NestSprites);
            if (dragonHealth != null) dragonHealth.ResetTo(species.MaxHealth);
            phaseController.EnterEncounter();
            EncounterStarted?.Invoke(species);

            if (introSequence != null) introSequence.PlayIntro(species, nestTile.transform.position, BeginDuel);
            else BeginDuel();
            return true;
        }

        /// <summary>결투 승리 (임시 버튼 / 카드 결투가 호출)</summary>
        public void Win()
        {
            if (!InDuel || ending) return;
            ending = true;
            if (introSequence != null) introSequence.PlayOutro(FinishWin);
            else FinishWin();
        }

        /// <summary>결투 패배: 바로 게임 오버 (임시 버튼 / 카드 결투가 호출)</summary>
        public void Lose()
        {
            if (!InDuel || ending) return;
            phaseController.TriggerGameOver(GameOverReason.LostDuel);
            DuelEnded?.Invoke(false);
        }

        void BeginDuel() => phaseController.EnterDuel();

        void FinishWin()
        {
            if (board != null) board.PurifyNest(CurrentNest, CurrentSpecies != null ? CurrentSpecies.NestSprites : null);
            phaseController.ReturnToSwordHunt();
            ending = false;
            DuelEnded?.Invoke(true);
        }
    }
}
