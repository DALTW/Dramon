using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 검으로 둥지를 골랐을 때 결투를 시작하고 결과를 처리한다.
    /// 지금은 카드 결투가 없어서 DuelPlaceholderUI의 [결투 승리]/[결투 패배] 버튼으로 결과를 정한다.
    /// (3단계에서 실제 결투 화면이 WinDuel/LoseDuel을 부르도록 바꿀 자리)
    /// </summary>
    public class DuelManager : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("탐험 단계 관리자 (결투 중 / 복귀 / 게임 오버 전환)")]
        [SerializeField] ExplorationState explorationState;

        public event Action<Vector2Int> DuelStarted;
        public event Action<bool> DuelEnded;

        public bool IsDueling { get; private set; }
        public Vector2Int CurrentNest { get; private set; }

        void OnEnable()
        {
            if (explorationState != null) explorationState.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (explorationState != null) explorationState.PhaseChanged -= HandlePhaseChanged;
        }

        public void StartDuel(Vector2Int nestPosition)
        {
            if (IsDueling || board == null || explorationState == null) return;
            CurrentNest = nestPosition;
            IsDueling = true;
            board.ActivateNest(nestPosition);
            explorationState.EnterDuel();
            DuelStarted?.Invoke(nestPosition);
        }

        /// <summary>결투 승리: 둥지를 정화하고 탐험으로 돌아간다. (보상 카드는 결투 시스템 단계에서 추가)</summary>
        public void WinDuel()
        {
            if (!IsDueling) return;
            IsDueling = false;
            explorationState.ReturnFromDuel();
            board.PurifyNest(CurrentNest);
            DuelEnded?.Invoke(true);
        }

        /// <summary>결투 패배: 즉시 게임 오버.</summary>
        public void LoseDuel()
        {
            if (!IsDueling) return;
            IsDueling = false;
            explorationState.TriggerGameOver();
            DuelEnded?.Invoke(false);
        }

        void HandlePhaseChanged(ExplorationPhase phase)
        {
            if (phase == ExplorationPhase.Ready) IsDueling = false; // 결투 중 재시작
        }
    }
}
