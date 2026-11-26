using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Dramon.EditorTools.DramonEditorUtil;

namespace Dramon.EditorTools
{
    /// <summary>
    /// 상점(재료 판매·능력 강화·아이템 구매)을 지금 열린 Forest Scene에 추가한다.
    /// - 에셋: 강화 4종·아이템 3종 데이터, 상점 목록(ShopCatalog), 줄 Prefab 3개 (Prefabs/UI/Shop)
    /// - Managers/PlayerUpgrades, Managers/PlayerItemBag, Managers/Shop/(MaterialSeller, UpgradePurchaser, ItemPurchaser)
    /// - HUD Canvas/ShopWindow: 상점 창 (탭 3개, 스크롤 목록, 돈 표시, 안내 문구, 확인 창)
    /// - HUD Canvas/TopBar/ShopButton 클릭 → 상점 열기
    /// 이미 있는 에셋·오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결·값도 덮어쓰지 않는다.
    /// </summary>
    public static class ShopInstaller
    {
        const string ShopSettingsFolder = DramonAssetBuilder.SettingsFolder + "/Shop";
        const string UpgradeFolder = ShopSettingsFolder + "/Upgrades";
        const string ItemFolder = ShopSettingsFolder + "/Items";
        const string CatalogPath = ShopSettingsFolder + "/ShopCatalog.asset";
        const string MaterialDataFolder = DramonAssetBuilder.SettingsFolder + "/Materials";
        const string RowPrefabFolder = DramonAssetBuilder.PrefabFolder + "/UI/Shop";
        const string SellRowPath = RowPrefabFolder + "/ShopSellRow.prefab";
        const string UpgradeRowPath = RowPrefabFolder + "/ShopUpgradeRow.prefab";
        const string BuyRowPath = RowPrefabFolder + "/ShopBuyRow.prefab";

        static readonly string[] DragonKeys = { "Ember", "Forest", "Storm", "Shadow", "Gold", "Frost", "Toxic", "Tide", "Stone", "Crystal" };
        static readonly string[] RarityKeys = { "Common", "Uncommon", "Rare" };

        struct UpgradeSpec
        {
            public string file, name, description, icon; public UpgradeStat stat; public int amount; public int[] prices;
        }

        static readonly UpgradeSpec[] Upgrades =
        {
            new UpgradeSpec { file = "Upgrade_SwordTraining", name = "검 단련", description = "검 카드의 공격력이 올라가요.",
                icon = "CardArt_SwordSlash", stat = UpgradeStat.SwordAttack, amount = 1, prices = new[] { 50, 80, 120, 170, 230 } },
            new UpgradeSpec { file = "Upgrade_ShieldReinforce", name = "방패 보강", description = "방패 카드로 막는 피해가 늘어나요.",
                icon = "CardArt_ShieldBlock", stat = UpgradeStat.ShieldDefense, amount = 1, prices = new[] { 50, 80, 120, 170, 230 } },
            new UpgradeSpec { file = "Upgrade_MaxHealth", name = "체력 증가", description = "결투를 시작할 때 최대 체력이 늘어나요.",
                icon = "CardArt_HealingHerb", stat = UpgradeStat.MaxHealth, amount = 10, prices = new[] { 40, 70, 110, 160, 220 } },
            new UpgradeSpec { file = "Upgrade_AttackPower", name = "공격력 증가", description = "모든 공격의 피해가 올라가요.",
                icon = "CardArt_LemonJuice", stat = UpgradeStat.AttackPower, amount = 1, prices = new[] { 60, 90, 130, 180, 240 } },
        };

        struct ItemSpec { public string file, name, description, icon; public int price; }

        static readonly ItemSpec[] Items =
        {
            new ItemSpec { file = "Item_HealingHerb", name = "회복 약초", description = "결투 중에 체력을 회복해요.", icon = "CardArt_HealingHerb", price = 30 },
            new ItemSpec { file = "Item_LemonBomb", name = "레몬 폭탄", description = "드래곤에게 레몬즙을 터뜨려 피해를 줘요.", icon = "CardArt_ThrownLemon", price = 45 },
            new ItemSpec { file = "Item_SmokeRoll", name = "연막 구르기", description = "드래곤의 공격을 한 번 피해요.", icon = "CardArt_DodgeRoll", price = 40 },
        };

        static readonly Color RowBack = new Color(0.23f, 0.14f, 0.08f, 0.10f);
        static readonly Color ReasonColor = new Color(0.62f, 0.14f, 0.1f, 1f);
        static readonly Color ScrollBack = new Color(0.23f, 0.14f, 0.08f, 0.18f);
        static readonly Color ScrollHandle = new Color(0.36f, 0.22f, 0.12f, 0.85f);

        [MenuItem("Dramon/기능 설치 - 상점 (현재 Scene)")]
        public static void InstallFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Dramon", "Play 모드를 끈 뒤 실행해 주세요.", "확인");
                return;
            }
            if (Install())
            {
                var scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Dramon] 상점 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            var managers = GameObject.Find("Managers");
            var hudCanvas = GameObject.Find("HUD Canvas");
            var phase = Object.FindAnyObjectByType<GamePhaseController>(FindObjectsInactive.Include);
            var wallet = Object.FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
            var inventory = Object.FindAnyObjectByType<PlayerInventory>(FindObjectsInactive.Include);
            if (managers == null || hudCanvas == null || phase == null || wallet == null || inventory == null)
            {
                EditorUtility.DisplayDialog("Dramon",
                    "Forest Scene이 열려 있어야 하고, 먼저 '기능 설치 - 결투 둥지 배경·재료 드랍'이 설치돼 있어야 해요.", "확인");
                return false;
            }

            // ── 에셋 ─────────────────────────────
            var catalog = EnsureCatalog();
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();
            DramonAssetBuilder.EnsureFolder(RowPrefabFolder);
            var sellRow = EnsureSellRowPrefab(buttonPrefab);
            var upgradeRow = EnsureUpgradeRowPrefab(buttonPrefab);
            var buyRow = EnsureBuyRowPrefab(buttonPrefab);

            // ── Managers ─────────────────────────
            var upgrades = GetOrAdd<PlayerUpgrades>(FindOrCreateChild(managers, "PlayerUpgrades"));
            SetRefIfEmpty(upgrades, "phaseController", phase);
            var bag = GetOrAdd<PlayerItemBag>(FindOrCreateChild(managers, "PlayerItemBag"));
            SetRefIfEmpty(bag, "phaseController", phase);

            var shopManagers = FindOrCreateChild(managers, "Shop");
            var seller = GetOrAdd<MaterialSeller>(FindOrCreateChild(shopManagers, "MaterialSeller"));
            SetRefIfEmpty(seller, "inventory", inventory);
            SetRefIfEmpty(seller, "wallet", wallet);
            var upgradePurchaser = GetOrAdd<UpgradePurchaser>(FindOrCreateChild(shopManagers, "UpgradePurchaser"));
            SetRefIfEmpty(upgradePurchaser, "upgrades", upgrades);
            SetRefIfEmpty(upgradePurchaser, "wallet", wallet);
            var itemPurchaser = GetOrAdd<ItemPurchaser>(FindOrCreateChild(shopManagers, "ItemPurchaser"));
            SetRefIfEmpty(itemPurchaser, "bag", bag);
            SetRefIfEmpty(itemPurchaser, "wallet", wallet);
            SetRefIfEmpty(itemPurchaser, "phaseController", phase);

            // ── 상점 창 (HUD Canvas/ShopWindow) ──
            var shopUI = Object.FindAnyObjectByType<ShopWindowUI>(FindObjectsInactive.Include);
            if (shopUI == null)
                shopUI = BuildShopWindow(hudCanvas.transform, phase, wallet, inventory, upgrades, catalog, seller, upgradePurchaser, itemPurchaser,
                    buttonPrefab, sellRow, upgradeRow, buyRow);

            // ── HUD 상점 버튼 → 상점 열기 ─────────
            var shopButton = Object.FindAnyObjectByType<ShopButtonUI>(FindObjectsInactive.Include);
            var button = shopButton != null ? shopButton.GetComponent<Button>() : null;
            if (button != null && !HasListener(button.onClick, shopUI, nameof(ShopWindowUI.Open)))
            {
                UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(shopUI.Open));
                RecordInstance(button);
            }
            else if (button == null)
                Debug.LogWarning("[Dramon] HUD 상점 버튼(ShopButtonUI)을 찾지 못했어요. 상점 버튼 OnClick에 ShopWindowUI.Open을 직접 연결해 주세요.");

            AssetDatabase.SaveAssets();
            return true;
        }

        // ── 데이터 에셋 ───────────────────────────
        static ShopCatalog EnsureCatalog()
        {
            DramonAssetBuilder.EnsureFolder(UpgradeFolder);
            DramonAssetBuilder.EnsureFolder(ItemFolder);

            var upgradeAssets = new List<ShopUpgradeData>();
            foreach (var spec in Upgrades)
            {
                string path = $"{UpgradeFolder}/{spec.file}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ShopUpgradeData>(path);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<ShopUpgradeData>();
                    AssetDatabase.CreateAsset(data, path);
                    var so = new SerializedObject(data);
                    so.FindProperty("displayName").stringValue = spec.name;
                    so.FindProperty("icon").objectReferenceValue = DramonAssetBuilder.LoadSprite("Battle/CardArt/" + spec.icon);
                    so.FindProperty("description").stringValue = spec.description;
                    so.FindProperty("stat").enumValueIndex = (int)spec.stat;
                    so.FindProperty("amountPerLevel").intValue = spec.amount;
                    so.FindProperty("maxLevel").intValue = spec.prices.Length;
                    var prices = so.FindProperty("pricesPerLevel");
                    prices.arraySize = spec.prices.Length;
                    for (int i = 0; i < spec.prices.Length; i++) prices.GetArrayElementAtIndex(i).intValue = spec.prices[i];
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(data);
                }
                upgradeAssets.Add(data);
            }

            var itemAssets = new List<ShopItemData>();
            foreach (var spec in Items)
            {
                string path = $"{ItemFolder}/{spec.file}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<ShopItemData>();
                    AssetDatabase.CreateAsset(data, path);
                    var so = new SerializedObject(data);
                    so.FindProperty("displayName").stringValue = spec.name;
                    so.FindProperty("icon").objectReferenceValue = DramonAssetBuilder.LoadSprite("Battle/CardArt/" + spec.icon);
                    so.FindProperty("description").stringValue = spec.description;
                    so.FindProperty("price").intValue = spec.price;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(data);
                }
                itemAssets.Add(data);
            }

            var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ShopCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            var cso = new SerializedObject(catalog);
            FillListIfEmpty(cso.FindProperty("upgrades"), upgradeAssets);
            FillListIfEmpty(cso.FindProperty("items"), itemAssets);
            var materials = new List<MaterialData>();
            for (int i = 0; i < DragonKeys.Length; i++)
            for (int r = 0; r < RarityKeys.Length; r++)
            {
                var m = AssetDatabase.LoadAssetAtPath<MaterialData>($"{MaterialDataFolder}/Mat{i + 1:00}_{DragonKeys[i]}_{RarityKeys[r]}.asset");
                if (m != null) materials.Add(m);
            }
            FillListIfEmpty(cso.FindProperty("materialOrder"), materials);
            cso.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        static void FillListIfEmpty<T>(SerializedProperty list, List<T> values) where T : Object
        {
            if (list.arraySize > 0) return;
            list.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        // ── 줄 Prefab ─────────────────────────────
        static GameObject NewRowRoot(string name, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.layer = LayerMask.NameToLayer("UI");
            ((RectTransform)go.transform).sizeDelta = new Vector2(500, height);
            var back = go.GetComponent<Image>();
            back.color = RowBack;
            back.raycastTarget = true; // 줄 위에서도 스크롤 드래그가 되게
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = height;
            return go;
        }

        static Image NewIcon(Transform parent, float size)
        {
            var go = NewUI("Icon", parent);
            SetAnchored((RectTransform)go.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(size, size));
            var icon = go.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return icon;
        }

        static Text Left(string name, Transform parent, float x, float y, float width, int size, string text, bool top)
        {
            var anchor = new Vector2(0, top ? 1f : 0f);
            return NewText(name, parent, anchor, anchor, new Vector2(x, y), new Vector2(width, 18), TextAnchor.MiddleLeft, size, text);
        }

        static Text Right(string name, Transform parent, float x, float y, float width, int size, string text, bool top)
        {
            var anchor = new Vector2(1, top ? 1f : 0f);
            return NewText(name, parent, anchor, anchor, new Vector2(x, y), new Vector2(width, 18), TextAnchor.MiddleRight, size, text);
        }

        static Button RowButton(Button prefab, Transform parent, string name, string label, float x, float width)
        {
            var b = NewButton(prefab, parent, name, label);
            SetAnchored((RectTransform)b.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(x, 0), new Vector2(width, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(b.transform);
            return b;
        }

        static T SaveRow<T>(GameObject go, string path) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<T>();
        }

        static ShopSellRow EnsureSellRowPrefab(Button buttonPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ShopSellRow>(SellRowPath);
            if (existing != null) return existing;

            var go = NewRowRoot("ShopSellRow", 30);
            var icon = NewIcon(go.transform, 26);
            var mid = new Vector2(0, 0.5f);
            var name = NewText("Name", go.transform, mid, mid, new Vector2(36, 0), new Vector2(130, 18), TextAnchor.MiddleLeft, 11, "재료 이름");
            var rarity = NewText("Rarity", go.transform, mid, mid, new Vector2(170, 0), new Vector2(40, 18), TextAnchor.MiddleLeft, 10, "흔함");
            var count = NewText("Count", go.transform, mid, mid, new Vector2(214, 0), new Vector2(44, 18), TextAnchor.MiddleRight, 11, "x1");
            var price = NewText("Price", go.transform, mid, mid, new Vector2(262, 0), new Vector2(64, 18), TextAnchor.MiddleRight, 11, "15 G");
            var sellOne = RowButton(buttonPrefab, go.transform, "SellOneButton", "1개 팔기", -84, 76);
            var sellAll = RowButton(buttonPrefab, go.transform, "SellAllButton", "모두 팔기", -4, 76);

            var row = go.AddComponent<ShopSellRow>();
            SetRef(row, "icon", icon);
            SetRef(row, "nameLabel", name);
            SetRef(row, "rarityLabel", rarity);
            SetRef(row, "countLabel", count);
            SetRef(row, "priceLabel", price);
            SetRef(row, "sellOneButton", sellOne);
            SetRef(row, "sellAllButton", sellAll);
            return SaveRow<ShopSellRow>(go, SellRowPath);
        }

        static ShopUpgradeRow EnsureUpgradeRowPrefab(Button buttonPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ShopUpgradeRow>(UpgradeRowPath);
            if (existing != null) return existing;

            var go = NewRowRoot("ShopUpgradeRow", 46);
            var icon = NewIcon(go.transform, 38);
            var name = Left("Name", go.transform, 48, -3, 150, 12, "강화 이름", true);
            var desc = Left("Description", go.transform, 48, 3, 200, 10, "설명", false);
            var level = Left("Level", go.transform, 252, -3, 90, 11, "단계 0 / 5", true);
            var effect = Left("Effect", go.transform, 252, 3, 130, 10, "검 공격력 +0 → +1", false);
            var price = Right("Price", go.transform, -74, -3, 64, 12, "50 G", true);
            var reason = Right("Reason", go.transform, -74, 3, 110, 10, "", false);
            reason.color = ReasonColor;
            var buy = RowButton(buttonPrefab, go.transform, "UpgradeButton", "강화", -4, 64);

            var row = go.AddComponent<ShopUpgradeRow>();
            SetRef(row, "icon", icon);
            SetRef(row, "nameLabel", name);
            SetRef(row, "descriptionLabel", desc);
            SetRef(row, "levelLabel", level);
            SetRef(row, "effectLabel", effect);
            SetRef(row, "priceLabel", price);
            SetRef(row, "reasonLabel", reason);
            SetRef(row, "buyButton", buy);
            return SaveRow<ShopUpgradeRow>(go, UpgradeRowPath);
        }

        static ShopBuyRow EnsureBuyRowPrefab(Button buttonPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ShopBuyRow>(BuyRowPath);
            if (existing != null) return existing;

            var go = NewRowRoot("ShopBuyRow", 46);
            var icon = NewIcon(go.transform, 38);
            var name = Left("Name", go.transform, 48, -3, 150, 12, "아이템 이름", true);
            var desc = Left("Description", go.transform, 48, 3, 200, 10, "설명", false);
            var owned = Left("Owned", go.transform, 252, -3, 90, 11, "보유 0 / 5", true);
            var stock = Left("Stock", go.transform, 252, 3, 110, 10, "재고 무제한", false);
            var price = Right("Price", go.transform, -74, -3, 64, 12, "30 G", true);
            var reason = Right("Reason", go.transform, -74, 3, 110, 10, "", false);
            reason.color = ReasonColor;
            var buy = RowButton(buttonPrefab, go.transform, "BuyButton", "구매", -4, 64);

            var row = go.AddComponent<ShopBuyRow>();
            SetRef(row, "icon", icon);
            SetRef(row, "nameLabel", name);
            SetRef(row, "descriptionLabel", desc);
            SetRef(row, "priceLabel", price);
            SetRef(row, "ownedLabel", owned);
            SetRef(row, "stockLabel", stock);
            SetRef(row, "reasonLabel", reason);
            SetRef(row, "buyButton", buy);
            return SaveRow<ShopBuyRow>(go, BuyRowPath);
        }

        // ── 상점 창 ───────────────────────────────
        static ShopWindowUI BuildShopWindow(Transform canvas, GamePhaseController phase, PlayerWallet wallet, PlayerInventory inventory,
            PlayerUpgrades upgrades, ShopCatalog catalog, MaterialSeller seller, UpgradePurchaser upgradePurchaser, ItemPurchaser itemPurchaser,
            Button buttonPrefab, ShopSellRow sellRow, ShopUpgradeRow upgradeRow, ShopBuyRow buyRow)
        {
            var root = NewUI("ShopWindow", canvas);
            Stretch((RectTransform)root.transform);
            var overlay = canvas.Find("EncounterOverlay");
            if (overlay != null) root.transform.SetSiblingIndex(overlay.GetSiblingIndex()); // 드래곤 등장 연출보다 아래
            var ui = root.AddComponent<ShopWindowUI>();
            var dialog = root.AddComponent<ShopConfirmDialog>();

            var panel = NewUI("Panel", root.transform);
            Stretch((RectTransform)panel.transform);

            var dim = NewUI("Dim", panel.transform);
            Stretch((RectTransform)dim.transform);
            dim.AddComponent<Image>().color = new Color(0, 0, 0, 0.55f); // 뒤쪽 HUD 클릭 막기

            var window = NewWindow("Window", panel.transform);
            SetAnchored(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -12), new Vector2(560, 300));
            var top = new Vector2(0, 1);
            var topRight = new Vector2(1, 1);

            NewText("Title", window.transform, top, top, new Vector2(18, -12), new Vector2(120, 22), TextAnchor.MiddleLeft, 15, "상점");

            // 돈 표시
            var money = NewUI("Money", window.transform);
            SetAnchored((RectTransform)money.transform, topRight, topRight, new Vector2(-76, -12), new Vector2(120, 22));
            var coinGO = NewUI("CoinIcon", money.transform);
            SetAnchored((RectTransform)coinGO.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(16, 16));
            var coin = coinGO.AddComponent<Image>();
            coin.preserveAspect = true;
            coin.raycastTarget = false;
            coin.enabled = false; // 동전 그림이 생기면 Sprite를 넣고 켜 주세요
            var moneyLabel = NewText("MoneyText", money.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(100, 22),
                TextAnchor.MiddleRight, 14, "0 G");
            var moneyUI = money.AddComponent<ShopMoneyUI>();
            SetRef(moneyUI, "wallet", wallet);
            SetRef(moneyUI, "label", moneyLabel);
            SetRef(moneyUI, "coinIcon", coin);

            var close = NewButton(buttonPrefab, window.transform, "CloseButton", "닫기");
            SetAnchored((RectTransform)close.transform, topRight, topRight, new Vector2(-12, -11), new Vector2(56, 24));
            RecordInstance(close.transform);

            // 탭
            var tabs = NewUI("Tabs", window.transform);
            SetAnchored((RectTransform)tabs.transform, top, top, new Vector2(16, -40), new Vector2(320, 24));
            string[] tabNames = { "재료 판매", "능력 강화", "아이템 구매" };
            string[] tabObjects = { "SellTab", "UpgradeTab", "BuyTab" };
            var tabButtons = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                tabButtons[i] = NewButton(buttonPrefab, tabs.transform, tabObjects[i], tabNames[i]);
                SetAnchored((RectTransform)tabButtons[i].transform, top, top, new Vector2(i * 102, 0), new Vector2(96, 24));
                RecordInstance(tabButtons[i].transform);
            }

            // 페이지
            var pages = NewUI("Pages", window.transform);
            var pagesRT = (RectTransform)pages.transform;
            pagesRT.anchorMin = Vector2.zero; pagesRT.anchorMax = Vector2.one; pagesRT.pivot = new Vector2(0.5f, 0.5f);
            pagesRT.offsetMin = new Vector2(16, 34); pagesRT.offsetMax = new Vector2(-16, -70);

            // 안내 문구 (창 아래)
            var toastText = NewText("Toast", window.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 11), new Vector2(520, 18),
                TextAnchor.MiddleCenter, 11, "");
            var audio = toastText.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            var toast = toastText.gameObject.AddComponent<ShopToastUI>();
            SetRef(toast, "label", toastText);
            SetRef(toast, "audioSource", audio);

            // 재료 판매
            var sellPage = NewPage("SellPage", pages.transform);
            var sellContent = NewScroll(sellPage.transform, 30);
            var empty = NewText("EmptyLabel", sellPage.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 14), new Vector2(300, 20),
                TextAnchor.MiddleCenter, 12, "팔 재료가 없어요");
            empty.gameObject.SetActive(false);
            var sellCommon = NewButton(buttonPrefab, sellPage.transform, "SellAllCommonButton", "흔함 재료 전부 팔기");
            SetAnchored((RectTransform)sellCommon.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(132, 24));
            RecordInstance(sellCommon.transform);
            var sellList = sellPage.AddComponent<ShopSellListUI>();
            SetRef(sellList, "seller", seller);
            SetRef(sellList, "catalog", catalog);
            SetRef(sellList, "listRoot", sellContent);
            SetRef(sellList, "rowPrefab", sellRow);
            SetRef(sellList, "emptyLabel", empty.gameObject);
            SetRef(sellList, "sellCommonButton", sellCommon);
            SetRef(sellList, "confirmDialog", dialog);
            SetRef(sellList, "toast", toast);
            UnityEventTools.AddPersistentListener(sellCommon.onClick, new UnityAction(sellList.SellAllCommon));
            RecordInstance(sellCommon);

            // 능력 강화
            var upgradePage = NewPage("UpgradePage", pages.transform);
            var upgradeContent = NewScroll(upgradePage.transform, 0);
            var upgradeList = upgradePage.AddComponent<ShopUpgradeListUI>();
            SetRef(upgradeList, "purchaser", upgradePurchaser);
            SetRef(upgradeList, "wallet", wallet);
            SetRef(upgradeList, "catalog", catalog);
            SetRef(upgradeList, "listRoot", upgradeContent);
            SetRef(upgradeList, "rowPrefab", upgradeRow);
            SetRef(upgradeList, "toast", toast);

            // 아이템 구매
            var buyPage = NewPage("BuyPage", pages.transform);
            var buyContent = NewScroll(buyPage.transform, 0);
            var buyList = buyPage.AddComponent<ShopBuyListUI>();
            SetRef(buyList, "purchaser", itemPurchaser);
            SetRef(buyList, "wallet", wallet);
            SetRef(buyList, "catalog", catalog);
            SetRef(buyList, "listRoot", buyContent);
            SetRef(buyList, "rowPrefab", buyRow);
            SetRef(buyList, "toast", toast);

            upgradePage.SetActive(false);
            buyPage.SetActive(false);

            // 확인 창 (상점 창 위)
            var confirmRoot = NewUI("ConfirmDialog", panel.transform);
            Stretch((RectTransform)confirmRoot.transform);
            confirmRoot.AddComponent<Image>().color = new Color(0, 0, 0, 0.35f);
            var confirmWindow = NewWindow("Window", confirmRoot.transform);
            SetAnchored(confirmWindow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -12), new Vector2(280, 120));
            var question = NewText("Message", confirmWindow.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(240, 48),
                TextAnchor.MiddleCenter, 12, "정말 팔까요?");
            var yes = NewButton(buttonPrefab, confirmWindow.transform, "YesButton", "예");
            SetAnchored((RectTransform)yes.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-40, 14), new Vector2(64, 24));
            RecordInstance(yes.transform);
            var no = NewButton(buttonPrefab, confirmWindow.transform, "NoButton", "아니요");
            SetAnchored((RectTransform)no.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(40, 14), new Vector2(64, 24));
            RecordInstance(no.transform);
            confirmRoot.SetActive(false);

            SetRef(dialog, "window", confirmRoot);
            SetRef(dialog, "messageLabel", question);
            UnityEventTools.AddPersistentListener(yes.onClick, new UnityAction(dialog.Confirm));
            RecordInstance(yes);
            UnityEventTools.AddPersistentListener(no.onClick, new UnityAction(dialog.Cancel));
            RecordInstance(no);

            // ShopWindowUI 연결
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "panel", panel);
            SetRef(ui, "confirmDialog", dialog);
            var so = new SerializedObject(ui);
            var tb = so.FindProperty("tabButtons");
            var tp = so.FindProperty("tabPages");
            var pageObjects = new[] { sellPage, upgradePage, buyPage };
            tb.arraySize = 3;
            tp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                tb.GetArrayElementAtIndex(i).objectReferenceValue = tabButtons[i];
                tp.GetArrayElementAtIndex(i).objectReferenceValue = pageObjects[i];
            }
            so.FindProperty("testWallet").objectReferenceValue = wallet;
            so.FindProperty("testInventory").objectReferenceValue = inventory;
            so.FindProperty("testUpgrades").objectReferenceValue = upgrades;
            so.FindProperty("testCatalog").objectReferenceValue = catalog;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(close.onClick, new UnityAction(ui.Close));
            RecordInstance(close);
            for (int i = 0; i < 3; i++)
            {
                UnityEventTools.AddIntPersistentListener(tabButtons[i].onClick, new UnityAction<int>(ui.SelectTab), i);
                RecordInstance(tabButtons[i]);
            }

            panel.SetActive(false);
            return ui;
        }

        static GameObject NewPage(string name, Transform parent)
        {
            var page = NewUI(name, parent);
            Stretch((RectTransform)page.transform);
            return page;
        }

        /// <summary>세로 스크롤 목록을 만들고 줄이 들어갈 Content를 돌려준다. bottomSpace = 아래쪽에 비워 둘 높이(버튼 자리).</summary>
        static RectTransform NewScroll(Transform page, float bottomSpace)
        {
            var scroll = NewUI("Scroll", page);
            var rt = (RectTransform)scroll.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(0, bottomSpace); rt.offsetMax = Vector2.zero;
            var hit = scroll.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0); // 빈 곳에서도 휠·드래그가 되게

            var viewport = NewUI("Viewport", scroll.transform);
            var vrt = (RectTransform)viewport.transform;
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one; vrt.pivot = new Vector2(0, 1);
            vrt.offsetMin = Vector2.zero; vrt.offsetMax = new Vector2(-12, 0);
            viewport.AddComponent<RectMask2D>();

            var content = NewUI("Content", viewport.transform);
            var crt = (RectTransform)content.transform;
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1); crt.pivot = new Vector2(0.5f, 1);
            crt.anchoredPosition = Vector2.zero; crt.sizeDelta = Vector2.zero;
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bar = NewUI("Scrollbar", scroll.transform);
            var brt = (RectTransform)bar.transform;
            brt.anchorMin = new Vector2(1, 0); brt.anchorMax = Vector2.one; brt.pivot = new Vector2(1, 1);
            brt.offsetMin = new Vector2(-8, 0); brt.offsetMax = Vector2.zero;
            bar.AddComponent<Image>().color = ScrollBack;
            var area = NewUI("Sliding Area", bar.transform);
            Stretch((RectTransform)area.transform);
            var handle = NewUI("Handle", area.transform);
            Stretch((RectTransform)handle.transform);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = ScrollHandle;
            var scrollbar = bar.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.handleRect = (RectTransform)handle.transform;
            scrollbar.targetGraphic = handleImage;

            var sr = scroll.AddComponent<ScrollRect>();
            sr.content = crt;
            sr.viewport = vrt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 20;
            sr.verticalScrollbar = scrollbar;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            sr.verticalScrollbarSpacing = 4;
            return crt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void RecordInstance(Object o)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(o)) PrefabUtility.RecordPrefabInstancePropertyModifications(o);
        }

        static bool HasListener(UnityEventBase evt, Object target, string method)
        {
            for (int i = 0; i < evt.GetPersistentEventCount(); i++)
                if (evt.GetPersistentTarget(i) == target && evt.GetPersistentMethodName(i) == method) return true;
            return false;
        }

        static void SetRefIfEmpty(Object target, string property, Object value)
        {
            if (target == null || value == null) return;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null) { Debug.LogError($"[Dramon] {target.GetType().Name}에 '{property}' 항목이 없어요."); return; }
            if (prop.objectReferenceValue != null) return;
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
