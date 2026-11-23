using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 오른쪽 클릭으로 칸에 레몬을 놓아 굴을 표시하고, 맞았는지 판정한다.
    /// 굴이 맞으면 레몬이 남고, 굴이 아니면 즉시 게임 오버. 레몬은 다시 뺄 수 없다.
    /// </summary>
    public class LemonMarker : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("게임 오버·단계 완료를 확인할 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("남은 레몬(=남은 굴) 수를 확인할 집계기")]
        [SerializeField] NestMarkCounter nestMarkCounter;

        [Header("레몬 개수")]
        [Tooltip("켜면 레몬을 굴 수만큼만 가질 수 있어요. 끄면 무제한")]
        [SerializeField] bool limitToNestCount = true;

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        /// <summary>칸에 레몬을 놓는다. 첫 칸을 열기 전에는 동작하지 않는다 (첫 클릭 안전 규칙).</summary>
        public void Place(Vector2Int pos)
        {
            if (board == null || board.IsLocked || !board.NestsPlaced) return;
            if (limitToNestCount && nestMarkCounter != null && nestMarkCounter.NestsLeft <= 0) return;

            var result = board.PlaceLemon(pos);
            if (result == ForestBoard.LemonPlacement.OnSafeTile && phaseController != null)
                phaseController.TriggerGameOver(GameOverReason.WrongLemon);
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            // 레몬 표시가 끝나면 레몬으로 막아 둔 굴을 '봉인된 굴' 모습으로 바꾼다 (검 사냥 대상)
            if (phase == GamePhase.MarkingComplete && board != null) board.ShowLemonedNestsAsSealed();
        }
    }
}
