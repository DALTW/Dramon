using System;
using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 드래곤 등장 연출을 순서대로 진행한다.
    /// ① 둥지 위 깨어나는 드래곤 + 화면 번쩍임 → ② 검게 덮는 전환 → ③ 결투 화면 슬라이드 인
    /// → ④ 어두운 색에서 원래 색 + 울음소리 → ⑤ 체력 표시 창 → ⑥ 메시지 → ⑦ 임시 승패 버튼.
    /// ③ 슬라이드 인부터 드래곤은 비행 모션(날갯짓 + 위아래 흔들림)을 반복하고, ④ 색 복귀는 그 위에 겹쳐서 진행된다.
    /// ②와 ③ 사이(검은 화면)에 배경을 그 드래곤의 결투 둥지로 바꾸고, 드래곤·플레이어를 둥지 발판 위치에 세운다.
    /// 결투에서 이기면 결투 화면을 페이드 아웃하고 숲으로 돌아간다 (기본 배경·Scene 배치 위치로 복귀). 단계마다 켜고 끄기·시간 조절 가능.
    /// </summary>
    public class EncounterIntroSequence : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("게임 단계 관리자 (게임 오버·다시 시작이면 연출을 멈춰요)")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("결투 화면 켜고 끄기")]
        [SerializeField] DuelScreenUI duelScreen;
        [Tooltip("화면 번쩍임")]
        [SerializeField] ScreenFlashUI flash;
        [Tooltip("검게 덮는 화면 전환")]
        [SerializeField] ScreenWipeTransition wipe;
        [Tooltip("드래곤 쪽·플레이어 쪽 슬라이드 인")]
        [SerializeField] DuelStageSlideIn slideIn;
        [Tooltip("결투 화면의 드래곤 그림")]
        [SerializeField] DragonBattlerView battler;
        [Tooltip("드래곤 체력 표시 창")]
        [SerializeField] DragonHealthUI healthUI;
        [Tooltip("등장 메시지 창")]
        [SerializeField] EncounterMessageUI message;
        [Tooltip("둥지 위에 깨어나는 드래곤을 보여 줄 표시")]
        [SerializeField] NestWakeSpriteView wakeSprite;
        [Tooltip("결투 화면 배경 (드래곤별 결투 둥지 배경으로 바꿔요)")]
        [SerializeField] DuelBackgroundView background;

        [Header("결투 둥지")]
        [Tooltip("켜면 화면이 검게 덮인 동안 배경을 그 드래곤의 결투 둥지로 바꾸고, 드래곤·플레이어를 그 둥지의 발판 위치에 세워요")]
        [SerializeField] bool useNestStage = true;

        [Header("① 둥지 번쩍임")]
        [Tooltip("켜면 화면이 하얗게 번쩍여요")]
        [SerializeField] bool useFlash = true;
        [Tooltip("번쩍이는 횟수")]
        [Min(0)]
        [SerializeField] int flashCount = 2;
        [Tooltip("한 번 번쩍일 때 밝아지는 시간 = 어두워지는 시간(초)")]
        [Min(0f)]
        [SerializeField] float flashHalfDuration = 0.2f;
        [Tooltip("켜면 번쩍이는 동안 둥지 위에 깨어나는 드래곤(빛나는 수면 그림)을 보여 줘요")]
        [SerializeField] bool showWakingDragon = true;

        [Header("② 화면 전환")]
        [Tooltip("켜면 화면을 검게 덮는 전환을 써요")]
        [SerializeField] bool useWipe = true;
        [Tooltip("화면을 다 덮는 데 걸리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float wipeDuration = 0.4f;
        [Tooltip("검은 화면을 유지하는 시간(초)")]
        [Min(0f)]
        [SerializeField] float blackHold = 0.1f;

        [Header("③ 슬라이드 인")]
        [Tooltip("켜면 드래곤 쪽과 플레이어 쪽이 양옆에서 들어와요")]
        [SerializeField] bool useSlideIn = true;
        [Tooltip("양옆에서 들어오는 데 걸리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float slideDuration = 1f;
        [Tooltip("검은 화면이 걷히는 시간(초)")]
        [Min(0f)]
        [SerializeField] float uncoverDuration = 0.3f;

        [Header("④ 색 복귀 · 울음소리")]
        [Tooltip("켜면 드래곤이 어두운 색으로 들어왔다가 원래 색으로 돌아와요")]
        [SerializeField] bool useDarkToNormal = true;
        [Tooltip("원래 색으로 돌아오는 시간(초)")]
        [Min(0f)]
        [SerializeField] float toneDuration = 0.4f;
        [Tooltip("켜면 원래 색으로 돌아온 뒤 울음소리를 내요 (DragonSpecies에 소리가 있을 때)")]
        [SerializeField] bool playCry = true;

        [Header("⑤ 체력 표시 창")]
        [Tooltip("켜면 드래곤 체력 표시 창이 나타나요")]
        [SerializeField] bool showHealth = true;
        [Tooltip("체력 표시 창이 나타나기 전 기다리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float healthDelay = 0.1f;

        [Header("⑥ 메시지")]
        [Tooltip("켜면 등장 메시지를 보여 줘요")]
        [SerializeField] bool showMessage = true;
        [Tooltip("{0} 자리에 드래곤 이름이 들어가요")]
        [SerializeField] string messageFormat = "앗! 둥지에서 {0}이(가) 깨어났다!";
        [Tooltip("메시지가 다 나온 뒤 버튼이 나타나기까지 기다리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float afterMessageDelay = 0.3f;

        [Header("결투가 끝난 뒤")]
        [Tooltip("이겼을 때 결투 화면이 사라지는 페이드 시간(초)")]
        [Min(0f)]
        [SerializeField] float outroFadeDuration = 0.4f;

        public bool IsPlaying { get; private set; }

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
            StopAll();
        }

        /// <summary>등장 연출 시작. 끝나면 onFinished 호출 (결투 시작).</summary>
        public void PlayIntro(DragonSpecies species, Vector3 nestWorldPosition, Action onFinished)
        {
            StopAll();
            IsPlaying = true;
            StartCoroutine(Intro(species, nestWorldPosition, onFinished));
        }

        /// <summary>결투 화면을 페이드 아웃하고 숲으로 돌아간다. 끝나면 onFinished 호출.</summary>
        public void PlayOutro(Action onFinished)
        {
            StopAll();
            IsPlaying = true;
            StartCoroutine(Outro(onFinished));
        }

        IEnumerator Intro(DragonSpecies species, Vector3 nestWorldPosition, Action onFinished)
        {
            // ① 둥지 위 깨어나는 드래곤 + 번쩍임
            if (showWakingDragon && wakeSprite != null && species != null)
                wakeSprite.Show(species.FieldSleepingGlow, nestWorldPosition);
            if (useFlash && flash != null && flashCount > 0)
                yield return flash.Flash(flashCount, flashHalfDuration);

            // ② 검게 덮기
            bool wiped = useWipe && wipe != null;
            if (wiped)
            {
                yield return wipe.Cover(wipeDuration);
                if (blackHold > 0f) yield return new WaitForSeconds(blackHold);
            }
            if (wakeSprite != null) wakeSprite.Hide();

            // ③ 결투 화면 준비 → 슬라이드 인
            if (battler != null)
            {
                battler.SetSpecies(species);
                battler.SetDarkened(useDarkToNormal);
            }
            if (healthUI != null) healthUI.Hide();
            if (message != null) message.Hide();
            if (duelScreen != null)
            {
                duelScreen.ShowStage();
                duelScreen.SetButtonsVisible(false);
            }
            ApplyNestStage(species); // 검은 화면이 덮인 동안 배경·발판 위치 바꾸기
            if (slideIn != null)
            {
                if (useSlideIn) slideIn.PrepareOffscreen();
                else slideIn.SnapHome();
            }
            if (wiped) StartCoroutine(wipe.Uncover(uncoverDuration));
            if (useSlideIn && slideIn != null) yield return slideIn.SlideIn(slideDuration);
            else if (wiped) yield return new WaitForSeconds(uncoverDuration);

            // ④ 어두운 색 → 원래 색, 울음소리
            if (useDarkToNormal && battler != null) yield return battler.FadeToNormal(toneDuration);
            if (playCry && battler != null) battler.PlayCry();

            // ⑤ 체력 표시 창
            if (showHealth && healthUI != null)
            {
                if (healthDelay > 0f) yield return new WaitForSeconds(healthDelay);
                healthUI.Show();
            }

            // ⑥ 메시지
            if (showMessage && message != null && species != null)
            {
                yield return message.Show(string.Format(messageFormat, species.DisplayName));
                if (afterMessageDelay > 0f) yield return new WaitForSeconds(afterMessageDelay);
            }

            // ⑦ 임시 승패 버튼
            if (duelScreen != null) duelScreen.SetButtonsVisible(true);
            IsPlaying = false;
            onFinished?.Invoke();
        }

        IEnumerator Outro(Action onFinished)
        {
            if (duelScreen != null) yield return duelScreen.FadeOut(outroFadeDuration);
            if (healthUI != null) healthUI.Hide();
            if (message != null) message.Hide();
            RestoreNestStage();
            if (battler != null) battler.StopMotion(); // 비행 모션·흔들림 멈춤
            IsPlaying = false;
            onFinished?.Invoke();
        }

        /// <summary>결투 배경을 그 드래곤의 둥지로, 도착 위치를 그 둥지의 발판 위치로 바꾼다.</summary>
        void ApplyNestStage(DragonSpecies species)
        {
            if (!useNestStage || species == null)
            {
                RestoreNestStage(false);
                return;
            }
            if (background != null) background.Apply(species);
            if (slideIn != null)
            {
                if (species.UseStandPoints) slideIn.SetArrivalFromStandPoints(species.DragonStandPoint, species.PlayerStandPoint);
                else slideIn.ResetArrival();
            }
        }

        /// <summary>기본 배경과 Scene 배치 위치로 되돌린다.</summary>
        void RestoreNestStage(bool movePositions = true)
        {
            if (background != null) background.RestoreDefault();
            if (slideIn == null) return;
            if (movePositions) slideIn.RestoreScenePositions();
            else slideIn.ResetArrival();
        }

        void StopAll()
        {
            StopAllCoroutines();
            IsPlaying = false;
            if (flash != null) flash.Clear();
            if (wipe != null) wipe.Clear();
            if (wakeSprite != null) wakeSprite.Hide();
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.GameOver && phase != GamePhase.LemonMarking) return;
            StopAll();
            if (healthUI != null) healthUI.Hide();
            if (message != null) message.Hide();
            RestoreNestStage();
            if (battler != null) battler.StopMotion(); // 비행 모션·흔들림 멈춤
        }
    }
}
