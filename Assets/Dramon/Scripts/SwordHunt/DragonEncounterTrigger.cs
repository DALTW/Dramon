using UnityEngine;
using UnityEngine.InputSystem;

namespace Dramon
{
    /// <summary>검 사냥 단계에서 봉인된 둥지를 클릭하면(또는 올려 두기만 해도) 드래곤 등장을 시작한다.</summary>
    public class DragonEncounterTrigger : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("커서 아래의 봉인된 둥지를 알려주는 감지기")]
        [SerializeField] SealedNestHoverDetector hoverDetector;
        [Tooltip("등장·결투를 관리하는 결투 세션")]
        [SerializeField] DuelSession duelSession;

        [Header("조작")]
        [Tooltip("켜면 클릭하지 않고 검을 둥지 위에 올려 두기만 해도 등장이 시작돼요")]
        [SerializeField] bool startOnHover = false;
        [Tooltip("올려 두기만 해도 시작할 때, 둥지 위에 이 시간(초)만큼 머물러야 시작돼요 (실수 방지)")]
        [Min(0f)]
        [SerializeField] float hoverDelay = 0.3f;

        ForestTile hoveredTile;
        float hoverStartTime;

        void Update()
        {
            if (hoverDetector == null || duelSession == null) return;

            var tile = hoverDetector.HoveredNest;
            if (tile != hoveredTile)
            {
                hoveredTile = tile;
                hoverStartTime = Time.unscaledTime;
            }
            if (tile == null) return;

            var mouse = Mouse.current;
            bool clicked = mouse != null && mouse.leftButton.wasPressedThisFrame;
            bool hoveredLongEnough = startOnHover && Time.unscaledTime - hoverStartTime >= hoverDelay;
            if ((clicked || hoveredLongEnough) && duelSession.TryStart(tile))
                hoveredTile = null;
        }
    }
}
