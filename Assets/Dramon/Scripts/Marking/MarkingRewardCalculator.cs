using System;
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

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.MarkingComplete) return;
            LastSeconds = timer != null ? timer.Elapsed : 0f;
            LastReward = settings != null ? settings.GetReward(LastSeconds) : 0;
            if (wallet != null) wallet.Add(LastReward);
            RewardGranted?.Invoke(LastSeconds, LastReward);
        }
    }
}
