using UnityEngine;
using UnityEngine.InputSystem;

namespace Dramon
{
    /// <summary>숲이 화면보다 클 때만 카메라를 움직인다: 방향키·WASD, 마우스 휠(위아래), Shift+휠(좌우). Main Camera에 붙인다.</summary>
    [RequireComponent(typeof(ForestCameraFitter))]
    public class ForestCameraPan : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] ForestCameraFitter fitter;
        [Tooltip("숲 화면일 때만 움직여요 (결투·상점·창이 떠 있으면 멈춤)")]
        [SerializeField] GamePhaseController phaseController;

        [Header("조작")]
        [Tooltip("키보드로 움직이는 빠르기 (초당 칸 수)")]
        [Min(0f)] [SerializeField] float keySpeed = 8f;
        [Tooltip("휠 한 칸에 움직이는 칸 수")]
        [Min(0f)] [SerializeField] float wheelStep = 1f;
        [Tooltip("켜면 방향키, WASD로 움직여요")]
        [SerializeField] bool useKeyboard = true;
        [Tooltip("켜면 마우스 휠로 움직여요")]
        [SerializeField] bool useWheel = true;

        void Reset() => fitter = GetComponent<ForestCameraFitter>();

        void LateUpdate()
        {
            if (fitter == null || !fitter.Overflows) return;
            if (phaseController != null && !CanPan(phaseController.Phase)) return;
            var move = Vector2.zero;
            var kb = Keyboard.current;
            if (useKeyboard && kb != null)
            {
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) move.x -= 1;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) move.x += 1;
                if (kb.downArrowKey.isPressed || kb.sKey.isPressed) move.y -= 1;
                if (kb.upArrowKey.isPressed || kb.wKey.isPressed) move.y += 1;
                move *= keySpeed * Time.unscaledDeltaTime;
            }
            var mouse = Mouse.current;
            if (useWheel && mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > 0.01f)
                {
                    float step = Mathf.Sign(wheel) * wheelStep;
                    bool shift = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
                    if (shift) move.x -= step; else move.y += step;
                }
            }
            if (move == Vector2.zero) return;
            var p = transform.position;
            var lim = fitter.PanLimits;
            p.x = Mathf.Clamp(p.x + move.x, lim.xMin, lim.xMax);
            p.y = Mathf.Clamp(p.y + move.y, lim.yMin, lim.yMax);
            transform.position = p;
        }

        static bool CanPan(GamePhase phase) => phase == GamePhase.LemonMarking || phase == GamePhase.SwordHunt;
    }
}
