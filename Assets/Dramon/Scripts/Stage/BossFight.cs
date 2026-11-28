using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 일반 모드 보스: 판을 시작할 때 10종 중 1마리를 보스로 정하고 이번 판 일반 결투에서 뺀다 (DragonSpawnList 에셋은 그대로).
    /// 마지막 스테이지를 끝내면 숲 가운데에서 보스전 — 1.25배 크기·붉은 기운·이름 띠, 체력이 절반 아래로 떨어지면 분노해서 필살 숨결 추가.
    /// </summary>
    public class BossFight : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] GamePhaseController phaseController;
        [SerializeField] DuelSession duelSession;
        [Tooltip("보스 후보 (10종)")]
        [SerializeField] DragonSpawnList bossCandidates;
        [SerializeField] ForestBoard board;
        [SerializeField] DragonBattlerView battler;
        [Tooltip("분노 조건을 볼 드래곤 체력")]
        [SerializeField] DragonHealth dragonHealth;
        [Tooltip("분노하면 필살 숨결을 행동 순서에 더해요")]
        [SerializeField] DragonIntentPicker intent;
        [SerializeField] BattleLogMessage log;
        [SerializeField] FloatingNumberUI floating;
        [Tooltip("붉은 기운")]
        [SerializeField] BossAuraView aura;
        [Tooltip("보스 이름 띠")]
        [SerializeField] BossNameBandUI nameBand;
        [Tooltip("분노할 때 한 번 보여 줄 이펙트 (Dragon/EnrageFX, 비우면 이펙트 없음)")]
        [SerializeField] UIFramePlayer enrageEffect;

        [Header("문구")]
        [Tooltip("분노 때 드래곤 위에 뜨는 글자")]
        [SerializeField] string enrageFloatText = "분노!";

        [Header("테스트 (에디터에서만)")]
        [Tooltip("넣어 두면 이 드래곤이 항상 보스 (에디터에서만)")]
        [SerializeField] DragonSpecies testBoss;

        BossRules rules;
        DragonActionData finisher;

        /// <summary>이번 판 보스 (보스가 없는 모드면 null)</summary>
        public DragonSpecies Boss { get; private set; }
        public BossRules Rules => rules;
        /// <summary>지금 보스전 중인지</summary>
        public bool IsActive { get; private set; }
        public bool Enraged { get; private set; }
        /// <summary>이 행동이 분노 때 더한 필살 숨결인지 (예고 아이콘용)</summary>
        public bool IsFinisher(DragonActionData action) => action != null && action == finisher;

        public string BossName => Boss == null ? "" : rules != null ? string.Format(rules.NameFormat, Boss.DisplayName) : Boss.DisplayName;

        void OnEnable()
        {
            if (dragonHealth != null) dragonHealth.Changed += HandleHealth;
            if (phaseController != null) phaseController.PhaseChanged += HandlePhase;
            if (duelSession != null) duelSession.EncounterStarted += HandleEncounterStarted;
        }

        void OnDisable()
        {
            if (dragonHealth != null) dragonHealth.Changed -= HandleHealth;
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhase;
            if (duelSession != null) duelSession.EncounterStarted -= HandleEncounterStarted;
        }

        /// <summary>판 시작: 보스를 정하고 일반 결투에서 뺀다 (보스가 없는 모드면 아무것도 빼지 않음)</summary>
        public void PrepareRun(GameModeData mode)
        {
            End();
            rules = mode != null ? mode.BossRules : null;
            Boss = null;
            if (mode != null && mode.UseBoss)
            {
                Boss = Application.isEditor && testBoss != null ? testBoss : bossCandidates != null ? bossCandidates.PickRandom() : null;
            }
            if (duelSession != null) duelSession.SetExcludedSpecies(Boss);
        }

        /// <summary>숲 가운데에서 보스전 시작 (검 사냥 단계에서). 시작했으면 true</summary>
        public bool StartBoss()
        {
            if (Boss == null || duelSession == null || board == null) return false;
            var tile = CenterTile();
            if (tile == null) return false;
            IsActive = true;
            Enraged = false;
            finisher = null;
            if (battler != null) battler.SetRuntimeScale(rules != null ? rules.SizeMultiplier : 1f);
            if (!duelSession.StartBoss(Boss, tile, BossName))
            {
                End();
                return false;
            }
            return true;
        }

        /// <summary>보스전 끝 (이기거나 지거나 다시 시작): 크기·기운·이름 띠를 원래대로</summary>
        public void End()
        {
            IsActive = false;
            Enraged = false;
            if (battler != null) battler.SetRuntimeScale(1f);
            if (aura != null) aura.Hide();
            if (nameBand != null) nameBand.Hide();
            if (enrageEffect != null) enrageEffect.Stop();
        }

        ForestTile CenterTile()
        {
            var center = new Vector2(board.Width - 1, board.Height - 1) * 0.5f;
            ForestTile best = null;
            float bestD = float.MaxValue;
            foreach (var t in board.Tiles)
            {
                if (t == null) continue;
                float d = Vector2.SqrMagnitude((Vector2)t.GridPosition - center);
                if (d < bestD) { bestD = d; best = t; }
            }
            return best;
        }

        void HandleEncounterStarted(DragonSpecies species)
        {
            if (!IsActive) return;
            if (aura != null) aura.Show(false);
            if (nameBand != null) nameBand.Show(BossName);
        }

        void HandlePhase(GamePhase phase)
        {
            if (phase == GamePhase.GameOver || phase == GamePhase.LemonMarking) End();
        }

        void HandleHealth(int current, int max)
        {
            if (!IsActive || Enraged || rules == null || phaseController == null || phaseController.Phase != GamePhase.Duel) return;
            if (current <= 0 || max <= 0 || current > max * rules.EnrageHealthRatio) return;
            Enrage();
        }

        void Enrage()
        {
            Enraged = true;
            finisher = BuildFinisher();
            if (intent != null && finisher != null)
            {
                intent.AddExtraAction(finisher);
                intent.JumpTo(finisher);
            }
            if (log != null && rules != null) log.Show(string.Format(rules.EnrageMessage, BossName));
            if (floating != null) floating.OnDragon(enrageFloatText, FloatingNumberUI.Kind.Damage);
            if (battler != null) battler.ShowAttack();
            if (aura != null) aura.Show(true);
            if (enrageEffect != null) enrageEffect.Play();
        }

        /// <summary>필살 숨결: 그 드래곤의 숨결 × 1.5 피해, 상태 2배 (숨결이 없으면 가장 센 공격으로)</summary>
        DragonActionData BuildFinisher()
        {
            DragonActionData baseAction = null;
            foreach (var a in Boss.BattleActions)
            {
                if (a == null || !a.IsAttack) continue;
                if (a.Type == DragonActionType.Breath) { baseAction = a; break; }
                if (baseAction == null || a.Value * a.Hits > baseAction.Value * baseAction.Hits) baseAction = a;
            }
            if (baseAction == null) return null;
            int value = Mathf.RoundToInt(baseAction.Value * rules.FinisherDamageMultiplier);
            int status = Mathf.RoundToInt(baseAction.StatusAmount * rules.FinisherStatusMultiplier);
            return DragonActionData.CreateRuntime(rules.FinisherName, DragonActionType.Breath, value, 1, baseAction.Status, status, baseAction.BlockIgnoreRatio);
        }
    }
}
