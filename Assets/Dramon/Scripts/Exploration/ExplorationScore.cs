using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 탐험 점수 = 기본 점수 - (쓴 레몬 수 × 레몬 감점) - (검 헛손질 수 × 헛손질 감점).
    /// 레몬은 무제한이지만 많이 쓸수록 점수가 줄어든다.
    /// </summary>
    public class ExplorationScore : MonoBehaviour
    {
        [Header("점수 규칙")]
        [Tooltip("감점 전 시작 점수")]
        [SerializeField, Min(0)] int baseScore = 1000;
        [Tooltip("레몬 1개를 쓸 때마다 깎이는 점수")]
        [SerializeField, Min(0)] int penaltyPerLemon = 10;
        [Tooltip("검으로 안전한 칸을 골랐을 때(헛손질) 깎이는 점수")]
        [SerializeField, Min(0)] int penaltyPerSwordMiss = 30;
        [Tooltip("켜면 점수가 0 아래로 내려가지 않아요")]
        [SerializeField] bool clampAtZero = true;

        [Header("연결")]
        [Tooltip("다시 시작할 때 점수를 초기화하기 위한 탐험 상태 관리자")]
        [SerializeField] ExplorationState explorationState;

        public event Action<int> ScoreChanged;

        public int LemonsUsed { get; private set; }
        public int SwordMisses { get; private set; }

        public int Score
        {
            get
            {
                int value = baseScore - LemonsUsed * penaltyPerLemon - SwordMisses * penaltyPerSwordMiss;
                return clampAtZero ? Mathf.Max(0, value) : value;
            }
        }

        void OnEnable()
        {
            if (explorationState != null) explorationState.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (explorationState != null) explorationState.PhaseChanged -= HandlePhaseChanged;
        }

        public void AddLemonsUsed(int count)
        {
            LemonsUsed += Mathf.Max(0, count);
            ScoreChanged?.Invoke(Score);
        }

        public void AddSwordMiss()
        {
            SwordMisses++;
            ScoreChanged?.Invoke(Score);
        }

        public void ResetScore()
        {
            LemonsUsed = 0;
            SwordMisses = 0;
            ScoreChanged?.Invoke(Score);
        }

        void HandlePhaseChanged(ExplorationPhase phase)
        {
            if (phase == ExplorationPhase.Ready) ResetScore();
        }
    }
}
