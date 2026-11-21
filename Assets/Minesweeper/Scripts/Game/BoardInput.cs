using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Dramon.Minesweeper
{
    /// <summary>마우스 클릭을 칸에 대한 명령(열기·깃발·주변 열기)으로 바꿔 현재 보드에 전달한다.</summary>
    public class BoardInput : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("마우스 위치를 월드 좌표로 바꿀 카메라")]
        [SerializeField] Camera targetCamera;
        [Tooltip("현재 플레이 중인 보드를 알려주는 게임 상태 관리자")]
        [SerializeField] GameStateManager gameState;

        [Header("조작")]
        [Tooltip("켜면 이미 열린 숫자 칸을 좌클릭했을 때 주변 칸을 한꺼번에 열어요 (휠 클릭은 항상 동작)")]
        [SerializeField] bool chordOnLeftClick = true;
        [Tooltip("칸을 감지할 물리 레이어 (칸 Prefab의 BoxCollider2D가 있는 레이어)")]
        [SerializeField] LayerMask cellLayers = ~0;

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null) return;
            bool left = mouse.leftButton.wasReleasedThisFrame;
            bool right = mouse.rightButton.wasPressedThisFrame;
            bool middle = mouse.middleButton.wasReleasedThisFrame;
            Vector2 screenPos = mouse.position.ReadValue();
#else
            bool left = Input.GetMouseButtonUp(0);
            bool right = Input.GetMouseButtonDown(1);
            bool middle = Input.GetMouseButtonUp(2);
            Vector2 screenPos = Input.mousePosition;
#endif
            if (!left && !right && !middle) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var cell = FindCell(screenPos);
            if (cell == null) return;
            var board = gameState != null ? gameState.CurrentBoard : cell.Board;
            if (board == null || cell.Board != board) return;

            var pos = cell.GridPosition;
            if (right) board.ToggleFlag(pos);
            else if (middle) board.Chord(pos);
            else if (board.IsRevealed(pos)) { if (chordOnLeftClick) board.Chord(pos); }
            else board.Reveal(pos);
        }

        CellView FindCell(Vector2 screenPos)
        {
            if (targetCamera == null) return null;
            Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -targetCamera.transform.position.z));
            var hit = Physics2D.OverlapPoint(world, cellLayers);
            return hit != null ? hit.GetComponent<CellView>() : null;
        }
    }
}
