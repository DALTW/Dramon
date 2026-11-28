using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>보스 힌트: 일반 모드 3 스테이지부터 스테이지가 시작될 때 보스 속성을 살짝 알려 주는 문구 ("숲 깊은 곳에서 불꽃 냄새가 난다…")</summary>
    public class BossHintUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] StageProgress stageProgress;
        [SerializeField] BossFight bossFight;
        [Tooltip("켜고 끌 창 (평소에는 꺼져 있음)")]
        [SerializeField] GameObject window;
        [SerializeField] Text label;
        [Tooltip("투명도를 바꿀 CanvasGroup")]
        [SerializeField] CanvasGroup group;

        [Header("표시")]
        [Tooltip("보이는 시간(초)")]
        [Min(0.5f)] [SerializeField] float showDuration = 4f;
        [Tooltip("사라지는 시간(초)")]
        [Min(0f)] [SerializeField] float fadeDuration = 0.6f;

        void Awake()
        {
            if (window != null) window.SetActive(false);
        }

        void OnEnable()
        {
            if (stageProgress != null) stageProgress.StageStarted += HandleStageStarted;
        }

        void OnDisable()
        {
            if (stageProgress != null) stageProgress.StageStarted -= HandleStageStarted;
        }

        void HandleStageStarted()
        {
            StopAllCoroutines();
            if (window != null) window.SetActive(false);
            if (stageProgress == null || bossFight == null || bossFight.Boss == null || bossFight.Rules == null) return;
            var mode = stageProgress.Mode;
            if (mode == null || !mode.UseBoss) return;
            int s = stageProgress.Stage;
            if (s < bossFight.Rules.HintFromStage || s > mode.LastStage) return;
            if (label != null) label.text = bossFight.Boss.BossHint;
            if (isActiveAndEnabled) StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            if (window != null) window.SetActive(true);
            if (group != null) group.alpha = 1f;
            yield return new WaitForSeconds(showDuration);
            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                if (group != null) group.alpha = 1f - t / fadeDuration;
                yield return null;
            }
            if (window != null) window.SetActive(false);
        }
    }
}
