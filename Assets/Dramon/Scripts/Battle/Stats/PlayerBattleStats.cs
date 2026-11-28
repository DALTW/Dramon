using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>결투 중 플레이어 수치: 체력·방어·레몬 에너지</summary>
    public class PlayerBattleStats : MonoBehaviour
    {
        [Header("테스트 (에디터에서만)")]
        [Tooltip("켜면 플레이어가 피해를 받지 않아요")]
        [SerializeField] bool testInvincible;

        public int MaxHealth { get; private set; } = 1;
        public int Health { get; private set; } = 1;
        public int Block { get; private set; }
        public int Energy { get; private set; }
        public int MaxEnergy { get; private set; }
        public bool IsDead => Health <= 0;
        /// <summary>이번 드래곤 턴에 공격받을 때마다 반사할 피해 (반격 자세)</summary>
        public int Reflect { get; private set; }
        /// <summary>다음 드래곤 공격을 무효로 할 횟수 (연막 구르기)</summary>
        public int NegateCharges { get; private set; }

        /// <summary>체력·방어·에너지가 바뀔 때</summary>
        public event Action Changed;
        /// <summary>체력이 0이 됐을 때</summary>
        public event Action Died;

        public bool TestInvincible
        {
            get => Application.isEditor && testInvincible;
            set => testInvincible = value;
        }

        /// <summary>결투 시작: 최대 체력으로 가득, 방어·에너지 0</summary>
        public void ResetForDuel(int maxHealth)
        {
            MaxHealth = Mathf.Max(1, maxHealth);
            Health = MaxHealth;
            Block = 0;
            Energy = 0;
            MaxEnergy = 0;
            Reflect = 0;
            NegateCharges = 0;
            Changed?.Invoke();
        }

        public void AddReflect(int amount)
        {
            if (amount <= 0) return;
            Reflect += amount;
            Changed?.Invoke();
        }

        public void ClearReflect()
        {
            if (Reflect == 0) return;
            Reflect = 0;
            Changed?.Invoke();
        }

        public void AddNegate(int count = 1)
        {
            if (count <= 0) return;
            NegateCharges += count;
            Changed?.Invoke();
        }

        /// <summary>무효 횟수가 있으면 1 쓰고 true</summary>
        public bool TryConsumeNegate()
        {
            if (NegateCharges <= 0) return false;
            NegateCharges--;
            Changed?.Invoke();
            return true;
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

        /// <summary>에너지를 amount로 다시 채운다 (남은 에너지는 버림)</summary>
        public void RefillEnergy(int amount)
        {
            Energy = Mathf.Max(0, amount);
            MaxEnergy = Energy;
            Changed?.Invoke();
        }

        public void AddEnergy(int amount)
        {
            if (amount == 0) return;
            Energy = Mathf.Max(0, Energy + amount);
            Changed?.Invoke();
        }

        public bool TrySpendEnergy(int amount)
        {
            if (amount > Energy) return false;
            Energy -= Mathf.Max(0, amount);
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// 공격 피해: 방어로 먼저 막고 남은 만큼 체력에서 뺀다. blockIgnoreRatio = 방어를 무시하는 비율 (0.5면 방어의 절반만 계산).
        /// 막은 양, 체력 피해를 돌려준다.
        /// </summary>
        public (int blocked, int lost) TakeAttack(int amount, float blockIgnoreRatio = 0f)
        {
            if (amount <= 0) return (0, 0);
            int usableBlock = Mathf.FloorToInt(Block * (1f - Mathf.Clamp01(blockIgnoreRatio)));
            int blocked = Mathf.Min(usableBlock, amount);
            Block -= blocked;
            int lost = LoseHealth(amount - blocked);
            Changed?.Invoke();
            return (blocked, lost);
        }

        /// <summary>방어를 무시하고 체력을 잃는다 (중독·출혈 등). 실제로 잃은 양을 돌려준다</summary>
        public int TakeDirect(int amount)
        {
            int lost = LoseHealth(amount);
            Changed?.Invoke();
            return lost;
        }

        public int Heal(int amount)
        {
            if (amount <= 0 || IsDead) return 0;
            int before = Health;
            Health = Mathf.Min(MaxHealth, Health + amount);
            Changed?.Invoke();
            return Health - before;
        }

        int LoseHealth(int amount)
        {
            if (amount <= 0 || TestInvincible) return 0;
            int before = Health;
            Health = Mathf.Max(0, Health - amount);
            if (Health == 0 && before > 0) Died?.Invoke();
            return before - Health;
        }
    }
}
