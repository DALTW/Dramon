using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>엔딩 화면 (일반 모드 보스를 쓰러뜨렸을 때): 쓰러뜨린 보스, 클리어 시간, 최고 기록, 무한 모드 열림 안내. [모드 선택] [다시 하기]</summary>
    public class EndingUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] StageProgress stageProgress;
        [SerializeField] GamePhaseController phaseController;
        [SerializeField] ProgressSave save;
        [Tooltip("엔딩 화면 (자식 Panel, 평소에는 꺼져 있음)")]
        [SerializeField] GameObject panel;
        [SerializeField] Text titleLabel;
        [SerializeField] Text bossLabel;
        [SerializeField] Text timeLabel;
        [SerializeField] Text unlockLabel;
        [Tooltip("엔딩 그림 칸 (그림이 비어 있으면 숨겨요)")]
        [SerializeField] Image picture;

        [Header("문구")]
        [SerializeField] string title = "숲의 주인을 쓰러뜨렸다!";
        [Tooltip("{0} = 보스 이름")]
        [SerializeField] string bossFormat = "{0}";
        [Tooltip("{0} = 이번 시간, {1} = 최고 기록")]
        [SerializeField] string timeFormat = "클리어 시간 {0}   (최고 {1})";
        [Tooltip("최고 기록을 새로 세웠을 때 덧붙는 문구")]
        [SerializeField] string newBestSuffix = "  최고 기록!";
        [SerializeField] string unlockText = "무한 모드가 열렸어요!";

        [Header("임시 에셋 표시")]
        [TextArea(1, 2)]
        [SerializeField] string tempAssetNote = "임시: 기본 창과 글자만 씀. 엔딩 화면 그림(Picture 칸)이 비어 있음";

        public string TempAssetNote => tempAssetNote;

        void OnEnable()
        {
            if (stageProgress != null) stageProgress.RunCleared += Show;
            if (phaseController != null) phaseController.PhaseChanged += HandlePhase;
            if (panel != null) panel.SetActive(false);
        }

        void OnDisable()
        {
            if (stageProgress != null) stageProgress.RunCleared -= Show;
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhase;
        }

        void Show()
        {
            if (titleLabel != null) titleLabel.text = title;
            if (bossLabel != null) bossLabel.text = string.Format(bossFormat, stageProgress.LastBossName);
            if (timeLabel != null)
            {
                float best = save != null ? save.BestClearSeconds : stageProgress.LastClearSeconds;
                timeLabel.text = string.Format(timeFormat, ModeSelectUI.FormatTime(stageProgress.LastClearSeconds), ModeSelectUI.FormatTime(best))
                    + (stageProgress.LastClearWasBest ? newBestSuffix : "");
            }
            if (unlockLabel != null) unlockLabel.text = stageProgress.LastClearUnlocked ? unlockText : "";
            if (picture != null) picture.gameObject.SetActive(picture.sprite != null);
            if (panel != null) panel.SetActive(true);
        }

        void HandlePhase(GamePhase phase)
        {
            if (phase != GamePhase.Ending && panel != null) panel.SetActive(false);
        }
    }
}
