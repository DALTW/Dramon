using System;
using UnityEngine;

namespace Dramon
{
    public enum ExplorationPhase { Ready, Exploring, Dueling, Cleared, GameOver }

    /// <summary>숲 탐험의 진행 단계(대기·탐험 중·결투 중·클리어·게임 오버)를 관리한다.</summary>
    public class ExplorationState : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("탐험할 숲 보드")]
        [SerializeField] ForestBoard board;

        [Header("게임 오버 연출")]
        [Tooltip("켜면 게임 오버 때 아직 드러나지 않은 둥지를 모두 보여줘요")]
        [SerializeField] bool revealAllNestsOnGameOver = true;

        public event Action<ExplorationPhase> PhaseChanged;

        public ExplorationPhase Phase { get; private set; } = ExplorationPhase.Ready;
        public ForestBoard Board => board;

        void OnEnable()
        {
            if (board == null) return;
            board.FirstRevealed += HandleFirstRevealed;
            board.Cleared += HandleCleared;
        }

        void OnDisable()
        {
            if (board == null) return;
            board.FirstRevealed -= HandleFirstRevealed;
            board.Cleared -= HandleCleared;
        }

        /// <summary>숲을 처음 상태로 되돌린다. (Restart 버튼에 연결됨)</summary>
        public void Restart()
        {
            if (board != null) board.ResetBoard();
            SetPhase(ExplorationPhase.Ready);
        }

        /// <summary>결투 시작: 보드 입력을 막는다.</summary>
        public void EnterDuel()
        {
            if (board != null) board.SetLocked(true);
            SetPhase(ExplorationPhase.Dueling);
        }

        /// <summary>결투에서 이기고 탐험으로 돌아온다.</summary>
        public void ReturnFromDuel()
        {
            if (board != null) board.SetLocked(false);
            SetPhase(ExplorationPhase.Exploring);
        }

        /// <summary>게임 오버 (결투 패배).</summary>
        public void TriggerGameOver()
        {
            if (board != null)
            {
                board.SetLocked(true);
                if (revealAllNestsOnGameOver) board.RevealAllNests();
            }
            SetPhase(ExplorationPhase.GameOver);
        }

        void HandleFirstRevealed() => SetPhase(ExplorationPhase.Exploring);
        void HandleCleared() => SetPhase(ExplorationPhase.Cleared);

        void SetPhase(ExplorationPhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }
    }
}
