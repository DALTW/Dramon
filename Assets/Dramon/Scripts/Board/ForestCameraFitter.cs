using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Dramon
{
    /// <summary>
    /// 숲 크기에 맞춰 칸 크기와 카메라 위치를 정한다 (스테이지 시작 때 StageProgress가 Fit 호출).
    /// 칸은 최대 크기(32px)에서 숲이 화면에 다 들어갈 때까지 줄이되 최소 크기(20px)까지만. 그래도 넘치면 ForestCameraPan으로 카메라를 움직여요.
    /// Main Camera에 붙인다. Fit 전 원래 값(픽셀 퍼펙트 PPU·카메라 위치)은 Restore로 되돌릴 수 있다.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ForestCameraFitter : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] ForestBoard board;
        [Tooltip("픽셀 퍼펙트 카메라 (Assets PPU를 칸 크기로 바꿔서 확대·축소해요). 비우면 카메라 Size로")]
        [SerializeField] PixelPerfectCamera pixelPerfect;

        [Header("칸 크기 (화면 기준 픽셀, 640×360 기준)")]
        [Tooltip("칸 최대 크기 (원래 그림 크기)")]
        [Min(4)] [SerializeField] int maxTilePixels = 32;
        [Tooltip("칸 최소 크기. 이보다 작아져야 들어가면 이 크기로 두고 카메라를 움직여요")]
        [Min(4)] [SerializeField] int minTilePixels = 20;

        [Header("숲이 들어갈 화면 영역 (640×360 기준 픽셀)")]
        [Tooltip("기준 해상도 (픽셀 퍼펙트 카메라가 있으면 그 값을 써요)")]
        [SerializeField] Vector2Int referenceResolution = new Vector2Int(640, 360);
        [Tooltip("위쪽 HUD 막대가 차지하는 높이")]
        [Min(0)] [SerializeField] int topReserved = 40;
        [Tooltip("아래쪽 여백")]
        [Min(0)] [SerializeField] int bottomReserved = 8;
        [Tooltip("왼쪽·오른쪽 여백 (한쪽)")]
        [Min(0)] [SerializeField] int sideReserved = 8;

        Camera cam;
        bool saved;
        int originalPPU;
        float originalSize;
        Vector3 originalPosition;

        /// <summary>지금 칸 크기(픽셀)</summary>
        public int TilePixels { get; private set; }
        /// <summary>숲이 화면 영역보다 커서 카메라를 움직여야 하는지</summary>
        public bool Overflows { get; private set; }
        /// <summary>카메라 중심이 움직일 수 있는 범위 (월드). 넘치지 않으면 크기 0</summary>
        public Rect PanLimits { get; private set; }

        Vector2Int RefRes => pixelPerfect != null ? new Vector2Int(pixelPerfect.refResolutionX, pixelPerfect.refResolutionY) : referenceResolution;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (pixelPerfect == null) pixelPerfect = GetComponent<PixelPerfectCamera>();
            Save();
        }

        void Save()
        {
            if (saved) return;
            saved = true;
            originalPPU = pixelPerfect != null ? pixelPerfect.assetsPPU : 32;
            originalSize = cam != null ? cam.orthographicSize : 5f;
            originalPosition = transform.position;
        }

        /// <summary>지금 숲 크기에 맞춰 칸 크기·카메라 위치를 정한다</summary>
        public void Fit()
        {
            if (board == null) return;
            if (cam == null) cam = GetComponent<Camera>();
            Save();
            var res = RefRes;
            float availW = Mathf.Max(1, res.x - sideReserved * 2);
            float availH = Mathf.Max(1, res.y - topReserved - bottomReserved);
            int w = Mathf.Max(1, board.Width), h = Mathf.Max(1, board.Height);
            int px = Mathf.FloorToInt(Mathf.Min(availW / w, availH / h));
            TilePixels = Mathf.Clamp(px, Mathf.Min(minTilePixels, maxTilePixels), maxTilePixels);

            // 1 월드 단위 = 칸 간격이 TilePixels가 되도록
            float spacing = Mathf.Max(0.01f, board.TileSpacing);
            int ppu = Mathf.Max(1, Mathf.RoundToInt(TilePixels / spacing));
            if (pixelPerfect != null) pixelPerfect.assetsPPU = ppu;
            else cam.orthographicSize = res.y * 0.5f / ppu;

            // 숲을 화면 영역 가운데에 (위 HUD만큼 카메라를 위로)
            var bounds = board.WorldBounds;
            float yOffset = (topReserved - bottomReserved) * 0.5f / ppu;
            var center = new Vector3(bounds.center.x, bounds.center.y + yOffset, originalPosition.z);
            transform.position = center;

            // 넘치면 카메라가 숲 끝까지 갈 수 있는 범위
            float halfW = availW * 0.5f / ppu, halfH = availH * 0.5f / ppu;
            float extraX = Mathf.Max(0f, bounds.extents.x - halfW);
            float extraY = Mathf.Max(0f, bounds.extents.y - halfH);
            Overflows = extraX > 0.001f || extraY > 0.001f;
            PanLimits = new Rect(center.x - extraX, center.y - extraY, extraX * 2f, extraY * 2f);
        }

        /// <summary>Scene에 저장된 원래 값으로 되돌리기</summary>
        public void Restore()
        {
            if (!saved) return;
            if (pixelPerfect != null) pixelPerfect.assetsPPU = originalPPU;
            if (cam != null) cam.orthographicSize = originalSize;
            transform.position = originalPosition;
        }

        void OnDisable() => Restore();
    }
}
