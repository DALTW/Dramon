using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>레몬 표시 단계가 끝나면 걸린 시간으로 보상금을 계산해 지갑에 넣는다.</summary>
    public class MarkingRewardCalculator : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("시간 구간별 보상금 규칙 (ScriptableObject)")]
        [SerializeField] MarkingRewardSettings settings;
        [Tooltip("걸린 시간을 알려주는 타이머")]
        [SerializeField] MarkingTimer timer;
        [Tooltip("보상금을 넣을 지갑")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("단계 완료 순간을 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        [Header("스테이지 시간 (v3)")]
        [Tooltip("켜면 보상 구간 시간을 스테이지 숲 칸 수 × StageRules의 칸당 초(0.8 / 1.6 / 2.4)로 정해요. 끄면 MarkingRewardSettings 시간 그대로")]
        [SerializeField] bool useStageTimes = true;
        [Tooltip("지금 스테이지와 숲 크기를 알려줘요")]
        [SerializeField] StageProgress stageProgress;

        /// <summary>보상금을 지급했을 때 (걸린 시간(초), 보상금)</summary>
        public event Action<float, int> RewardGranted;

        public float LastSeconds { get; private set; }
        public int LastReward { get; private set; }

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        /// <summary>이번 스테이지 보상 구간 시간 (칸 수 × 칸당 초). 쓰지 않으면 null</summary>
        public List<float> StageLimits()
        {
            if (!useStageTimes || stageProgress == null || stageProgress.Rules == null) return null;
            int cells = stageProgress.Current.Cells;
            var list = new List<float>();
            foreach (var perCell in stageProgress.Rules.RewardSecondsPerCell) list.Add(cells * perCell);
            return list;
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.MarkingComplete) return;
            LastSeconds = timer != null ? timer.Elapsed : 0f;
            LastReward = settings != null ? settings.GetReward(LastSeconds, StageLimits()) : 0;
            if (wallet != null) wallet.Add(LastReward);
            RewardGranted?.Invoke(LastSeconds, LastReward);
        }
    }
}
