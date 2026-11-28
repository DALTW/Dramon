using System;
using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 스테이지 진행: 판 시작(1 스테이지) → 스테이지마다 숲 크기·둥지 수·카메라를 맞춤 → 결투마다 드래곤 배율을 정함 →
    /// 깨어난 둥지를 모두 정화하면 스테이지 클리어(보상 골드) → [다음 스테이지].
    /// 숫자는 모두 모드의 StageRules에서 가져온다.
    /// </summary>
    public class StageProgress : MonoBehaviour
    {
        [Header("모드·규칙")]
        [Tooltip("지금 모드 (Settings/Modes). 모드 선택 화면은 4단계에서 — 지금은 여기 넣은 모드로 시작해요")]
        [SerializeField] GameModeData mode;
        [Tooltip("모드에 StageRules가 비어 있을 때 쓸 규칙")]
        [SerializeField] StageRules fallbackRules;

        [Header("연결")]
        [SerializeField] GamePhaseController phaseController;
        [SerializeField] ForestBoard board;
        [Tooltip("둥지 수를 스테이지 값으로 정해 줘요")]
        [SerializeField] NestPlacer nestPlacer;
        [Tooltip("결투가 시작·끝나는 순간을 알려줘요")]
        [SerializeField] DuelSession duelSession;
        [Tooltip("이번 결투의 드래곤 배율 (스테이지 · 같은 스테이지 몇 번째 결투)")]
        [SerializeField] DragonScaling scaling;
        [Tooltip("스테이지 클리어 보상을 넣을 지갑")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("일반 모드 보스 (판마다 보스를 정하고, 마지막 스테이지 뒤 보스전)")]
        [SerializeField] BossFight bossFight;
        [Tooltip("클리어 기록·무한 모드 최고 스테이지 저장")]
        [SerializeField] ProgressSave save;
        [Tooltip("모드별 체력 증가 강화 상한을 정해 줘요 (일반 5단계, 무한 50단계)")]
        [SerializeField] PlayerUpgrades playerUpgrades;
        [Tooltip("숲 크기에 맞춰 칸 크기·카메라를 맞춰요 (비우면 카메라는 그대로)")]
        [SerializeField] ForestCameraFitter cameraFitter;

        [Header("진행")]
        [Tooltip("켜면 스테이지마다 숲 크기·둥지 수를 StageRules 값으로 바꿔요. 끄면 Scene 칸과 NestPlacer 값 그대로 (예전 방식)")]
        [SerializeField] bool applyStageBoard = true;
        [Tooltip("마지막 깨어난 둥지를 정화하고 클리어 창이 뜨기까지(초)")]
        [Min(0f)]
        [SerializeField] float clearDelay = 0.6f;
        [Tooltip("마지막 스테이지의 깨어난 둥지를 모두 정화하고 보스전이 시작되기까지(초)")]
        [Min(0f)]
        [SerializeField] float bossDelay = 1f;

        [Header("테스트 (에디터에서만)")]
        [Tooltip("0보다 크면 판을 이 스테이지부터 시작해요 (다시 시작 때도)")]
        [Min(0)]
        [SerializeField] int testStartStage;

        public GameModeData Mode => mode;
        public StageRules Rules => mode != null && mode.StageRules != null ? mode.StageRules : fallbackRules;
        public int Stage { get; private set; } = 1;
        /// <summary>이번 스테이지에서 시작한 결투 수</summary>
        public int DuelsInStage { get; private set; }
        /// <summary>이번 판에서 시작한 결투 수</summary>
        public int TotalDuels { get; private set; }
        /// <summary>지금 스테이지 값 (숲 크기·둥지·깨어난 둥지·에너지·뽑기)</summary>
        public StageRules.StageValues Current { get; private set; }
        /// <summary>다음 스테이지 값 (클리어 창 안내용)</summary>
        public StageRules.StageValues Next => Rules != null ? Rules.Get(Stage + 1) : default;
        /// <summary>마지막 클리어 보상</summary>
        public int LastClearGold { get; private set; }

        /// <summary>일반 모드 클리어 시간(초)</summary>
        public float LastClearSeconds { get; private set; }
        public bool LastClearWasBest { get; private set; }
        public bool LastClearUnlocked { get; private set; }
        public string LastBossName { get; private set; }
        /// <summary>이번 판 경과 시간(초)</summary>
        public float RunSeconds => Time.time - runStartTime;
        /// <summary>지금이 모드의 마지막 스테이지인지 (무한 모드는 항상 false)</summary>
        public bool IsLastStage => mode != null && !mode.IsEndless && Stage >= mode.LastStage;
        public ProgressSave Save => save;
        public BossFight Boss => bossFight;

        float runStartTime;

        /// <summary>일반 모드를 클리어했을 때 (보스 처치)</summary>
        public event Action RunCleared;
        /// <summary>스테이지가 시작됐을 때 (숲을 새로 깐 뒤)</summary>
        public event Action StageStarted;
        /// <summary>스테이지를 클리어했을 때 (스테이지, 보상 골드)</summary>
        public event Action<int, int> StageCleared;

        int StartStage => Application.isEditor && testStartStage > 0 ? testStartStage : 1;

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += BeginRun;
            if (duelSession != null)
            {
                duelSession.EncounterPreparing += HandleEncounterPreparing;
                duelSession.DuelEnded += HandleDuelEnded;
            }
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= BeginRun;
            if (duelSession != null)
            {
                duelSession.EncounterPreparing -= HandleEncounterPreparing;
                duelSession.DuelEnded -= HandleDuelEnded;
            }
        }

        void Start() => BeginRun();

        /// <summary>판 시작 (게임 시작·다시 시작): 1 스테이지 (테스트 시작 스테이지가 있으면 그 스테이지)</summary>
        public void BeginRun()
        {
            StopAllCoroutines();
            Stage = StartStage;
            TotalDuels = 0;
            runStartTime = Time.time;
            if (bossFight != null) bossFight.PrepareRun(mode);
            if (playerUpgrades != null && mode != null) playerUpgrades.SetStatLevelCap(UpgradeStat.MaxHealth, mode.MaxHealthUpgradeLevel);
            ApplyStage();
        }

        /// <summary>모드 선택 화면에서 모드를 고름: 그 모드의 1 스테이지로 새로 시작 (돈·덱·강화도 처음으로)</summary>
        public void StartMode(GameModeData newMode)
        {
            if (newMode != null) mode = newMode;
            if (phaseController != null) phaseController.Restart(); // → Restarted → BeginRun
            else BeginRun();
        }

        /// <summary>스테이지 클리어 창의 [다음 스테이지] 버튼</summary>
        public void GoToNextStage()
        {
            if (phaseController == null || phaseController.Phase != GamePhase.StageClear) return;
            Stage++;
            ApplyStage();
            phaseController.BeginNextStage();
        }

        void ApplyStage()
        {
            Current = Rules != null ? Rules.Get(Stage) : new StageRules.StageValues { stage = Stage };
            DuelsInStage = 0;
            if (applyStageBoard && Rules != null)
            {
                if (nestPlacer != null) nestPlacer.SetStageNestCount(Current.nests);
                if (board != null) board.BuildStageTiles(Current.width, Current.height);
            }
            if (cameraFitter != null) cameraFitter.Fit();
            if (save != null && mode != null && mode.IsEndless) save.RecordEndlessStage(Stage);
            if (scaling != null) scaling.SetDuel(Stage, 0, TotalDuels);
            StageStarted?.Invoke();
        }

        void HandleEncounterPreparing(DragonSpecies species)
        {
            if (bossFight != null && bossFight.IsActive)
            {
                if (scaling != null) scaling.SetBoss(Stage, bossFight.Rules);
                return; // 보스전은 결투 수에 넣지 않음
            }
            if (scaling != null) scaling.SetDuel(Stage, DuelsInStage, TotalDuels);
            DuelsInStage++;
            TotalDuels++;
        }

        void HandleDuelEnded(bool won)
        {
            if (!won) return;
            if (bossFight != null && bossFight.IsActive)
            {
                bossFight.End();
                FinishRun();
                return;
            }
            if (board == null || board.SealedNestCount > 0) return;
            StopAllCoroutines();
            if (IsLastStage && mode.UseBoss && bossFight != null && bossFight.Boss != null) StartCoroutine(BossLater());
            else StartCoroutine(ClearLater());
        }

        IEnumerator BossLater()
        {
            if (bossDelay > 0f) yield return new WaitForSeconds(bossDelay);
            if (bossFight == null || !bossFight.StartBoss()) ClearStage(); // 보스를 시작할 수 없으면 그냥 클리어
        }

        /// <summary>일반 모드 클리어: 기록 저장 → 엔딩</summary>
        void FinishRun()
        {
            if (phaseController == null || !phaseController.EnterEnding()) return;
            LastClearSeconds = RunSeconds;
            LastBossName = bossFight != null ? bossFight.BossName : "";
            if (save != null) (LastClearWasBest, LastClearUnlocked) = save.RecordNormalClear(LastClearSeconds, LastBossName);
            RunCleared?.Invoke();
        }

        IEnumerator ClearLater()
        {
            if (clearDelay > 0f) yield return new WaitForSeconds(clearDelay);
            ClearStage();
        }

        void ClearStage()
        {
            if (phaseController == null || !phaseController.CompleteStage()) return;
            LastClearGold = Rules != null ? Rules.ClearGold(Stage) : 0;
            if (wallet != null && LastClearGold > 0) wallet.Add(LastClearGold);
            StageCleared?.Invoke(Stage, LastClearGold);
        }

        // ── 테스트 (에디터에서만) ─────────────
        /// <summary>테스트: 검 사냥 중이면 남은 결투 없이 바로 스테이지 클리어</summary>
        public void TestClearStage()
        {
            if (!Application.isEditor) return;
            ClearStage();
        }

        /// <summary>테스트: 보스전 바로 시작 (마지막 스테이지로 숲을 깔고 검 사냥으로 넘긴 뒤 보스전)</summary>
        public void TestStartBoss()
        {
            if (!Application.isEditor || phaseController == null || mode == null || !mode.UseBoss || bossFight == null) return;
            if (phaseController.Phase != GamePhase.SwordHunt)
            {
                if (phaseController.Phase != GamePhase.LemonMarking) phaseController.Restart();
                Stage = Mathf.Max(Stage, mode.LastStage);
                ApplyStage();
                phaseController.CompleteMarking();
                phaseController.BeginSwordHunt();
            }
            if (bossFight.Boss == null) bossFight.PrepareRun(mode);
            bossFight.StartBoss();
        }

        /// <summary>테스트: 골드 추가</summary>
        public void TestAddGold(int amount)
        {
            if (Application.isEditor && wallet != null) wallet.Add(amount);
        }

        /// <summary>테스트: 이 스테이지부터 다시 시작 (돈·덱도 처음으로)</summary>
        public void TestRestartAt(int stage)
        {
            if (!Application.isEditor || phaseController == null) return;
            testStartStage = Mathf.Max(1, stage);
            phaseController.Restart();
        }
    }
}
