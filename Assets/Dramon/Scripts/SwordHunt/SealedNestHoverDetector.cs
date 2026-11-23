using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Dramon
{
    /// <summary>
    /// 검 사냥 단계에서 마우스가 '봉인된 굴'(레몬으로 막아 두고 아직 결투하지 않은 굴) 위에 있는지 알려 준다.
    /// 커서 강조(PhaseCursorView)가 이 정보를 쓰고, 다음 단계의 결투 시작 입력도 이어서 쓸 수 있다.
    /// </summary>
    public class SealedNestHoverDetector : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("현재 단계를 알려주는 단계 관리자 (검 사냥 단계에서만 감지해요)")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("마우스 위치를 월드 좌표로 바꿀 카메라")]
        [SerializeField] Camera targetCamera;
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;

        [Header("감지")]
        [Tooltip("칸을 감지할 물리 레이어 (ForestTile Prefab의 BoxCollider2D가 있는 레이어)")]
        [SerializeField] LayerMask tileLayers = ~0;
        [Tooltip("켜면 마우스가 UI(버튼·창) 위에 있을 때는 굴을 감지하지 않아요")]
        [SerializeField] bool ignoreWhenOverUI = true;

        /// <summary>마우스 아래의 봉인된 굴 칸이 바뀌었을 때 (벗어나면 null)</summary>
        public event Action<ForestTile> HoveredNestChanged;

        /// <summary>지금 마우스 아래에 있는 봉인된 굴 칸 (없으면 null)</summary>
        public ForestTile HoveredNest { get; private set; }
        public bool IsHovering => HoveredNest != null;

        void Update() => SetHovered(FindSealedNest());

        void OnDisable() => SetHovered(null);

        ForestTile FindSealedNest()
        {
            if (phaseController == null || phaseController.Phase != GamePhase.SwordHunt) return null;
            if (board == null || targetCamera == null) return null;
            var mouse = Mouse.current;
            if (mouse == null) return null;
            if (ignoreWhenOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return null;

            Vector2 screenPos = mouse.position.ReadValue();
            Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -targetCamera.transform.position.z));
            var hit = Physics2D.OverlapPoint(world, tileLayers);
            var tile = hit != null ? hit.GetComponent<ForestTile>() : null;
            if (tile == null || tile.Board != board) return null;
            return board.IsSealedNest(tile.GridPosition) ? tile : null;
        }

        void SetHovered(ForestTile tile)
        {
            if (tile == HoveredNest) return;
            HoveredNest = tile;
            HoveredNestChanged?.Invoke(tile);
        }
    }
}
