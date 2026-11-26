using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 아이템 구매 탭: 상점 목록의 아이템마다 줄(ShopBuyRow Prefab)을 만들고, [구매]를 처리한다.
    /// 품절·최대 보유·돈 부족이면 버튼을 끄고 이유를 보여 준다. (아이템 효과는 카드 결투 단계에서 사용)
    /// </summary>
    public class ShopBuyListUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("아이템 구매 처리")]
        [SerializeField] ItemPurchaser purchaser;
        [Tooltip("돈이 바뀌면 버튼 상태를 다시 계산해요")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("상점에서 파는 아이템 목록")]
        [SerializeField] ShopCatalog catalog;
        [Tooltip("줄이 들어갈 곳 (스크롤 Content, Vertical Layout Group)")]
        [SerializeField] RectTransform listRoot;
        [Tooltip("아이템 한 줄 Prefab")]
        [SerializeField] ShopBuyRow rowPrefab;
        [Tooltip("거래 안내 문구")]
        [SerializeField] ShopToastUI toast;

        [Header("표시")]
        [Tooltip("{0} = 가격")]
        [SerializeField] string priceFormat = "{0} G";
        [Tooltip("{0} = 가진 개수 (최대 보유 제한이 없을 때)")]
        [SerializeField] string ownedFormat = "보유 {0}";
        [Tooltip("{0} = 가진 개수, {1} = 최대 보유 개수")]
        [SerializeField] string ownedMaxFormat = "보유 {0} / {1}";
        [Tooltip("{0} = 남은 재고")]
        [SerializeField] string stockFormat = "재고 {0}";
        [SerializeField] string unlimitedStockText = "재고 무제한";
        [SerializeField] string soldOutText = "품절";
        [SerializeField] string bagFullText = "더 가질 수 없어요";
        [SerializeField] string notEnoughMoneyText = "돈이 모자라요";

        [Header("안내 문구")]
        [Tooltip("{0} = 아이템 이름, {1} = 낸 돈")]
        [SerializeField] string boughtFormat = "{0}을(를) 샀어요 (-{1} G)";

        readonly List<ShopBuyRow> rows = new List<ShopBuyRow>();

        PlayerItemBag Bag => purchaser != null ? purchaser.Bag : null;

        void OnEnable()
        {
            if (wallet != null) wallet.MoneyChanged += HandleMoney;
            if (Bag != null) Bag.Changed += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            if (wallet != null) wallet.MoneyChanged -= HandleMoney;
            if (Bag != null) Bag.Changed -= Rebuild;
        }

        void HandleMoney(int _) => Rebuild();

        void Buy(ShopItemData item)
        {
            if (purchaser == null) return;
            if (purchaser.TryBuy(item))
            {
                if (toast != null) toast.Show(string.Format(boughtFormat, item.DisplayName, item.Price), ShopFeedbackKind.Buy);
            }
            else if (toast != null)
            {
                toast.Show(Reason(purchaser.Check(item)), ShopFeedbackKind.Fail);
            }
        }

        void Rebuild()
        {
            foreach (var row in rows) if (row != null) Destroy(row.gameObject);
            rows.Clear();
            if (catalog == null || rowPrefab == null || listRoot == null || purchaser == null || Bag == null) return;

            foreach (var item in catalog.Items)
            {
                if (item == null) continue;
                int owned = Bag.GetCount(item);
                int stock = purchaser.RemainingStock(item);
                var block = purchaser.Check(item);
                var row = Instantiate(rowPrefab, listRoot);
                row.Setup(item,
                    string.Format(priceFormat, item.Price),
                    item.MaxOwned > 0 ? string.Format(ownedMaxFormat, owned, item.MaxOwned) : string.Format(ownedFormat, owned),
                    stock < 0 ? unlimitedStockText : string.Format(stockFormat, stock),
                    Reason(block),
                    block == ItemPurchaser.Block.None, Buy);
                rows.Add(row);
            }
        }

        string Reason(ItemPurchaser.Block block)
        {
            switch (block)
            {
                case ItemPurchaser.Block.SoldOut: return soldOutText;
                case ItemPurchaser.Block.BagFull: return bagFullText;
                case ItemPurchaser.Block.NotEnoughMoney: return notEnoughMoneyText;
                default: return "";
            }
        }
    }
}
