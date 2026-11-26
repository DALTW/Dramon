using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 상점에서 파는 결투용 소모품 한 종류: 이름·아이콘·설명·가격·상점 재고·최대 보유 개수.
    /// 효과는 카드 결투 단계에서 만들고, 지금은 PlayerItemBag에 보관만 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopItemData", menuName = "Dramon/Shop Item Data")]
    public class ShopItemData : ScriptableObject
    {
        [Header("표시")]
        [Tooltip("상점에 보일 아이템 이름")]
        [SerializeField] string displayName = "아이템";
        [Tooltip("아이템 아이콘")]
        [SerializeField] Sprite icon;
        [Tooltip("상점에 보일 짧은 설명 (효과는 카드 결투 단계에서 사용)")]
        [TextArea(1, 3)]
        [SerializeField] string description;

        [Header("판매")]
        [Tooltip("가격")]
        [Min(0)]
        [SerializeField] int price = 30;
        [Tooltip("켜면 상점 재고가 무제한이에요")]
        [SerializeField] bool unlimitedStock = true;
        [Tooltip("상점 재고 (무제한이 꺼져 있을 때만). 다시 시작하면 처음 재고로 돌아와요")]
        [Min(0)]
        [SerializeField] int stock = 5;
        [Tooltip("플레이어가 가질 수 있는 최대 개수 (0 = 제한 없음)")]
        [Min(0)]
        [SerializeField] int maxOwned = 5;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string Description => description;
        public int Price => price;
        public bool UnlimitedStock => unlimitedStock;
        public int Stock => stock;
        public int MaxOwned => maxOwned;
    }
}
