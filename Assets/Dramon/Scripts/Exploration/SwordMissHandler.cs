using UnityEngine;

namespace Dramon
{
    /// <summary>검을 들고 안전한 칸을 골랐을 때(헛손질): 칸은 밝혀지지만 점수가 깎인다.</summary>
    public class SwordMissHandler : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("헛손질 횟수를 기록할 점수 관리자")]
        [SerializeField] ExplorationScore score;

        public void Handle(Vector2Int pos)
        {
            if (board == null) return;
            board.RevealSafe(pos);
            if (score != null) score.AddSwordMiss();
        }
    }
}
