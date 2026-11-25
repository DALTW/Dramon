using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>플레이어의 돈을 보관한다. (지금은 넣기만, 쓰는 기능은 상점 단계에서 추가)</summary>
    public class PlayerWallet : MonoBehaviour
    {
        [Header("돈")]
        [Tooltip("게임을 시작할 때 가진 돈")]
        [SerializeField, Min(0)] int startingMoney = 0;
        [Tooltip("켜면 다시 시작(Restart)할 때 시작 금액으로 되돌려요. 게임 오버만으로는 그대로 둬요")]
        [SerializeField] bool resetOnRestart = true;

        [Header("연결")]
        [Tooltip("다시 시작을 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        public event Action<int> MoneyChanged;

        public int Money { get; private set; }

        void Awake()
        {
            Money = startingMoney;
        }

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += HandleRestarted;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= HandleRestarted;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Money += amount;
            MoneyChanged?.Invoke(Money);
        }

        /// <summary>돈을 쓴다. 모자라면 false. (상점 단계용)</summary>
        public bool TrySpend(int amount)
        {
            if (amount < 0 || Money < amount) return false;
            Money -= amount;
            MoneyChanged?.Invoke(Money);
            return true;
        }

        void HandleRestarted()
        {
            if (resetOnRestart && Money != startingMoney)
            {
                Money = startingMoney;
                MoneyChanged?.Invoke(Money);
            }
        }
    }
}
