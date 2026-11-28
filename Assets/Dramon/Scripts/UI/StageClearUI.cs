using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 스테이지 클리어 창: 클리어한 스테이지, 보상 골드, 다음 스테이지 안내. [다음 스테이지] 버튼은 StageProgress.GoToNextStage에 연결.
    /// </summary>
    public class StageClearUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] StageProgress stageProgress;
        [Tooltip("창을 닫을 때를 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("클리어 창 (자식 Panel, 평소에는 꺼져 있음)")]
        [SerializeField] GameObject panel;
        [SerializeField] Text titleLabel;
        [SerializeField] Text rewardLabel;
        [SerializeField] Text nextLabel;
        [Tooltip("클리어 그림 칸 (그림이 비어 있으면 숨겨요). 스테이지 클리어 그림이 생기면 여기에 넣기")]
        [SerializeField] Image banner;
        [Tooltip("다음 스테이지 결투 수를 알려 줘요 (깨어난 둥지만 결투인지, 모든 둥지와 결투인지)")]
        [SerializeField] AwakenedNestPicker nestPicker;

        [Header("문구")]
        [Tooltip("{0} = 클리어한 스테이지")]
        [SerializeField] string titleFormat = "{0} 스테이지 클리어!";
        [Tooltip("{0} = 보상 골드")]
        [SerializeField] string rewardFormat = "클리어 보상  {0}G";
        [Tooltip("{0} = 다음 스테이지, {1}×{2} = 숲 크기, {3} = 둥지, {4} = 결투, {5} = 최대 에너지")]
        [TextArea(2, 3)]
        [SerializeField] string nextFormat = "다음: {0} 스테이지\n숲 {1}×{2} · 둥지 {3} · 결투 {4} · 에너지 {5}";

        [Header("임시 에셋 표시")]
        [Tooltip("임시로 채운 자리 메모 (정식 그림을 넣으면 지워도 돼요)")]
        [TextArea(1, 3)]
        [SerializeField] string tempAssetNote = "임시: 기본 창(UI_Window_9Slice)과 글자만 씀. 스테이지 클리어 그림(Banner 칸)이 비어 있음";

        public string TempAssetNote => tempAssetNote;
        bool hasResult;

        void OnEnable()
        {
            if (stageProgress != null) stageProgress.StageCleared += Show;
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
            if (panel != null) panel.SetActive(false);
        }

        void OnDisable()
        {
            if (stageProgress != null) stageProgress.StageCleared -= Show;
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        void Show(int stage, int gold)
        {
            if (titleLabel != null) titleLabel.text = string.Format(titleFormat, stage);
            if (rewardLabel != null) rewardLabel.text = string.Format(rewardFormat, gold);
            if (nextLabel != null && stageProgress != null)
            {
                var n = stageProgress.Next;
                int duels = nestPicker != null ? nestPicker.DuelCount(n) : n.awakened;
                nextLabel.text = string.Format(nextFormat, n.stage, n.width, n.height, n.nests, duels, n.maxEnergy);
            }
            if (banner != null) banner.gameObject.SetActive(banner.sprite != null);
            hasResult = true;
            if (panel != null) panel.SetActive(true);
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.LemonMarking || phase == GamePhase.GameOver) hasResult = false;
            // 상점에 다녀오면 (상점 → 스테이지 클리어) 창을 다시 보여 줌
            if (panel != null) panel.SetActive(phase == GamePhase.StageClear && hasResult);
        }
    }
}
