using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>상점에 보일 목록: 능력 강화, 아이템, 판매 목록에서 재료를 보여 줄 순서.</summary>
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "Dramon/Shop Catalog")]
    public class ShopCatalog : ScriptableObject
    {
        [Header("카드 구매 탭 (v3)")]
        [Tooltip("상점에서 파는 카드 (위에서부터 순서대로). 가격은 각 카드의 Shop Price")]
        [SerializeField] List<CardData> cards = new List<CardData>();

        [Header("능력 강화 탭 (v3: 체력 증가만)")]
        [Tooltip("상점에 보일 강화 (위에서부터 순서대로)")]
        [SerializeField] List<ShopUpgradeData> upgrades = new List<ShopUpgradeData>();

        [Header("아이템 구매 탭 (v3에서는 쓰지 않음, 데이터만 보관)")]
        [Tooltip("상점에서 파는 아이템 (위에서부터 순서대로)")]
        [SerializeField] List<ShopItemData> items = new List<ShopItemData>();

        [Header("재료 판매 탭")]
        [Tooltip("판매 목록에 재료를 보여 줄 순서 (여기에 없는 재료는 맨 뒤에)")]
        [SerializeField] List<MaterialData> materialOrder = new List<MaterialData>();

        public IReadOnlyList<CardData> Cards => cards;
        public IReadOnlyList<ShopUpgradeData> Upgrades => upgrades;
        public IReadOnlyList<ShopItemData> Items => items;
        public IReadOnlyList<MaterialData> MaterialOrder => materialOrder;
    }
}
