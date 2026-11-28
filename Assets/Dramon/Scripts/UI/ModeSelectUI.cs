using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 모드 선택 화면: [일반 모드] [무한 모드]. 무한 모드는 일반 모드를 클리어해야 열려요 (잠금 표시).
    /// 고르면 그 모드의 1 스테이지로 새로 시작한다. 게임을 켤 때와 엔딩·기록 창의 [모드 선택]에서 열린다.
    /// </summary>
    public class ModeSelectUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] StageProgress stageProgress;
        [SerializeField] ProgressSave save;
        [Tooltip("모드 선택 화면 (자식 Panel)")]
        [SerializeField] GameObject panel;
        [SerializeField] GameModeData normalMode;
        [SerializeField] GameModeData endlessMode;

        [Header("화면 글자")]
        [SerializeField] Text normalDescription;
        [SerializeField] Text endlessDescription;
        [Tooltip("무한 모드 버튼 (잠겼으면 흐리게)")]
        [SerializeField] Button endlessButton;
        [Tooltip("잠금 아이콘 칸 (그림이 비어 있으면 숨겨요)")]
        [SerializeField] Image lockIcon;
        [SerializeField] Text recordLabel;
        [Tooltip("잠긴 모드를 눌렀을 때 문구")]
        [SerializeField] Text messageLabel;

        [Header("진행")]
        [Tooltip("켜면 게임을 켤 때 모드 선택 화면부터 보여요")]
        [SerializeField] bool showOnStart = true;
        [Tooltip("잠긴 버튼 색")]
        [SerializeField] Color lockedButtonColor = new Color(0.55f, 0.52f, 0.48f, 1f);

        [Header("문구")]
        [Tooltip("{0} = 일반 모드 클리어 표시, {1} = 최고 클리어 시간, {2} = 무한 모드 최고 스테이지")]
        [SerializeField] string recordFormat = "일반 모드 {0} · 최고 {1}    무한 모드 최고 {2} 스테이지";
        [SerializeField] string clearedMark = "클리어!";
        [SerializeField] string notClearedMark = "아직";
        [SerializeField] string noTimeText = "--:--";

        [Header("임시 에셋 표시")]
        [TextArea(1, 3)]
        [SerializeField] string tempAssetNote = "임시: 기본 창과 글자 버튼만 씀. 모드 선택 화면 그림(배경·제목)과 잠금 아이콘(Lock Icon 칸)이 비어 있음";

        Color endlessButtonColor = Color.white;

        public bool IsOpen => panel != null && panel.activeSelf;
        public string TempAssetNote => tempAssetNote;

        void Awake()
        {
            if (endlessButton != null && endlessButton.image != null) endlessButtonColor = endlessButton.image.color;
        }

        void Start()
        {
            if (showOnStart) Open();
            else if (panel != null) panel.SetActive(false);
        }

        /// <summary>모드 선택 화면 열기</summary>
        public void Open()
        {
            Refresh();
            if (messageLabel != null) messageLabel.text = "";
            if (panel != null)
            {
                panel.SetActive(true);
                transform.SetAsLastSibling(); // 다른 창 위에
            }
        }

        public void ChooseNormal() => Choose(normalMode);
        public void ChooseEndless() => Choose(endlessMode);

        void Choose(GameModeData mode)
        {
            if (mode == null || stageProgress == null) return;
            if (save != null && !save.IsUnlocked(mode))
            {
                if (messageLabel != null) messageLabel.text = mode.LockedMessage;
                return;
            }
            if (panel != null) panel.SetActive(false);
            stageProgress.StartMode(mode);
        }

        void Refresh()
        {
            if (normalDescription != null && normalMode != null) normalDescription.text = normalMode.Description;
            bool endlessOpen = save == null || save.IsUnlocked(endlessMode);
            if (endlessDescription != null && endlessMode != null)
                endlessDescription.text = endlessOpen ? endlessMode.Description : endlessMode.LockedMessage;
            if (endlessButton != null && endlessButton.image != null)
                endlessButton.image.color = endlessOpen ? endlessButtonColor : lockedButtonColor;
            if (lockIcon != null) lockIcon.gameObject.SetActive(!endlessOpen && lockIcon.sprite != null);
            if (recordLabel != null && save != null)
            {
                float best = save.BestClearSeconds;
                string time = best > 0f ? FormatTime(best) : noTimeText;
                recordLabel.text = string.Format(recordFormat, save.NormalCleared ? clearedMark : notClearedMark, time, save.EndlessBestStage);
            }
        }

        public static string FormatTime(float seconds)
        {
            int s = Mathf.FloorToInt(seconds);
            return s >= 3600 ? $"{s / 3600}:{s / 60 % 60:00}:{s % 60:00}" : $"{s / 60:00}:{s % 60:00}";
        }
    }
}
