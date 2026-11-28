using System.Collections.Generic;
using System.Linq;
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
    /// v3 3단계: 상점 개편을 지금 열린 Forest Scene에 추가한다. (상점과 v3 2단계가 먼저 설치돼 있어야 함)
    /// - 탭: 재료 판매 / 카드 구매 / 카드 강화 / 덱(보기·제거) / 체력 증가. 예전 아이템 구매 탭은 숨김(지우지 않음)
    /// - ShopCatalog: 카드 목록 채우기(처음 한 번), 강화 목록에서 검 단련·방패 보강·공격력 증가 빼기, 아이템 목록 비우기 (에셋은 그대로)
    /// - Managers/Shop: CardPurchaser·CardUpgrader·CardRemover, Prefabs/UI/Shop/ShopCardRow
    /// - 스테이지 클리어 창에 [상점] 버튼
    /// 이미 있는 오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결은 덮어쓰지 않는다.
    /// </summary>
    public static class ShopV3Installer
    {
        const string CatalogPath = DramonAssetBuilder.SettingsFolder + "/Shop/ShopCatalog.asset";
        const string CardFolder = DramonAssetBuilder.SettingsFolder + "/Cards";
        const string RowPath = DramonAssetBuilder.PrefabFolder + "/UI/Shop/ShopCardRow.prefab";
        static readonly string[] RemovedUpgrades = { "Upgrade_SwordTraining", "Upgrade_ShieldReinforce", "Upgrade_AttackPower" };
        static readonly Color RowBack = new Color(0.23f, 0.14f, 0.08f, 0.10f);
        static readonly Color ReasonColor = new Color(0.62f, 0.14f, 0.1f, 1f);

        [MenuItem("Dramon/기능 설치 - v3 3단계 (상점 개편, 현재 Scene)")]
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
                Debug.Log("[Dramon] v3 3단계 설치 완료: " + scene.path);
            }
        }

        static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        public static bool Install()
        {
            if (Find<StageProgress>() == null && !StageProgressInstaller.Install()) return false;

            var shopUI = Find<ShopWindowUI>();
            var upgradePurchaser = Find<UpgradePurchaser>();
            var wallet = Find<PlayerWallet>();
            var deck = Find<PlayerDeck>();
            var cardUpgrades = Find<CardUpgradeState>();
            var calculator = Find<CardStatCalculator>();
            var phase = Find<GamePhaseController>();
            var playerUpgrades = Find<PlayerUpgrades>();
            var progress = Find<StageProgress>();
            var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CatalogPath);
            var window = shopUI != null ? shopUI.transform.Find("Panel/Window") : null;
            if (shopUI == null || upgradePurchaser == null || wallet == null || deck == null || cardUpgrades == null || catalog == null || window == null)
            {
                EditorUtility.DisplayDialog("Dramon", "상점('기능 설치 - 상점')과 카드 결투·v3 1단계가 먼저 설치돼 있어야 해요.", "확인");
                return false;
            }
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();

            // ── 데이터 ───────────────────────────
            UpdateCatalog(catalog);
            var rowPrefab = EnsureRowPrefab(buttonPrefab);

            // ── Managers/Shop ───────────────────
            var shopRoot = upgradePurchaser.transform.parent != null ? upgradePurchaser.transform.parent.gameObject : upgradePurchaser.gameObject;
            var cardPurchaser = GetOrAdd<CardPurchaser>(FindOrCreateChild(shopRoot, "CardPurchaser"));
            var cardUpgrader = GetOrAdd<CardUpgrader>(FindOrCreateChild(shopRoot, "CardUpgrader"));
            var cardRemover = GetOrAdd<CardRemover>(FindOrCreateChild(shopRoot, "CardRemover"));
            SetRefIfEmpty(cardPurchaser, "wallet", wallet);
            SetRefIfEmpty(cardPurchaser, "deck", deck);
            SetRefIfEmpty(cardUpgrader, "wallet", wallet);
            SetRefIfEmpty(cardUpgrader, "upgrades", cardUpgrades);
            SetRefIfEmpty(cardUpgrader, "deck", deck);
            SetRefIfEmpty(cardRemover, "wallet", wallet);
            SetRefIfEmpty(cardRemover, "deck", deck);
            SetRefIfEmpty(cardRemover, "phaseController", phase);
            SetRefIfEmpty(progress, "playerUpgrades", playerUpgrades);

            // ── 상점 창 탭·페이지 ────────────────
            BuildTabs(shopUI, window, buttonPrefab, rowPrefab, catalog, cardPurchaser, cardUpgrader, cardRemover, calculator, cardUpgrades);

            // ── 스테이지 클리어 창 [상점] ─────────
            AddStageClearShopButton(shopUI, buttonPrefab);

            // 스테이지 번호가 상점 창 위에 그려지지 않게 (상점 창보다 앞 순서로)
            var stageLabel = shopUI.transform.parent != null ? shopUI.transform.parent.Find("StageLabel") : null;
            if (stageLabel != null && stageLabel.GetSiblingIndex() > shopUI.transform.GetSiblingIndex())
                stageLabel.SetSiblingIndex(shopUI.transform.GetSiblingIndex());

            AssetDatabase.SaveAssets();
            return true;
        }

        // ── 상점 목록 ────────────────────────────
        static void UpdateCatalog(ShopCatalog catalog)
        {
            var so = new SerializedObject(catalog);
            var cards = so.FindProperty("cards");
            if (cards.arraySize > 0) return; // 이미 v3로 정리됨 (사용자가 고친 목록 그대로)

            var list = new List<CardData>();
            foreach (var guid in AssetDatabase.FindAssets("t:CardData", new[] { CardFolder }))
            {
                var c = AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid));
                if (c != null && c.ShopPrice > 0) list.Add(c);
            }
            list = list.OrderBy(c => c.ShopPrice).ThenBy(c => c.name).ToList();
            cards.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++) cards.GetArrayElementAtIndex(i).objectReferenceValue = list[i];

            // 능력 강화는 체력 증가만 (에셋은 지우지 않음)
            var ups = so.FindProperty("upgrades");
            for (int i = ups.arraySize - 1; i >= 0; i--)
            {
                var u = ups.GetArrayElementAtIndex(i).objectReferenceValue;
                if (u == null || RemovedUpgrades.Contains(u.name))
                {
                    ups.GetArrayElementAtIndex(i).objectReferenceValue = null;
                    ups.DeleteArrayElementAtIndex(i);
                }
            }
            // 아이템 구매는 v3에서 쓰지 않음 (에셋은 그대로)
            so.FindProperty("items").arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        // ── 카드 한 줄 Prefab ───────────────────
        static ShopCardRow EnsureRowPrefab(Button buttonPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ShopCardRow>(RowPath);
            if (existing != null) return existing;

            var go = new GameObject("ShopCardRow", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.layer = LayerMask.NameToLayer("UI");
            ((RectTransform)go.transform).sizeDelta = new Vector2(500, 50);
            var back = go.GetComponent<Image>();
            back.color = RowBack;
            back.raycastTarget = true;
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = 50;

            var imgGO = NewUI("CardImage", go.transform);
            SetAnchored((RectTransform)imgGO.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(30, 46));
            var img = imgGO.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;

            var tl = new Vector2(0, 1); var bl = new Vector2(0, 0); var tr = new Vector2(1, 1); var br = new Vector2(1, 0);
            var name = NewText("Name", go.transform, tl, tl, new Vector2(40, -4), new Vector2(140, 18), TextAnchor.MiddleLeft, 12, "카드 이름");
            var cost = NewText("Cost", go.transform, tl, tl, new Vector2(184, -4), new Vector2(90, 18), TextAnchor.MiddleLeft, 10, "코스트 1");
            var info = NewText("Info", go.transform, tl, tl, new Vector2(278, -4), new Vector2(120, 18), TextAnchor.MiddleLeft, 10, "덱에 0장");
            var desc = NewText("Description", go.transform, bl, bl, new Vector2(40, 4), new Vector2(360, 22), TextAnchor.MiddleLeft, 10, "피해 6");
            desc.supportRichText = true;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Truncate;
            var price = NewText("Price", go.transform, tr, tr, new Vector2(-74, -4), new Vector2(64, 18), TextAnchor.MiddleRight, 12, "60 G");
            var reason = NewText("Reason", go.transform, br, br, new Vector2(-74, 4), new Vector2(110, 18), TextAnchor.MiddleRight, 10, "");
            reason.color = ReasonColor;
            var button = NewButton(buttonPrefab, go.transform, "ActionButton", "구매");
            SetAnchored((RectTransform)button.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-4, 0), new Vector2(64, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(button.transform);
            var buttonText = button.GetComponentInChildren<Text>();

            var row = go.AddComponent<ShopCardRow>();
            SetRef(row, "cardImage", img);
            SetRef(row, "nameLabel", name);
            SetRef(row, "costLabel", cost);
            SetRef(row, "descriptionLabel", desc);
            SetRef(row, "infoLabel", info);
            SetRef(row, "priceLabel", price);
            SetRef(row, "reasonLabel", reason);
            SetRef(row, "actionButton", button);
            SetRef(row, "actionLabel", buttonText);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, RowPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<ShopCardRow>();
        }

        // ── 탭 ──────────────────────────────────
        static void BuildTabs(ShopWindowUI ui, Transform window, Button buttonPrefab, ShopCardRow rowPrefab, ShopCatalog catalog,
            CardPurchaser purchaser, CardUpgrader upgrader, CardRemover remover, CardStatCalculator calculator, CardUpgradeState cardUpgrades)
        {
            var tabs = window.Find("Tabs");
            var pages = window.Find("Pages");
            if (tabs == null || pages == null || tabs.Find("CardBuyTab") != null) return; // 이미 설치됨

            var toast = window.GetComponentInChildren<ShopToastUI>(true);
            var dialog = ui.GetComponent<ShopConfirmDialog>();
            var top = new Vector2(0, 1);

            var sellTab = tabs.Find("SellTab")?.GetComponent<Button>();
            var upgradeTab = tabs.Find("UpgradeTab")?.GetComponent<Button>();
            var buyTab = tabs.Find("BuyTab")?.GetComponent<Button>();
            var sellPage = pages.Find("SellPage")?.gameObject;
            var upgradePage = pages.Find("UpgradePage")?.gameObject;
            var buyPage = pages.Find("BuyPage")?.gameObject;

            Button NewTab(string obj, string label)
            {
                var b = NewButton(buttonPrefab, tabs, obj, label);
                return b;
            }
            var cardBuyTab = NewTab("CardBuyTab", "카드 구매");
            var cardUpgradeTab = NewTab("CardUpgradeTab", "카드 강화");
            var deckTab = NewTab("DeckTab", "덱");

            // 체력 증가 (예전 능력 강화 탭)
            if (upgradeTab != null)
            {
                var label = upgradeTab.GetComponentInChildren<Text>();
                if (label != null && label.text == "능력 강화")
                {
                    label.text = "체력 증가";
                    RecordInstance(label);
                }
            }
            // 예전 아이템 구매 탭은 숨김 (지우지 않음, 켜면 다시 보여요)
            if (buyTab != null) buyTab.gameObject.SetActive(false);
            if (buyPage != null) buyPage.SetActive(false);

            var order = new[] { sellTab, cardBuyTab, cardUpgradeTab, deckTab, upgradeTab };
            ((RectTransform)tabs).sizeDelta = new Vector2(order.Length * 102, 24);
            for (int i = 0; i < order.Length; i++)
            {
                if (order[i] == null) continue;
                SetAnchored((RectTransform)order[i].transform, top, top, new Vector2(i * 102, 0), new Vector2(96, 24));
                RecordInstance(order[i].transform);
            }

            // 카드 구매
            var cardBuyPage = ShopInstaller.NewPage("CardBuyPage", pages);
            var buyContent = ShopInstaller.NewScroll(cardBuyPage.transform, 0);
            var buyList = cardBuyPage.AddComponent<ShopCardBuyListUI>();
            SetRef(buyList, "purchaser", purchaser);
            SetRef(buyList, "catalog", catalog);
            SetRef(buyList, "calculator", calculator);
            SetRef(buyList, "cardUpgrades", cardUpgrades);
            SetRef(buyList, "listRoot", buyContent);
            SetRef(buyList, "rowPrefab", rowPrefab);
            SetRef(buyList, "toast", toast);

            // 카드 강화
            var cardUpgradePage = ShopInstaller.NewPage("CardUpgradePage", pages);
            var upContent = ShopInstaller.NewScroll(cardUpgradePage.transform, 0);
            var empty = NewText("EmptyLabel", cardUpgradePage.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 20),
                TextAnchor.MiddleCenter, 12, "강화할 카드가 없어요");
            empty.gameObject.SetActive(false);
            var upList = cardUpgradePage.AddComponent<ShopCardUpgradeListUI>();
            SetRef(upList, "upgrader", upgrader);
            SetRef(upList, "calculator", calculator);
            SetRef(upList, "listRoot", upContent);
            SetRef(upList, "rowPrefab", rowPrefab);
            SetRef(upList, "toast", toast);
            SetRef(upList, "emptyLabel", empty.gameObject);

            // 덱 (보기·제거)
            var deckPage = ShopInstaller.NewPage("DeckPage", pages);
            var deckContent = ShopInstaller.NewScroll(deckPage.transform, 22);
            var summary = NewText("Summary", deckPage.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(500, 20),
                TextAnchor.MiddleLeft, 11, "덱 10장 · 카드 제거 50 G");
            var deckList = deckPage.AddComponent<ShopDeckListUI>();
            SetRef(deckList, "remover", remover);
            SetRef(deckList, "calculator", calculator);
            SetRef(deckList, "cardUpgrades", cardUpgrades);
            SetRef(deckList, "listRoot", deckContent);
            SetRef(deckList, "rowPrefab", rowPrefab);
            SetRef(deckList, "toast", toast);
            SetRef(deckList, "confirmDialog", dialog);
            SetRef(deckList, "summaryLabel", summary);

            cardBuyPage.SetActive(false);
            cardUpgradePage.SetActive(false);
            deckPage.SetActive(false);

            // ShopWindowUI 탭 배열과 버튼 연결 (새 순서)
            var pageOrder = new[] { sellPage, cardBuyPage, cardUpgradePage, deckPage, upgradePage };
            var so = new SerializedObject(ui);
            var tb = so.FindProperty("tabButtons");
            var tp = so.FindProperty("tabPages");
            tb.arraySize = order.Length;
            tp.arraySize = order.Length;
            for (int i = 0; i < order.Length; i++)
            {
                tb.GetArrayElementAtIndex(i).objectReferenceValue = order[i];
                tp.GetArrayElementAtIndex(i).objectReferenceValue = pageOrder[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            for (int i = 0; i < order.Length; i++)
            {
                var b = order[i];
                if (b == null) continue;
                for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--)
                    if (b.onClick.GetPersistentTarget(k) == ui && b.onClick.GetPersistentMethodName(k) == nameof(ShopWindowUI.SelectTab))
                        UnityEventTools.RemovePersistentListener(b.onClick, k);
                UnityEventTools.AddIntPersistentListener(b.onClick, new UnityAction<int>(ui.SelectTab), i);
                RecordInstance(b);
            }
        }

        // ── 스테이지 클리어 창 [상점] ───────────
        static void AddStageClearShopButton(ShopWindowUI shop, Button buttonPrefab)
        {
            var clear = Find<StageClearUI>();
            var panel = clear != null ? clear.transform.Find("Panel") : null;
            if (panel == null || panel.Find("ShopButton") != null) return;
            var next = panel.Find("NextStageButton");
            if (next != null)
            {
                var nrt = (RectTransform)next;
                if (nrt.anchoredPosition == new Vector2(0, 12))
                {
                    nrt.anchoredPosition = new Vector2(52, 12);
                    RecordInstance(nrt);
                }
            }
            var button = NewButton(buttonPrefab, panel, "ShopButton", "상점");
            SetAnchored((RectTransform)button.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-56, 12), new Vector2(72, 24));
            RecordInstance(button.transform);
            UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(shop.Open));
            RecordInstance(button);
        }

        static void RecordInstance(Object o)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(o)) PrefabUtility.RecordPrefabInstancePropertyModifications(o);
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
