using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>놓은 레몬 수와 남은 굴 수를 집계하고, 레몬 표시 단계가 끝났는지 확인한다.</summary>
    public class NestMarkCounter : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;

        [Header("완료 조건")]
        [Tooltip("켜면 굴을 모두 레몬으로 표시하고 안전한 칸도 모두 열어야 단계가 끝나요. 끄면 굴만 다 표시하면 끝")]
        [SerializeField] bool requireAllSafeTilesOpened = true;

        /// <summary>레몬 표시 단계 완료 조건을 만족한 순간</summary>
        public event Action MarkingCompleted;
        /// <summary>놓은 레몬 수가 바뀌었을 때 (인자: 남은 굴 수)</summary>
        public event Action<int> CountChanged;

        public int TotalNests => board != null ? board.NestCount : 0;
        public int LemonsPlaced => board != null ? board.LemonCount : 0;
        public int NestsLeft => TotalNests - LemonsPlaced;

        public bool IsComplete =>
            board != null && board.NestsPlaced &&
            LemonsPlaced >= TotalNests &&
            (!requireAllSafeTilesOpened || board.AllSafeRevealed);

        int lastLemons = -1;
        bool completedRaised;

        void OnEnable()
        {
            if (board != null) board.BoardChanged += HandleBoardChanged;
        }

        void OnDisable()
        {
            if (board != null) board.BoardChanged -= HandleBoardChanged;
        }

        void HandleBoardChanged()
        {
            if (LemonsPlaced != lastLemons)
            {
                lastLemons = LemonsPlaced;
                CountChanged?.Invoke(NestsLeft);
            }

            if (!board.NestsPlaced) { completedRaised = false; return; } // 보드 리셋
            if (!completedRaised && IsComplete)
            {
                completedRaised = true;
                MarkingCompleted?.Invoke();
            }
        }
    }
}
