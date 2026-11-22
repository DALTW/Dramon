using UnityEngine;

namespace Dramon
{
    /// <summary>레몬을 들고 안전한 칸을 골랐을 때: 레몬을 두고 칸을 밝힌다(0이면 연쇄). 쓴 레몬은 점수에 기록.</summary>
    public class LemonPlacer : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("레몬 소모량 설정")]
        [SerializeField] ToolSettings toolSettings;
        [Tooltip("쓴 레몬 수를 기록할 점수 관리자")]
        [SerializeField] ExplorationScore score;

        public void Place(Vector2Int pos)
        {
            if (board == null) return;
            board.RevealSafe(pos);
            if (score != null) score.AddLemonsUsed(toolSettings != null ? toolSettings.LemonsPerReveal : 1);
        }
    }
}
