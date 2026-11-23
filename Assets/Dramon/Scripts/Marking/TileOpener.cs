using UnityEngine;

namespace Dramon
{
    /// <summary>왼쪽 클릭으로 칸을 연다 (지뢰찾기와 같음). 굴을 직접 열면 게임 오버.</summary>
    public class TileOpener : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("게임 오버를 알릴 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        /// <summary>가려진 칸 하나를 연다. 첫 번째로 여는 칸과 주변은 항상 안전.</summary>
        public void Open(Vector2Int pos)
        {
            if (board == null || board.IsLocked || !board.IsUntouched(pos)) return;

            board.EnsureNestsPlaced(pos);
            if (board.IsNest(pos))
            {
                board.ActivateNest(pos); // Nest_Active 프레임 반복
                if (phaseController != null) phaseController.TriggerGameOver(GameOverReason.OpenedNest);
                return;
            }
            board.RevealSafe(pos);
        }

        /// <summary>열린 숫자 칸 주변의 레몬 수가 숫자와 같으면, 남은 주변 칸을 한꺼번에 연다.</summary>
        public void OpenAround(Vector2Int pos)
        {
            if (board == null || board.IsLocked || !board.CanActAround(pos)) return;
            foreach (var offset in ForestBoard.NeighborOffsets)
            {
                if (board.IsLocked) return;
                var np = pos + offset;
                if (board.IsUntouched(np)) Open(np);
            }
        }
    }
}
