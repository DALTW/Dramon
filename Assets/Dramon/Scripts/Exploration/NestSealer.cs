using UnityEngine;

namespace Dramon
{
    /// <summary>레몬을 들고 둥지를 골랐을 때: 드래곤이 피해 가서 결투 없이 둥지가 봉인된다(보상 없음). 쓴 레몬은 점수에 기록.</summary>
    public class NestSealer : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("레몬 소모량 설정")]
        [SerializeField] ToolSettings toolSettings;
        [Tooltip("쓴 레몬 수를 기록할 점수 관리자")]
        [SerializeField] ExplorationScore score;

        public void Seal(Vector2Int pos)
        {
            if (board == null) return;
            board.SealNest(pos);
            if (score != null) score.AddLemonsUsed(toolSettings != null ? toolSettings.LemonsPerSeal : 1);
        }
    }
}
