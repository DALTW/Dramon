using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Dramon
{
    /// <summary>
    /// 결투 화면의 검사(레몬 검사) 그림. 대기(반복 + 숨쉬기 1px) / 등장 걷기 / 카드 동작 / 피격·속성 반응 / 승리 / 쓰러짐.
    /// 동작은 한 번 재생한 뒤 대기로 돌아오고, 승리·쓰러짐은 마지막 프레임을 유지한다.
    /// 프레임 재생은 DragonFrameAnimator, 숨쉬기는 DragonHoverMotion을 그대로 쓴다. 위치·크기는 Scene(PlayerSide) 배치를 그대로 쓴다.
    /// </summary>
    public class HeroBattlerView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("검사 동작별 프레임 묶음")]
        [SerializeField] HeroSpriteSet spriteSet;
        [Tooltip("프레임 재생기 (PlayerSide/Hero/Motion/Sprite)")]
        [SerializeField] DragonFrameAnimator frameAnimator;
        [Tooltip("숨쉬기 오르내림 (PlayerSide/Hero)")]
        [SerializeField] DragonHoverMotion breathing;
        [Tooltip("게임 단계 관리자 (숲으로 돌아가기·게임 오버·다시 시작이면 대기로 초기화)")]
        [SerializeField] GamePhaseController phaseController;

        [Header("숨쉬기")]
        [Tooltip("켜면 대기 중에 1px씩 위아래로 오르내려요")]
        [SerializeField] bool useBreathing = true;
        [Tooltip("오르내리는 거리 (px)")]
        [Min(0f)]
        [SerializeField] float breathAmplitude = 1f;
        [Tooltip("한 번 오르내리는 시간 (초)")]
        [Min(0.1f)]
        [SerializeField] float breathPeriod = 1.6f;

        [Header("쓰러짐")]
        [Tooltip("결투 패배 때 쓰러진 자세를 보여 주는 시간 (초). 이 시간이 지나면 게임 오버")]
        [Min(0f)]
        [SerializeField] float defeatedHoldSeconds = 1.2f;

        [Header("테스트 (에디터에서만)")]
        [Tooltip("켜면 Play 중 결투 화면에서 1 = 베기, 2 = 방패, 3 = 레몬 던지기, 4 = 지금 드래곤의 속성 반응을 볼 수 있어요")]
        [SerializeField] bool testHotkeys = true;

        DragonSpecies opponent;
        bool lowHealth;
        Coroutine routine;

        public DragonSpecies Opponent => opponent;
        public float DefeatedHoldSeconds => defeatedHoldSeconds;

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
            StopRoutine();
        }

        /// <summary>이번 결투 상대 (속성 반응을 고를 때 씀)</summary>
        public void SetOpponent(DragonSpecies species) => opponent = species;

        /// <summary>체력이 낮을 때 대기 자세를 바꾼다 (카드 결투 단계에서 사용)</summary>
        public void SetLowHealth(bool low)
        {
            if (lowHealth == low) return;
            lowHealth = low;
            if (frameAnimator != null && !frameAnimator.IsPlaying) ShowIdle();
        }

        // ── 기본 상태 ──────────────────────────

        /// <summary>대기 반복 + 숨쉬기</summary>
        public void ShowIdle()
        {
            StopRoutine();
            var clip = lowHealth && Has(Set?.LowHealth) ? Set.LowHealth : Set?.Idle;
            Loop(clip);
            if (breathing != null)
            {
                if (useBreathing) breathing.Play(breathAmplitude, breathPeriod);
                else breathing.Stop();
            }
        }

        /// <summary>등장 걷기 반복 (슬라이드 인 동안)</summary>
        public void PlayEnter()
        {
            StopRoutine();
            StopBreathing();
            Loop(Set?.Enter);
        }

        /// <summary>대기로 초기화 (숲으로 돌아가기·게임 오버·다시 시작)</summary>
        public void ResetToIdle()
        {
            lowHealth = false;
            ShowIdle();
        }

        // ── 카드 동작 (재생 후 대기로) ─────────

        public void PlaySlash() => PlayThenIdle(Set?.Slash);
        public void PlayBlock() => PlayThenIdle(Set?.Block);
        public void PlayRoll() => PlayThenIdle(Set?.Roll);
        public void PlayLemonThrow() => PlayThenIdle(Set?.LemonThrow);
        public void PlayLemonSqueeze() => PlayThenIdle(Set?.LemonSqueeze);
        public void PlayHerb() => PlayThenIdle(Set?.Herb);
        public void PlayDrawCard() => PlayThenIdle(Set?.DrawCard);

        /// <summary>피격 → 자세 회복 → 대기</summary>
        public void PlayHit() => PlaySequence(Set?.Hit, Set?.HitRecover);

        /// <summary>드래곤 브레스 반응: 그 드래곤의 속성 반응 → 자세 회복 → 대기 (반응이 없으면 기본 피격)</summary>
        public void PlayDragonBreathReaction(DragonSpecies species)
        {
            if (species != null) opponent = species;
            var type = opponent != null ? opponent.PlayerHitReaction : HeroReactionType.None;
            var reaction = Set != null ? Set.Reaction(type) : null;
            if (!Has(reaction)) reaction = Set?.Hit;
            PlaySequence(reaction, Set?.HitRecover);
        }

        // ── 결과 (마지막 프레임 유지) ───────────

        /// <summary>승리 자세 (전리품 창까지 유지)</summary>
        public void ShowVictory()
        {
            StopRoutine();
            StopBreathing();
            Hold(Set?.Victory, null);
        }

        /// <summary>피격 → 쓰러짐을 보여 주고, 쓰러진 자세를 정해진 시간 동안 둔 뒤 onShown을 부른다.</summary>
        public void ShowDefeated(Action onShown)
        {
            StopRoutine();
            StopBreathing();
            if (!isActiveAndEnabled || frameAnimator == null)
            {
                onShown?.Invoke();
                return;
            }
            routine = StartCoroutine(DefeatedRoutine(onShown));
        }

        IEnumerator DefeatedRoutine(Action onShown)
        {
            bool done = false;
            if (Has(Set?.Hit))
            {
                frameAnimator.PlayOnce(Set.Hit.Valid(), Set.Hit.fps, () => done = true);
                while (!done) yield return null;
            }
            Hold(Set?.Defeated, null);
            if (defeatedHoldSeconds > 0f) yield return new WaitForSeconds(defeatedHoldSeconds);
            routine = null;
            onShown?.Invoke();
        }

        /// <summary>재생·숨쉬기를 멈춘다 (지금 그림은 그대로)</summary>
        public void StopMotion()
        {
            StopRoutine();
            if (frameAnimator != null) frameAnimator.Stop();
            StopBreathing();
        }

        // ── 내부 ───────────────────────────────

        HeroSpriteSet Set => spriteSet;

        static bool Has(HeroClip clip) => clip != null && clip.Valid() != null;

        void Loop(HeroClip clip)
        {
            if (frameAnimator == null || !Has(clip)) return;
            frameAnimator.PlayLoop(clip.Valid(), clip.fps);
        }

        void Hold(HeroClip clip, Action onFinished)
        {
            if (frameAnimator == null || !Has(clip)) { onFinished?.Invoke(); return; }
            frameAnimator.PlayAndHold(clip.Valid(), clip.fps, onFinished);
        }

        void PlayThenIdle(HeroClip clip) => PlaySequence(clip, null);

        /// <summary>first → second(있으면) → 대기</summary>
        void PlaySequence(HeroClip first, HeroClip second)
        {
            StopRoutine();
            if (frameAnimator == null || !Has(first)) { ShowIdle(); return; }
            StopBreathing();
            frameAnimator.PlayOnce(first.Valid(), first.fps, () =>
            {
                if (Has(second)) frameAnimator.PlayOnce(second.Valid(), second.fps, ShowIdle);
                else ShowIdle();
            });
        }

        void StopBreathing()
        {
            if (breathing != null) breathing.Stop();
        }

        void StopRoutine()
        {
            if (routine == null) return;
            StopCoroutine(routine);
            routine = null;
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.SwordHunt || phase == GamePhase.GameOver || phase == GamePhase.LemonMarking)
                ResetToIdle();
        }

        // ── 테스트 ─────────────────────────────
        [ContextMenu("테스트/대기")] void TestIdle() => ShowIdle();
        [ContextMenu("테스트/등장 걷기")] void TestEnter() => PlayEnter();
        [ContextMenu("테스트/베기")] void TestSlash() => PlaySlash();
        [ContextMenu("테스트/방패")] void TestBlock() => PlayBlock();
        [ContextMenu("테스트/구르기")] void TestRoll() => PlayRoll();
        [ContextMenu("테스트/레몬 던지기")] void TestLemonThrow() => PlayLemonThrow();
        [ContextMenu("테스트/레몬즙")] void TestLemonSqueeze() => PlayLemonSqueeze();
        [ContextMenu("테스트/약초")] void TestHerb() => PlayHerb();
        [ContextMenu("테스트/카드 뽑기")] void TestDrawCard() => PlayDrawCard();
        [ContextMenu("테스트/피격")] void TestHit() => PlayHit();
        [ContextMenu("테스트/속성 반응 (지금 드래곤)")] void TestReaction() => PlayDragonBreathReaction(opponent);
        [ContextMenu("테스트/승리")] void TestVictory() => ShowVictory();
        [ContextMenu("테스트/쓰러짐")] void TestDefeated() => ShowDefeated(null);
        [ContextMenu("테스트/체력 낮음 켜기·끄기")] void TestLowHealth() => SetLowHealth(!lowHealth);

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        void Update()
        {
            if (!testHotkeys) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.digit1Key.wasPressedThisFrame) PlaySlash();
            else if (keyboard.digit2Key.wasPressedThisFrame) PlayBlock();
            else if (keyboard.digit3Key.wasPressedThisFrame) PlayLemonThrow();
            else if (keyboard.digit4Key.wasPressedThisFrame) PlayDragonBreathReaction(opponent);
        }
#endif
    }
}
