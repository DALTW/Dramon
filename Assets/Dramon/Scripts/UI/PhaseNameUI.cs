using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>현재 단계 이름("레몬 표시" / "검 사냥" / "상점" …)을 표시한다.</summary>
    public class PhaseNameUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("현재 단계를 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("단계 이름을 표시할 Text")]
        [SerializeField] Text label;

        [Header("단계별 이름")]
        [SerializeField] string lemonMarkingName = "레몬 표시";
        [Tooltip("완료 안내 창이 떠 있는 동안 보여줄 이름")]
        [SerializeField] string markingCompleteName = "레몬 표시";
        [SerializeField] string swordHuntName = "검 사냥";
        [SerializeField] string duelName = "결투";
        [Tooltip("상점 창이 열려 있는 동안 보여줄 이름")]
        [SerializeField] string shopName = "상점";
        [SerializeField] string gameOverName = "게임 오버";
        [Tooltip("스테이지 클리어 창이 떠 있는 동안 보여줄 이름")]
        [SerializeField] string stageClearName = "스테이지 클리어";
        [SerializeField] string endingName = "엔딩";

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += Show;
        }

        void Start()
        {
            if (phaseController != null) Show(phaseController.Phase);
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= Show;
        }

        void Show(GamePhase phase)
        {
            if (label == null) return;
            label.text = phase switch
            {
                GamePhase.LemonMarking => lemonMarkingName,
                GamePhase.MarkingComplete => markingCompleteName,
                GamePhase.SwordHunt => swordHuntName,
                GamePhase.Duel => duelName,
                GamePhase.Encounter => duelName,
                GamePhase.Shop => shopName,
                GamePhase.StageClear => stageClearName,
                GamePhase.Ending => endingName,
                _ => gameOverName,
            };
        }
    }
}
