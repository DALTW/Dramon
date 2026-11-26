using System;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// Image에 스프라이트 프레임 배열을 재생한다: 반복 / 한 번 재생 후 콜백 / 마지막 프레임에서 멈춤.
    /// 프레임마다 스프라이트 Pivot이 같은 자리(그림 칸의 아래 가운데)에 오도록 그림 위치를 정수 픽셀로 맞춘다.
    /// </summary>
    public class DragonFrameAnimator : MonoBehaviour
    {
        enum Mode { None, Loop, Once, Hold }

        [Header("연결")]
        [Tooltip("프레임을 그릴 Image (Preserve Aspect 켜 둠)")]
        [SerializeField] Image image;

        [Header("Pivot 맞추기")]
        [Tooltip("켜면 프레임마다 스프라이트 Pivot이 그림 칸의 아래 가운데에 오도록 그림 위치를 맞춰요 (프레임끼리 흔들림 보정)")]
        [SerializeField] bool alignToSpritePivot = true;

        Sprite[] frames;
        float fps;
        Mode mode;
        int index;
        float timer;
        Action onComplete;
        Vector2 restPosition;
        bool restRecorded;

        public bool IsPlaying => mode != Mode.None;

        void Awake() => RecordRest();

        void RecordRest()
        {
            if (restRecorded || image == null) return;
            restPosition = image.rectTransform.anchoredPosition;
            restRecorded = true;
        }

        /// <summary>프레임을 계속 반복한다.</summary>
        public void PlayLoop(Sprite[] newFrames, float framesPerSecond) => Begin(newFrames, framesPerSecond, Mode.Loop, null);

        /// <summary>프레임을 한 번 재생하고 onFinished를 부른다.</summary>
        public void PlayOnce(Sprite[] newFrames, float framesPerSecond, Action onFinished) => Begin(newFrames, framesPerSecond, Mode.Once, onFinished);

        /// <summary>프레임을 한 번 재생하고 마지막 프레임에서 멈춘 채 onFinished를 부른다.</summary>
        public void PlayAndHold(Sprite[] newFrames, float framesPerSecond, Action onFinished) => Begin(newFrames, framesPerSecond, Mode.Hold, onFinished);

        /// <summary>재생을 멈추고 그림 한 장을 보여 준다 (null이면 그림을 숨김).</summary>
        public void ShowStill(Sprite sprite)
        {
            Stop();
            SetSprite(sprite);
        }

        /// <summary>재생만 멈춘다 (지금 그림은 그대로).</summary>
        public void Stop()
        {
            mode = Mode.None;
            onComplete = null;
        }

        void Begin(Sprite[] newFrames, float framesPerSecond, Mode newMode, Action onFinished)
        {
            frames = newFrames;
            fps = Mathf.Max(0.1f, framesPerSecond);
            mode = frames != null && frames.Length > 0 ? newMode : Mode.None;
            onComplete = onFinished;
            index = 0;
            timer = 0f;
            if (mode == Mode.None)
            {
                onComplete = null;
                onFinished?.Invoke();
                return;
            }
            SetSprite(frames[0]);
        }

        void Update()
        {
            if (mode == Mode.None) return;
            timer += Time.deltaTime;
            float frameTime = 1f / fps;
            while (timer >= frameTime && mode != Mode.None)
            {
                timer -= frameTime;
                Advance();
            }
        }

        void Advance()
        {
            int next = index + 1;
            if (next < frames.Length)
            {
                index = next;
                SetSprite(frames[index]);
                return;
            }
            if (mode == Mode.Loop)
            {
                index = 0;
                SetSprite(frames[0]);
                return;
            }
            // 한 번 재생 끝: Hold면 마지막 프레임 그대로
            var done = onComplete;
            mode = Mode.None;
            onComplete = null;
            done?.Invoke();
        }

        void SetSprite(Sprite sprite)
        {
            if (image == null) return;
            RecordRest();
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.rectTransform.anchoredPosition = restPosition + PivotOffset(sprite);
        }

        /// <summary>스프라이트 Pivot이 그림 칸 아래 가운데에 오도록 옮길 거리 (정수 픽셀).</summary>
        Vector2 PivotOffset(Sprite sprite)
        {
            if (!alignToSpritePivot || sprite == null) return Vector2.zero;
            var rt = image.rectTransform;
            var r = rt.rect;
            var size = sprite.rect.size;
            if (size.x <= 0f || size.y <= 0f || r.width <= 0f || r.height <= 0f) return Vector2.zero;

            // Preserve Aspect일 때 Image가 실제로 그리는 영역 (Image와 같은 계산)
            float k = image.preserveAspect ? Mathf.Min(r.width / size.x, r.height / size.y) : 0f;
            var drawn = image.preserveAspect ? size * k : r.size;
            var scale = image.preserveAspect ? new Vector2(k, k) : new Vector2(r.width / size.x, r.height / size.y);
            var drawnMin = r.min + (r.size - drawn) * rt.pivot;

            var pivotPoint = drawnMin + Vector2.Scale(sprite.pivot, scale);
            var target = new Vector2(r.center.x, r.yMin);
            var offset = Vector2.Scale(target - pivotPoint, rt.localScale); // 크기 배율만큼 부모 기준 거리로
            return new Vector2(Mathf.Round(offset.x), Mathf.Round(offset.y));
        }
    }
}
