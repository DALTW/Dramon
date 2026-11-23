using UnityEngine;

namespace Dramon
{
    /// <summary>레몬 표시 단계의 경과 시간을 잰다. 첫 칸을 열 때 시작, 단계 완료·게임 오버 때 멈춤.</summary>
    public class MarkingTimer : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("첫 칸이 열리는 순간을 알려주는 숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("단계가 끝나거나 다시 시작하는 순간을 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        public bool IsRunning { get; private set; }

        float startTime;
        float stoppedElapsed;

        /// <summary>경과 시간(초)</summary>
        public float Elapsed => IsRunning ? Time.time - startTime : stoppedElapsed;

        void OnEnable()
        {
            if (board != null) board.FirstRevealed += StartTimer;
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (board != null) board.FirstRevealed -= StartTimer;
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        void StartTimer()
        {
            startTime = Time.time;
            stoppedElapsed = 0f;
            IsRunning = true;
        }

        void StopTimer()
        {
            if (!IsRunning) return;
            stoppedElapsed = Time.time - startTime;
            IsRunning = false;
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.MarkingComplete:
                case GamePhase.GameOver:
                    StopTimer();
                    break;
                case GamePhase.LemonMarking: // 다시 시작
                    IsRunning = false;
                    stoppedElapsed = 0f;
                    break;
            }
        }
    }
}
