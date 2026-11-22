using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Dramon
{
    /// <summary>
    /// 마우스 입력을 숲 칸 명령으로 바꾼다.
    /// 좌클릭 = 들고 있는 도구로 칸 고르기(TileActionResolver), 우클릭 = 경고 표식, 마우스 올림 = Hover 표시.
    /// </summary>
    public class ForestInput : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("마우스 위치를 월드 좌표로 바꿀 카메라")]
        [SerializeField] Camera targetCamera;
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("도구 + 칸 종류로 결과를 정하는 컴포넌트")]
        [SerializeField] TileActionResolver actionResolver;

        [Header("조작")]
        [Tooltip("켜면 이미 밝혀진 숫자 칸을 좌클릭했을 때 주변 칸을 들고 있는 도구로 한꺼번에 골라요 (휠 클릭은 항상 동작)")]
        [SerializeField] bool actAroundOnLeftClick = true;
        [Tooltip("켜면 가려진 칸에 마우스를 올렸을 때 Hover 스프라이트로 바뀌어요")]
        [SerializeField] bool showHover = true;
        [Tooltip("칸을 감지할 물리 레이어 (ForestTile Prefab의 BoxCollider2D가 있는 레이어)")]
        [SerializeField] LayerMask tileLayers = ~0;

        ForestTile hoveredTile;

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 screenPos = mouse.position.ReadValue();
            bool left = mouse.leftButton.wasReleasedThisFrame;
            bool right = mouse.rightButton.wasPressedThisFrame;
            bool middle = mouse.middleButton.wasReleasedThisFrame;

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            var tile = overUI || board == null || board.IsLocked ? null : FindTile(screenPos);

            UpdateHover(tile);
            if (tile == null || actionResolver == null) return;

            var pos = tile.GridPosition;
            if (right) board.ToggleMarker(pos);
            else if (middle) actionResolver.ResolveAround(pos);
            else if (left)
            {
                if (board.IsRevealed(pos)) { if (actAroundOnLeftClick) actionResolver.ResolveAround(pos); }
                else actionResolver.Resolve(pos);
            }
        }

        void UpdateHover(ForestTile tile)
        {
            if (!showHover) tile = null;
            if (tile == hoveredTile) return;
            if (hoveredTile != null) hoveredTile.SetHovered(false);
            hoveredTile = tile;
            if (hoveredTile != null) hoveredTile.SetHovered(true);
        }

        ForestTile FindTile(Vector2 screenPos)
        {
            if (targetCamera == null) return null;
            Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -targetCamera.transform.position.z));
            var hit = Physics2D.OverlapPoint(world, tileLayers);
            var tile = hit != null ? hit.GetComponent<ForestTile>() : null;
            return tile != null && tile.Board == board ? tile : null;
        }
    }
}
