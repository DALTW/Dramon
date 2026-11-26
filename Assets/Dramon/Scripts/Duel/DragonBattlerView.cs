using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Dramon
{
    /// <summary>
    /// 결투 화면의 드래곤 그림. 비행(반복) / 공격 / 피격 / 패배 모션, 등장할 때 어두운 색 → 원래 색, 울음소리.
    /// 모션 프레임이 있으면 DragonFrameAnimator로 재생하고 DragonHoverMotion으로 위아래로 흔들며,
    /// 없으면 예전처럼 기본·공격·피격 그림 1장씩을 보여 준다. 위치·크기는 Scene 배치를 그대로 쓴다.
    /// </summary>
    public class DragonBattlerView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("드래곤 그림을 그릴 Image (Dragon/Motion/Sprite)")]
        [SerializeField] Image image;
        [Tooltip("울음소리를 낼 AudioSource (비워 두면 소리 없음)")]
        [SerializeField] AudioSource audioSource;
        [Tooltip("프레임 재생기 (Dragon/Motion/Sprite)")]
        [SerializeField] DragonFrameAnimator frameAnimator;
        [Tooltip("비행 중 위아래 흔들림 (Dragon)")]
        [SerializeField] DragonHoverMotion hover;

        [Header("크기")]
        [Tooltip("모든 드래곤 공통 그림 크기 배율 (1 = Dragon 영역에 꽉 맞춤). 드래곤별 결투 크기 배율과 곱해지고, 그림 칸의 아래 가운데를 기준으로 커지고 작아져요")]
        [Min(0.1f)]
        [SerializeField] float sizeMultiplier = 1f;

        [Header("연출")]
        [Tooltip("모션 프레임이 없을 때: 공격·피격 그림을 보여 준 뒤 대기 그림으로 돌아올 때까지의 시간(초)")]
        [Min(0f)]
        [SerializeField] float poseDuration = 0.4f;
        [Tooltip("등장 직후 어두운 색 (원래 색으로 돌아오기 전)")]
        [SerializeField] Color darkTint = new Color(0.25f, 0.25f, 0.32f, 1f);

        [Header("테스트 (에디터에서만)")]
        [Tooltip("켜면 Play 중 결투 화면에서 1 = 공격, 2 = 피격, 3 = 패배 모션을 볼 수 있어요")]
        [SerializeField] bool testHotkeys = true;

        DragonSpecies species;
        Coroutine revert;

        public DragonSpecies Species => species;

        void Awake() => ApplySize();

        void OnValidate() => ApplySize();

        /// <summary>이번 결투의 드래곤으로 바꾸고 비행(대기) 모션을 시작한다.</summary>
        public void SetSpecies(DragonSpecies newSpecies)
        {
            species = newSpecies;
            ApplySize();
            ShowIdle();
        }

        /// <summary>비행 모션 반복 (프레임이 없으면 기본 그림 1장).</summary>
        public void ShowIdle()
        {
            StopRevert();
            var fly = Frames(species != null ? species.FlyFrames : null);
            if (fly != null && frameAnimator != null)
            {
                frameAnimator.PlayLoop(fly, species.FlyFps);
                if (hover != null) hover.Play(species.HoverAmplitude, species.HoverPeriod);
                return;
            }
            if (hover != null) hover.Stop();
            ShowStill(species != null ? species.BattleIdle : null);
        }

        /// <summary>공격 모션 한 번 → 비행으로.</summary>
        public void ShowAttack() =>
            PlayOnceOrPose(species != null ? species.AttackFrames : null, species != null ? species.AttackFps : 10f,
                species != null ? species.BattleAttack : null);

        /// <summary>피격 모션 한 번 → 비행으로.</summary>
        public void ShowHurt() =>
            PlayOnceOrPose(species != null ? species.HurtFrames : null, species != null ? species.HurtFps : 8f,
                species != null ? species.BattleHurt : null);

        /// <summary>패배 모션: 쓰러지는 프레임을 재생하고 마지막 프레임(쓰러짐)에서 멈춘 뒤 onFinished.</summary>
        public void ShowDefeat(Action onFinished = null)
        {
            StopRevert();
            if (hover != null) hover.Stop();
            var defeat = Frames(species != null ? species.DefeatFrames : null);
            if (defeat != null && frameAnimator != null)
            {
                frameAnimator.PlayAndHold(defeat, species.DefeatFps, onFinished);
                return;
            }
            // 프레임이 없으면 피격 그림을 잠깐 보여 준 뒤 다음으로
            ShowStill(species != null ? species.BattleHurt : null);
            if (isActiveAndEnabled && poseDuration > 0f) revert = StartCoroutine(InvokeLater(onFinished));
            else onFinished?.Invoke();
        }

        /// <summary>모션을 모두 멈추고 처음 상태로 (숲으로 돌아갈 때·게임 오버·다시 시작).</summary>
        public void StopMotion()
        {
            StopRevert();
            if (frameAnimator != null) frameAnimator.Stop();
            if (hover != null) hover.Stop();
            if (image != null) image.color = Color.white;
        }

        /// <summary>어두운 색으로 바로 바꾼다 (true) / 원래 색으로 바로 바꾼다 (false).</summary>
        public void SetDarkened(bool darkened)
        {
            if (image != null) image.color = darkened ? darkTint : Color.white;
        }

        /// <summary>어두운 색에서 원래 색으로 서서히 돌아온다.</summary>
        public IEnumerator FadeToNormal(float duration)
        {
            if (image == null) yield break;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                image.color = Color.Lerp(darkTint, Color.white, t / duration);
                yield return null;
            }
            image.color = Color.white;
        }

        public void PlayCry()
        {
            if (audioSource != null && species != null && species.Cry != null)
                audioSource.PlayOneShot(species.Cry);
        }

        void OnDisable() => StopRevert();

        void PlayOnceOrPose(Sprite[] frames, float fps, Sprite fallback)
        {
            StopRevert();
            var list = Frames(frames);
            if (list != null && frameAnimator != null)
            {
                frameAnimator.PlayOnce(list, fps, ShowIdle);
                return;
            }
            ShowStill(fallback);
            if (isActiveAndEnabled) revert = StartCoroutine(RevertLater());
        }

        IEnumerator RevertLater()
        {
            yield return new WaitForSeconds(poseDuration);
            revert = null;
            ShowIdle();
        }

        IEnumerator InvokeLater(Action action)
        {
            yield return new WaitForSeconds(poseDuration);
            revert = null;
            action?.Invoke();
        }

        void StopRevert()
        {
            if (revert == null) return;
            StopCoroutine(revert);
            revert = null;
        }

        void ShowStill(Sprite sprite)
        {
            if (frameAnimator != null)
            {
                frameAnimator.ShowStill(sprite);
                return;
            }
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        void ApplySize()
        {
            if (image == null) return;
            float m = sizeMultiplier * (species != null ? species.BattleSizeMultiplier : 1f);
            image.rectTransform.localScale = new Vector3(m, m, 1f);
        }

        /// <summary>빈 칸을 뺀 프레임 목록 (한 장도 없으면 null).</summary>
        static Sprite[] Frames(Sprite[] frames)
        {
            if (!DragonSpecies.HasFrames(frames)) return null;
            var list = new List<Sprite>(frames.Length);
            foreach (var f in frames) if (f != null) list.Add(f);
            return list.ToArray();
        }

        // ── 테스트 ───────────────────────────
        [ContextMenu("테스트/비행")]
        void TestFly() => ShowIdle();

        [ContextMenu("테스트/공격")]
        void TestAttack() => ShowAttack();

        [ContextMenu("테스트/피격")]
        void TestHurt() => ShowHurt();

        [ContextMenu("테스트/패배")]
        void TestDefeat() => ShowDefeat();

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        void Update()
        {
            if (!testHotkeys || species == null) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.digit1Key.wasPressedThisFrame) ShowAttack();
            else if (keyboard.digit2Key.wasPressedThisFrame) ShowHurt();
            else if (keyboard.digit3Key.wasPressedThisFrame) ShowDefeat();
        }
#endif
    }
}
