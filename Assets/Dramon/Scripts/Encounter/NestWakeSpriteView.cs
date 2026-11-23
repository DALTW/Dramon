using UnityEngine;

namespace Dramon
{
    /// <summary>결투 직전, 둥지 위에 깨어나는 드래곤(빛나는 수면 그림)을 잠깐 보여 준다.</summary>
    public class NestWakeSpriteView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("깨어나는 드래곤을 그릴 SpriteRenderer")]
        [SerializeField] SpriteRenderer spriteRenderer;

        [Header("배치")]
        [Tooltip("둥지 칸 중심에서 얼마나 떨어진 곳에 그릴지 (월드 단위)")]
        [SerializeField] Vector3 offset = new Vector3(0f, 0.25f, 0f);
        [Tooltip("그림 크기 배율 (1 = 원래 크기 88×64)")]
        [Min(0.01f)]
        [SerializeField] float scale = 0.5f;

        void Awake() => Hide();

        public void Show(Sprite sprite, Vector3 nestWorldPosition)
        {
            if (spriteRenderer == null || sprite == null) return;
            transform.position = nestWorldPosition + offset;
            transform.localScale = Vector3.one * scale;
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = true;
        }

        public void Hide()
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }
    }
}
