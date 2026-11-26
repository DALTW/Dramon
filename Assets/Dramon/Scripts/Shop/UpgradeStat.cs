using UnityEngine;

namespace Dramon
{
    /// <summary>강화로 올릴 수 있는 능력 종류 (카드 결투 단계에서 사용)</summary>
    public enum UpgradeStat
    {
        [InspectorName("검 공격력")] SwordAttack,
        [InspectorName("방패 방어력")] ShieldDefense,
        [InspectorName("최대 체력")] MaxHealth,
        [InspectorName("공격력")] AttackPower,
        [InspectorName("레몬 공격력")] LemonAttack,
    }

    /// <summary>능력 종류의 화면 표시 이름</summary>
    public static class UpgradeStatNames
    {
        public static string Get(UpgradeStat stat)
        {
            switch (stat)
            {
                case UpgradeStat.SwordAttack: return "검 공격력";
                case UpgradeStat.ShieldDefense: return "방패 방어력";
                case UpgradeStat.MaxHealth: return "최대 체력";
                case UpgradeStat.AttackPower: return "공격력";
                case UpgradeStat.LemonAttack: return "레몬 공격력";
                default: return stat.ToString();
            }
        }
    }
}
