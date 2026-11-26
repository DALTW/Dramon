using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 재료 판매 탭: 가진 재료만 줄(ShopSellRow Prefab)로 보여 주고, 1개·모두·흔함 재료 전부 팔기를 처리한다.
    /// 희귀 재료를 모두 팔 때는 확인 창을 띄울 수 있다.
    /// </summary>
    public class ShopSellListUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("재료 판매 처리")]
        [SerializeField] MaterialSeller seller;
        [Tooltip("재료를 보여 줄 순서가 들어 있는 상점 목록")]
        [SerializeField] ShopCatalog catalog;
        [Tooltip("줄이 들어갈 곳 (스크롤 Content, Vertical Layout Group)")]
        [SerializeField] RectTransform listRoot;
        [Tooltip("재료 한 줄 Prefab")]
        [SerializeField] ShopSellRow rowPrefab;
        [Tooltip("팔 재료가 없을 때 보여 줄 글")]
        [SerializeField] GameObject emptyLabel;
        [Tooltip("[흔함 재료 전부 팔기] 버튼 (팔 흔함 재료가 없으면 꺼져요)")]
        [SerializeField] Button sellCommonButton;
        [Tooltip("확인 창")]
        [SerializeField] ShopConfirmDialog confirmDialog;
        [Tooltip("거래 안내 문구")]
        [SerializeField] ShopToastUI toast;

        [Header("희귀도 표시")]
        [SerializeField] string commonName = "흔함";
        [SerializeField] Color commonColor = new Color32(0x3B, 0x24, 0x14, 0xFF);
        [SerializeField] string uncommonName = "보통";
        [SerializeField] Color uncommonColor = new Color32(0x1F, 0x5F, 0xA8, 0xFF);
        [SerializeField] string rareName = "희귀";
        [SerializeField] Color rareColor = new Color32(0x8A, 0x2B, 0xB8, 0xFF);

        [Header("버튼 색")]
        [Tooltip("누를 수 있을 때 [흔함 재료 전부 팔기] 버튼 색")]
        [SerializeField] Color enabledButtonColor = Color.white;
        [Tooltip("팔 흔함 재료가 없을 때 버튼 색")]
        [SerializeField] Color disabledButtonColor = new Color(0.55f, 0.52f, 0.48f, 1f);

        [Header("확인 창")]
        [Tooltip("켜면 희귀 재료를 [모두 팔기]할 때 한 번 더 물어봐요")]
        [SerializeField] bool confirmRareSellAll = true;
        [Tooltip("확인 문구. {0} = 재료 이름, {1} = 개수")]
        [SerializeField] string confirmRareFormat = "희귀 재료 {0} {1}개를 정말 팔까요?";

        [Header("안내 문구")]
        [Tooltip("{0} = 재료 이름, {1} = 개수, {2} = 받은 돈")]
        [SerializeField] string soldFormat = "{0} {1}개를 {2} G에 팔았어요";
        [Tooltip("{0} = 판 개수, {1} = 받은 돈")]
        [SerializeField] string soldCommonFormat = "흔함 재료 {0}개를 {1} G에 팔았어요";

        readonly List<ShopSellRow> rows = new List<ShopSellRow>();

        PlayerInventory Inventory => seller != null ? seller.Inventory : null;

        void OnEnable()
        {
            if (Inventory != null) Inventory.Changed += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            if (Inventory != null) Inventory.Changed -= Rebuild;
        }

        /// <summary>[흔함 재료 전부 팔기] 버튼</summary>
        public void SellAllCommon()
        {
            if (seller == null) return;
            var (count, money) = seller.SellAllOfRarity(MaterialRarity.Common);
            if (count > 0 && toast != null) toast.Show(string.Format(soldCommonFormat, count, money), ShopFeedbackKind.Sell);
        }

        void SellOne(MaterialData material) => Sell(material, 1);

        void SellAll(MaterialData material)
        {
            if (Inventory == null || material == null) return;
            int count = Inventory.GetCount(material);
            if (confirmRareSellAll && material.Rarity == MaterialRarity.Rare && confirmDialog != null)
                confirmDialog.Show(string.Format(confirmRareFormat, material.DisplayName, count), () => Sell(material, Inventory.GetCount(material)));
            else
                Sell(material, count);
        }

        void Sell(MaterialData material, int count)
        {
            if (seller == null || Inventory == null || material == null) return;
            int before = Inventory.GetCount(material);
            int money = seller.Sell(material, count);
            int sold = before - Inventory.GetCount(material);
            if (sold > 0 && toast != null) toast.Show(string.Format(soldFormat, material.DisplayName, sold, money), ShopFeedbackKind.Sell);
        }

        void Rebuild()
        {
            foreach (var row in rows) if (row != null) Destroy(row.gameObject);
            rows.Clear();

            var materials = Ordered();
            bool anyCommon = false;
            if (rowPrefab != null && listRoot != null)
            {
                foreach (var m in materials)
                {
                    var row = Instantiate(rowPrefab, listRoot);
                    row.Setup(m, Inventory.GetCount(m), RarityName(m.Rarity), RarityColor(m.Rarity), SellOne, SellAll);
                    rows.Add(row);
                    if (m.Rarity == MaterialRarity.Common) anyCommon = true;
                }
            }
            if (emptyLabel != null) emptyLabel.SetActive(materials.Count == 0);
            if (sellCommonButton != null)
            {
                sellCommonButton.interactable = anyCommon;
                if (sellCommonButton.image != null) sellCommonButton.image.color = anyCommon ? enabledButtonColor : disabledButtonColor;
            }
        }

        /// <summary>가진 재료를 상점 목록 순서대로</summary>
        List<MaterialData> Ordered()
        {
            var result = new List<MaterialData>();
            var inv = Inventory;
            if (inv == null) return result;
            if (catalog != null)
                foreach (var m in catalog.MaterialOrder)
                    if (m != null && inv.GetCount(m) > 0 && !result.Contains(m)) result.Add(m);
            foreach (var pair in inv.Counts)
                if (pair.Key != null && pair.Value > 0 && !result.Contains(pair.Key)) result.Add(pair.Key);
            return result;
        }

        string RarityName(MaterialRarity r) => r == MaterialRarity.Rare ? rareName : r == MaterialRarity.Uncommon ? uncommonName : commonName;
        Color RarityColor(MaterialRarity r) => r == MaterialRarity.Rare ? rareColor : r == MaterialRarity.Uncommon ? uncommonColor : commonColor;
    }
}
