using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 강화한 카드 종류 목록 (카드 종류 단위 1단계 "+"). 강화하면 덱의 같은 카드와 앞으로 살 카드까지 모두 적용된다.
    /// 다시 시작(Restart)하면 비운다.
    /// </summary>
    public class CardUpgradeState : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("다시 시작을 알려 주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        [Header("초기화")]
        [Tooltip("켜면 다시 시작할 때 카드 강화를 비워요")]
        [SerializeField] bool clearOnRestart = true;

        [Header("확인용 (실행 중 강화한 카드)")]
        [SerializeField] List<CardData> upgraded = new List<CardData>();

        public event Action Changed;

        void Awake() => upgraded.Clear();

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += HandleRestarted;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= HandleRestarted;
        }

        public bool IsUpgraded(CardData card) => card != null && upgraded.Contains(card);

        public void Upgrade(CardData card)
        {
            if (card == null || upgraded.Contains(card)) return;
            upgraded.Add(card);
            Changed?.Invoke();
        }

        public void ClearAll()
        {
            if (upgraded.Count == 0) return;
            upgraded.Clear();
            Changed?.Invoke();
        }

        void HandleRestarted()
        {
            if (clearOnRestart) ClearAll();
        }
    }
}
