using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Dramon
{
    /// <summary>
    /// 레몬 표시 단계의 마우스 입력. 왼쪽 클릭 = 칸 열기(TileOpener), 오른쪽 클릭 = 레몬 놓기(LemonMarker),
    /// 마우스를 올린 가려진 칸은 Hover로 강조.
    /// </summary>
    public class MarkingInput : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("마우스 위치를 월드 좌표로 바꿀 카메라")]
        [SerializeField] Camera targetCamera;
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("왼쪽 클릭: 칸 열기")]
        [SerializeField] TileOpener tileOpener;
        [Tooltip("오른쪽 클릭: 레몬 놓기")]
        [SerializeField] LemonMarker lemonMarker;

        [Header("조작")]
        [Tooltip("켜면 열린 숫자 칸을 왼쪽 클릭했을 때, 주변 레몬 수가 숫자와 같으면 남은 주변 칸을 한꺼번에 열어요 (휠 클릭은 항상 동작)")]
        [SerializeField] bool openAroundOnLeftClick = true;
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
            if (tile == null) return;

            var pos = tile.GridPosition;
            if (right) { if (lemonMarker != null) lemonMarker.Place(pos); }
            else if (middle) { if (tileOpener != null) tileOpener.OpenAround(pos); }
            else if (left && tileOpener != null)
            {
                if (board.IsRevealed(pos)) { if (openAroundOnLeftClick) tileOpener.OpenAround(pos); }
                else tileOpener.Open(pos);
            }
        }

        void OnDisable() => UpdateHover(null);

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
