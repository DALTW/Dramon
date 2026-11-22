using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// "들고 있는 도구 + 고른 칸 종류"를 보고 어떤 처리를 할지 정해서 담당 스크립트에 넘긴다.
    ///   레몬 + 안전한 칸 → LemonPlacer      레몬 + 둥지 → NestSealer
    ///   검   + 안전한 칸 → SwordMissHandler 검   + 둥지 → DuelManager
    /// 실제 처리 내용은 각 담당 스크립트에 있다.
    /// </summary>
    public class TileActionResolver : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("지금 들고 있는 도구를 알려주는 컴포넌트")]
        [SerializeField] ToolSwitcher toolSwitcher;

        [Header("결과별 담당")]
        [Tooltip("레몬 + 안전한 칸: 레몬을 두고 칸을 밝힘")]
        [SerializeField] LemonPlacer lemonPlacer;
        [Tooltip("레몬 + 둥지: 결투 없이 봉인")]
        [SerializeField] NestSealer nestSealer;
        [Tooltip("검 + 안전한 칸: 칸은 밝혀지지만 점수 감점")]
        [SerializeField] SwordMissHandler swordMissHandler;
        [Tooltip("검 + 둥지: 드래곤과 결투 시작")]
        [SerializeField] DuelManager duelManager;

        /// <summary>가려진 칸 하나를 들고 있는 도구로 고른다.</summary>
        public void Resolve(Vector2Int pos)
        {
            if (board == null || board.IsLocked || !board.IsUntouched(pos)) return;

            board.EnsureNestsPlaced(pos); // 첫 행동은 항상 안전한 칸
            bool isNest = board.IsNest(pos);
            var tool = toolSwitcher != null ? toolSwitcher.CurrentTool : PlayerTool.Lemon;

            switch (tool)
            {
                case PlayerTool.Lemon when isNest: nestSealer?.Seal(pos); break;
                case PlayerTool.Lemon: lemonPlacer?.Place(pos); break;
                case PlayerTool.Sword when isNest: duelManager?.StartDuel(pos); break;
                case PlayerTool.Sword: swordMissHandler?.Handle(pos); break;
            }
        }

        /// <summary>밝혀진 숫자 칸 주변의 확인된 둥지 수가 숫자와 같으면, 남은 주변 칸을 모두 들고 있는 도구로 고른다.</summary>
        public void ResolveAround(Vector2Int pos)
        {
            if (board == null || board.IsLocked || !board.CanActAround(pos)) return;
            foreach (var offset in ForestBoard.NeighborOffsets)
            {
                if (board.IsLocked) return; // 결투가 시작되면 멈춤
                var np = pos + offset;
                if (board.IsUntouched(np)) Resolve(np);
            }
        }
    }
}
