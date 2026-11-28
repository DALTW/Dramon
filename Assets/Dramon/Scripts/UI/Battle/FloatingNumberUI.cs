using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>피해·방어·회복 숫자를 대상 위로 띄운다 (FloatingNumber Prefab을 만들어 떠오르게 한 뒤 사라짐)</summary>
    public class FloatingNumberUI : MonoBehaviour
    {
        public enum Kind { Damage, Block, Heal, Status }

        [Header("연결")]
        [Tooltip("숫자 한 개 Prefab")]
        [SerializeField] FloatingNumber prefab;
        [Tooltip("숫자가 놓일 곳 (결투 화면 전체를 덮는 오브젝트)")]
        [SerializeField] RectTransform container;
        [Tooltip("플레이어 쪽 숫자가 뜰 자리 (PlayerSide/FloatAnchor)")]
        [SerializeField] RectTransform playerAnchor;
        [Tooltip("드래곤 쪽 숫자가 뜰 자리 (DragonSide/FloatAnchor)")]
        [SerializeField] RectTransform dragonAnchor;

        [Header("색")]
        [SerializeField] Color damageColor = new Color32(0xE0, 0x40, 0x30, 0xFF);
        [SerializeField] Color blockColor = new Color32(0x5A, 0x9C, 0xE0, 0xFF);
        [SerializeField] Color healColor = new Color32(0x58, 0xC8, 0x48, 0xFF);
        [SerializeField] Color statusColor = new Color32(0xC0, 0x80, 0xE0, 0xFF);

        [Header("움직임")]
        [Tooltip("같은 자리에 연달아 뜰 때 옆으로 벌리는 거리(px)")]
        [SerializeField] float spread = 10f;

        int counter;

        public void OnPlayer(string text, Kind kind) => Spawn(playerAnchor, text, kind);
        public void OnDragon(string text, Kind kind) => Spawn(dragonAnchor, text, kind);

        void Spawn(RectTransform anchor, string text, Kind kind)
        {
            if (prefab == null || container == null || anchor == null || !isActiveAndEnabled) return;
            var n = Instantiate(prefab, container);
            var canvas = container.GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var screen = RectTransformUtility.WorldToScreenPoint(cam, anchor.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screen, cam, out var local);
            counter++;
            local.x += ((counter % 3) - 1) * spread;
            n.Play(local, text, ColorOf(kind));
        }

        Color ColorOf(Kind kind)
        {
            switch (kind)
            {
                case Kind.Block: return blockColor;
                case Kind.Heal: return healColor;
                case Kind.Status: return statusColor;
                default: return damageColor;
            }
        }
    }
}
