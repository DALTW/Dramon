using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>SpriteRenderer 하나에 스프라이트 프레임 여러 장을 순서대로 보여준다. (둥지 결투 중 깜빡임, 이펙트 등)</summary>
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("프레임을 바꿔 끼울 SpriteRenderer")]
        [SerializeField] SpriteRenderer target;

        [Header("재생")]
        [Tooltip("초당 프레임 수. 클수록 빨리 바뀌어요")]
        [SerializeField, Min(0.1f)] float framesPerSecond = 4f;

        public event Action Finished;
        public bool IsPlaying { get; private set; }

        Sprite[] frames;
        bool loop;
        float elapsed;

        void Reset()
        {
            target = GetComponent<SpriteRenderer>();
        }

        public void Play(Sprite[] sequence, bool loopPlayback = true)
        {
            if (sequence == null || sequence.Length == 0 || target == null) { Stop(); return; }
            frames = sequence;
            loop = loopPlayback;
            elapsed = 0f;
            IsPlaying = true;
            target.sprite = frames[0];
        }

        public void Stop()
        {
            IsPlaying = false;
            frames = null;
        }

        void Update()
        {
            if (!IsPlaying) return;
            elapsed += Time.deltaTime;
            int index = Mathf.FloorToInt(elapsed * framesPerSecond);
            if (loop) index %= frames.Length;
            else if (index >= frames.Length)
            {
                IsPlaying = false;
                Finished?.Invoke();
                return;
            }
            target.sprite = frames[index];
        }
    }
}
