using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 상점의 능력 강화 한 종류: 이름·아이콘·설명, 올릴 능력, 단계마다 오르는 양, 최대 단계, 단계별 가격.
    /// 강화 단계는 PlayerUpgrades에 보관한다.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopUpgradeData", menuName = "Dramon/Shop Upgrade Data")]
    public class ShopUpgradeData : ScriptableObject
    {
        [Header("표시")]
        [Tooltip("상점에 보일 강화 이름")]
        [SerializeField] string displayName = "강화";
        [Tooltip("강화 아이콘")]
        [SerializeField] Sprite icon;
        [Tooltip("상점에 보일 짧은 설명")]
        [TextArea(1, 3)]
        [SerializeField] string description;

        [Header("효과")]
        [Tooltip("올릴 능력 종류 (카드 결투 단계에서 사용)")]
        [SerializeField] UpgradeStat stat = UpgradeStat.SwordAttack;
        [Tooltip("한 단계마다 오르는 양")]
        [SerializeField] int amountPerLevel = 1;
        [Tooltip("최대 단계")]
        [Min(1)]
        [SerializeField] int maxLevel = 5;

        [Header("가격")]
        [Tooltip("단계별 가격 (첫 칸 = 1단계로 올릴 때). 칸이 최대 단계보다 적으면 마지막 가격을 계속 써요")]
        [SerializeField] int[] pricesPerLevel = { 50, 80, 120, 170, 230 };

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string Description => description;
        public UpgradeStat Stat => stat;
        public int AmountPerLevel => amountPerLevel;
        public int MaxLevel => maxLevel;

        /// <summary>level 단계로 올릴 때의 가격 (1부터)</summary>
        public int PriceForLevel(int level)
        {
            if (pricesPerLevel == null || pricesPerLevel.Length == 0) return 0;
            int i = Mathf.Clamp(level - 1, 0, pricesPerLevel.Length - 1);
            return Mathf.Max(0, pricesPerLevel[i]);
        }

        /// <summary>level 단계일 때의 능력 보너스</summary>
        public int BonusAt(int level) => amountPerLevel * Mathf.Clamp(level, 0, maxLevel);
    }
}
