using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 보스 붉은 기운: 드래곤 뒤의 그림이 천천히 밝아졌다 어두워진다. 분노하면 더 진하고 빠르게.
    /// 그림은 Image의 Source Image (FX_BossAura). Color는 그림에 곱해지는 색이에요.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BossAuraView : MonoBehaviour
    {
        [SerializeField] Image image;

        [Header("평소")]
        [SerializeField] Color color = new Color(0.9f, 0.1f, 0.05f, 1f);
        [Range(0f, 1f)] [SerializeField] float minAlpha = 0.15f;
        [Range(0f, 1f)] [SerializeField] float maxAlpha = 0.45f;
        [Tooltip("한 번 밝아졌다 어두워지는 시간(초)")]
        [Min(0.1f)] [SerializeField] float period = 1.6f;

        [Header("분노")]
        [Range(0f, 1f)] [SerializeField] float enragedMinAlpha = 0.35f;
        [Range(0f, 1f)] [SerializeField] float enragedMaxAlpha = 0.8f;
        [Min(0.1f)] [SerializeField] float enragedPeriod = 0.7f;
        [Tooltip("분노하면 이 배율로 커져요")]
        [Min(0.1f)] [SerializeField] float enragedScale = 1.15f;

        [Header("임시 에셋 표시")]
        [TextArea(1, 2)]
        [SerializeField] string tempAssetNote = "임시: Unity 기본 원(Knob) 그림을 붉게 칠해서 씀. 붉은 기운 이펙트 그림으로 교체";

        bool enraged;
        float t;
        Vector3 baseScale = Vector3.one;

        public string TempAssetNote => tempAssetNote;

        void Reset() => image = GetComponent<Image>();

        void Awake()
        {
            if (image == null) image = GetComponent<Image>();
            baseScale = transform.localScale; // 처음에는 Scene에서 꺼 둔 상태 (설치 메뉴가 꺼 둠)
        }

        public void Show(bool enrage)
        {
            enraged = enrage;
            if (image != null) image.enabled = true;
            transform.localScale = baseScale * (enraged ? enragedScale : 1f);
        }

        public void Hide()
        {
            enraged = false;
            if (image != null) image.enabled = false;
            transform.localScale = baseScale;
        }

        void Update()
        {
            if (image == null || !image.enabled) return;
            t += Time.deltaTime;
            float p = enraged ? enragedPeriod : period;
            float k = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f / p);
            float a = Mathf.Lerp(enraged ? enragedMinAlpha : minAlpha, enraged ? enragedMaxAlpha : maxAlpha, k);
            image.color = new Color(color.r, color.g, color.b, a);
        }
    }
}
