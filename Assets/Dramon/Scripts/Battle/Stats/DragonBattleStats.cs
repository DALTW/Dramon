using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>결투 중 드래곤 수치: 체력(DragonHealth)·방어</summary>
    public class DragonBattleStats : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("드래곤 체력")]
        [SerializeField] DragonHealth health;

        public int Block { get; private set; }
        public int Health => health != null ? health.Current : 0;
        public int MaxHealth => health != null ? health.Max : 0;
        public bool IsDead => health != null && health.Current <= 0;
        /// <summary>다음 공격 1번에 더해질 피해 (강화 행동)</summary>
        public int NextAttackBonus { get; private set; }

        /// <summary>방어가 바뀔 때</summary>
        public event Action Changed;
        /// <summary>체력이 0이 됐을 때</summary>
        public event Action Died;

        /// <summary>결투 시작: 체력을 maxHealth로 가득, 방어 0</summary>
        public void ResetForDuel(int maxHealth)
        {
            if (health != null) health.ResetTo(maxHealth);
            Block = 0;
            NextAttackBonus = 0;
            Changed?.Invoke();
        }

        public void ClearBlock()
        {
            if (Block == 0) return;
            Block = 0;
            Changed?.Invoke();
        }

        public void GainBlock(int amount)
        {
            if (amount <= 0) return;
            Block += amount;
            Changed?.Invoke();
        }

        /// <summary>공격 피해: 방어로 먼저 막고 남은 만큼 체력에서 뺀다. (막은 양, 체력 피해)</summary>
        public (int blocked, int lost) TakeAttack(int amount)
        {
            if (amount <= 0 || health == null) return (0, 0);
            int blocked = Mathf.Min(Block, amount);
            Block -= blocked;
            int lost = LoseHealth(amount - blocked);
            Changed?.Invoke();
            return (blocked, lost);
        }

        public void AddAttackBonus(int amount)
        {
            if (amount <= 0) return;
            NextAttackBonus += amount;
            Changed?.Invoke();
        }

        /// <summary>강화 보너스를 꺼내 쓰고 0으로</summary>
        public int ConsumeAttackBonus()
        {
            int b = NextAttackBonus;
            if (b == 0) return 0;
            NextAttackBonus = 0;
            Changed?.Invoke();
            return b;
        }

        public int Heal(int amount)
        {
            if (amount <= 0 || health == null || IsDead) return 0;
            int before = health.Current;
            health.SetCurrent(health.Current + amount);
            return health.Current - before;
        }

        /// <summary>테스트: 체력을 정해진 값으로</summary>
        public void SetHealth(int value)
        {
            if (health == null) return;
            health.SetCurrent(value);
            if (health.Current <= 0) Died?.Invoke();
        }

        int LoseHealth(int amount)
        {
            if (amount <= 0) return 0;
            int before = health.Current;
            health.SetCurrent(health.Current - amount);
            if (health.Current == 0 && before > 0) Died?.Invoke();
            return before - health.Current;
        }
    }
}
