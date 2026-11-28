using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// UI Image에 프레임 그림을 차례로 보여 주는 재생기 (분노·숨결 이펙트 등).
    /// 평소에는 그림을 숨겨 두고, Play를 부르면 한 번(또는 반복) 보여 준 뒤 다시 숨긴다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIFramePlayer : MonoBehaviour
    {
        [Tooltip("프레임을 보여 줄 Image (같은 오브젝트)")]
        [SerializeField] Image image;
        [Tooltip("차례로 보여 줄 그림 (시트를 자른 프레임, 위에서부터 순서대로)")]
        [SerializeField] Sprite[] frames = new Sprite[0];
        [Tooltip("1초에 보여 줄 프레임 수")]
        [Min(1f)]
        [SerializeField] float fps = 10f;
        [Tooltip("켜면 마지막 프레임 뒤 처음부터 반복해요. 끄면 한 번 보여 주고 숨겨요")]
        [SerializeField] bool loop;

        public bool IsPlaying { get; private set; }

        void Reset() => image = GetComponent<Image>();

        void Awake()
        {
            if (image == null) image = GetComponent<Image>();
            if (image != null) image.enabled = false;
        }

        void OnDisable()
        {
            IsPlaying = false;
            if (image != null) image.enabled = false;
        }

        /// <summary>그림 색 그대로 재생</summary>
        public void Play() => Play(Color.white);

        /// <summary>tint 색을 곱해서 재생 (흰 그림이면 그 색이 돼요)</summary>
        public void Play(Color tint)
        {
            StopAllCoroutines();
            if (!isActiveAndEnabled || image == null || frames == null || frames.Length == 0) return;
            image.color = tint;
            StartCoroutine(Run());
        }

        public void Stop()
        {
            StopAllCoroutines();
            IsPlaying = false;
            if (image != null) image.enabled = false;
        }

        IEnumerator Run()
        {
            IsPlaying = true;
            image.enabled = true;
            var wait = new WaitForSeconds(1f / fps);
            do
            {
                foreach (var f in frames)
                {
                    if (f == null) continue;
                    image.sprite = f;
                    yield return wait;
                }
            } while (loop);
            image.enabled = false;
            IsPlaying = false;
        }
    }
}
