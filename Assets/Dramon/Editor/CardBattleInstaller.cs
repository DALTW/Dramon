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
    /// 카드 결투 (1단계: 기본 규칙·기본 덱·드래곤 할퀴기·연타·방어·손패·에너지·턴 종료·체력 창)를 지금 열린 Forest Scene에 추가한다.
    /// - 데이터: Settings/Battle/BattleRules, Settings/Cards/ 카드 13종 + StarterDeck, Settings/Dragons/Actions/ 드래곤 행동
    /// - DragonSpecies 10종: 행동 목록(비어 있을 때만), 최대 체력(지금 100일 때만 새 값으로)
    /// - Prefabs/UI/Battle/CardView
    /// - Managers/Battle (진행·덱·수치), DuelScreen/Panel/Battle (손패·에너지·더미·턴 종료), 플레이어·드래곤 체력 창 숫자
    /// 이미 있는 에셋·오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결·값은 덮어쓰지 않는다.
    /// </summary>
    public static class CardBattleInstaller
    {
        const string BattleSettingsFolder = DramonAssetBuilder.SettingsFolder + "/Battle";
        const string CardFolder = DramonAssetBuilder.SettingsFolder + "/Cards";
        const string ActionFolder = DramonAssetBuilder.SettingsFolder + "/Dragons/Actions";
        const string DragonFolder = DramonAssetBuilder.SettingsFolder + "/Dragons";
        const string RulesPath = BattleSettingsFolder + "/BattleRules.asset";
        const string StarterDeckPath = CardFolder + "/StarterDeck.asset";
        const string PrefabFolder = DramonAssetBuilder.PrefabFolder + "/UI/Battle";
        const string CardPrefabPath = PrefabFolder + "/CardView.prefab";
        const string CardArtPath = "Battle/Cards";

        static readonly Color Brown = new Color32(0x3B, 0x24, 0x14, 0xFF);

        // ── 카드 13종 ────────────────────────────
        struct CardSpec
        {
            public string key, name, description; public CardType type; public int cost; public CardTag tags;
            public CardEffect[] effects; public bool exhaust; public HeroCardAction hero; public int heroRepeat; public int price;
        }

        static CardEffect E(CardEffectType t, int v, int c = 1) => new CardEffect(t, v, c);

        static readonly CardSpec[] Cards =
        {
            // 기본 덱
            new CardSpec { key = "SwordSlash", name = "검 베기", type = CardType.Attack, cost = 1, tags = CardTag.Sword,
                effects = new[] { E(CardEffectType.Damage, 6) }, description = "피해 {0}", hero = HeroCardAction.Slash },
            new CardSpec { key = "ShieldBlock", name = "방패 막기", type = CardType.Defense, cost = 1,
                effects = new[] { E(CardEffectType.Block, 5) }, description = "방어 {0}", hero = HeroCardAction.Block },
            new CardSpec { key = "ThrownLemon", name = "레몬 던지기", type = CardType.Special, cost = 1, tags = CardTag.Lemon,
                effects = new[] { E(CardEffectType.Damage, 4), E(CardEffectType.Sour, 2) }, description = "피해 {0}\n산미 {1}", hero = HeroCardAction.LemonThrow },
            new CardSpec { key = "DodgeRoll", name = "구르기", type = CardType.Defense, cost = 1,
                effects = new[] { E(CardEffectType.Block, 3), E(CardEffectType.Draw, 1) }, description = "방어 {0}\n카드 {1}장 뽑기", hero = HeroCardAction.Roll },
            // 상점 카드
            new CardSpec { key = "LemonJuice", name = "레몬즙 뿌리기", type = CardType.Special, cost = 1, tags = CardTag.Lemon, price = 60,
                effects = new[] { E(CardEffectType.Damage, 3), E(CardEffectType.Sour, 3) }, description = "피해 {0}\n산미 {1}", hero = HeroCardAction.LemonSqueeze },
            new CardSpec { key = "DoubleSlash", name = "연속 베기", type = CardType.Attack, cost = 1, tags = CardTag.Sword, price = 80,
                effects = new[] { E(CardEffectType.MultiDamage, 4, 2) }, description = "피해 {0} × {0x}", hero = HeroCardAction.Slash, heroRepeat = 2 },
            new CardSpec { key = "IronShield", name = "강철 방패", type = CardType.Defense, cost = 2, price = 80,
                effects = new[] { E(CardEffectType.Block, 12) }, description = "방어 {0}", hero = HeroCardAction.Block },
            new CardSpec { key = "QuickRoll", name = "질풍 구르기", type = CardType.Defense, cost = 0, price = 70,
                effects = new[] { E(CardEffectType.Block, 4), E(CardEffectType.Draw, 1) }, description = "방어 {0}\n카드 {1}장 뽑기", hero = HeroCardAction.Roll },
            new CardSpec { key = "HealingHerb", name = "회복 약초", type = CardType.Special, cost = 1, price = 70, exhaust = true,
                effects = new[] { E(CardEffectType.Heal, 8) }, description = "체력 {0} 회복\n소멸", hero = HeroCardAction.Herb },
            new CardSpec { key = "CounterStance", name = "반격 자세", type = CardType.Defense, cost = 1, price = 90,
                effects = new[] { E(CardEffectType.Block, 4), E(CardEffectType.Reflect, 4) }, description = "방어 {0}\n이번 드래곤 턴에\n공격받으면 {1} 반사", hero = HeroCardAction.Block },
            new CardSpec { key = "LemonBurst", name = "레몬 폭발", type = CardType.Special, cost = 2, tags = CardTag.Lemon, price = 120,
                effects = new[] { E(CardEffectType.SourScaledDamage, 3) }, description = "드래곤의 산미 × {0} 피해\n(산미 유지)", hero = HeroCardAction.LemonThrow },
            new CardSpec { key = "ZestAura", name = "상큼한 기운", type = CardType.Power, cost = 1, tags = CardTag.Lemon, price = 140, exhaust = true,
                effects = new[] { E(CardEffectType.SourEachTurn, 1) }, description = "내 턴 시작마다\n드래곤에게 산미 {0}\n소멸", hero = HeroCardAction.DrawCard },
            new CardSpec { key = "DragonSlayer", name = "드래곤 슬레이어", type = CardType.Attack, cost = 3, tags = CardTag.Sword, price = 160,
                effects = new[] { E(CardEffectType.Damage, 20), E(CardEffectType.BonusDamageIfSour, 10) }, description = "피해 {0}\n산미가 있으면 +{1}", hero = HeroCardAction.Slash },
        };

        static readonly (string key, int count)[] StarterCards = { ("SwordSlash", 4), ("ShieldBlock", 4), ("ThrownLemon", 1), ("DodgeRoll", 1) };

        // ── 드래곤 10종 행동 (1번째 결투 기준) ─────
        struct ActionSpec
        {
            public string name; public DragonActionType type; public int value, hits; public StatusType status; public int statusAmount; public float ignore;
            public ActionSpec(string name, DragonActionType type, int value, int hits = 1, StatusType status = StatusType.None, int statusAmount = 0, float ignore = 0f)
            { this.name = name; this.type = type; this.value = value; this.hits = hits; this.status = status; this.statusAmount = statusAmount; this.ignore = ignore; }
        }

        static readonly string[] DragonKeys = { "Ember", "Forest", "Storm", "Shadow", "Gold", "Frost", "Toxic", "Tide", "Stone", "Crystal" };
        static readonly int[] DragonHealths = { 40, 44, 28, 38, 52, 38, 40, 42, 52, 46 };
        static readonly ActionSpec[][] DragonActions =
        {
            new[] { new ActionSpec("할퀴기", DragonActionType.Claw, 10), new ActionSpec("할퀴기", DragonActionType.Claw, 10), new ActionSpec("화염 숨결", DragonActionType.Breath, 17, 1, StatusType.Burn, 2) },
            new[] { new ActionSpec("할퀴기", DragonActionType.Claw, 12), new ActionSpec("회복", DragonActionType.Heal, 6), new ActionSpec("포자 숨결", DragonActionType.Breath, 14, 1, StatusType.Poison, 3) },
            new[] { new ActionSpec("연타", DragonActionType.MultiHit, 6, 2), new ActionSpec("번개 숨결", DragonActionType.Breath, 15, 1, StatusType.Shock, 1), new ActionSpec("할퀴기", DragonActionType.Claw, 10) },
            new[] { new ActionSpec("할퀴기", DragonActionType.Claw, 15), new ActionSpec("그림자 숨결", DragonActionType.Breath, 17, 1, StatusType.Fear, 2), new ActionSpec("방어", DragonActionType.Defend, 6) },
            new[] { new ActionSpec("방어", DragonActionType.Defend, 8), new ActionSpec("할퀴기", DragonActionType.Claw, 14), new ActionSpec("황금 숨결", DragonActionType.Breath, 22, 1, StatusType.Burn, 2) },
            new[] { new ActionSpec("할퀴기", DragonActionType.Claw, 12), new ActionSpec("서리 숨결", DragonActionType.Breath, 17, 1, StatusType.Frozen, 1), new ActionSpec("할퀴기", DragonActionType.Claw, 11) },
            new[] { new ActionSpec("꼬리치기", DragonActionType.Claw, 10), new ActionSpec("독 숨결", DragonActionType.Breath, 12, 1, StatusType.Poison, 4), new ActionSpec("할퀴기", DragonActionType.Claw, 10) },
            new[] { new ActionSpec("할퀴기", DragonActionType.Claw, 13), new ActionSpec("물결 숨결", DragonActionType.Breath, 19, 1, StatusType.Drenched, 1), new ActionSpec("방어", DragonActionType.Defend, 5) },
            new[] { new ActionSpec("방어", DragonActionType.Defend, 8), new ActionSpec("할퀴기", DragonActionType.Claw, 13), new ActionSpec("바위 숨결", DragonActionType.Breath, 20, 1, StatusType.None, 0, 0.5f) },
            new[] { new ActionSpec("할퀴기", DragonActionType.Claw, 12), new ActionSpec("수정 숨결", DragonActionType.Breath, 16, 1, StatusType.Bleed, 3), new ActionSpec("강화", DragonActionType.Buff, 3) },
        };

        [MenuItem("Dramon/기능 설치 - 카드 결투 (현재 Scene)")]
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
                Debug.Log("[Dramon] 카드 결투 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            var managers = GameObject.Find("Managers");
            var phase = Object.FindAnyObjectByType<GamePhaseController>(FindObjectsInactive.Include);
            var session = Object.FindAnyObjectByType<DuelSession>(FindObjectsInactive.Include);
            var screen = Object.FindAnyObjectByType<DuelScreenUI>(FindObjectsInactive.Include);
            var dragonHealth = Object.FindAnyObjectByType<DragonHealth>(FindObjectsInactive.Include);
            var panel = screen != null ? screen.transform.Find("Panel") : null;
            if (managers == null || phase == null || session == null || panel == null || dragonHealth == null
                || panel.Find("PlayerHealthWindow") == null || panel.Find("MessageWindow") == null)
            {
                EditorUtility.DisplayDialog("Dramon",
                    "Forest Scene이 열려 있어야 하고, 먼저 '기능 설치 - 결투 검사·포켓몬 구도'가 설치돼 있어야 해요.", "확인");
                return false;
            }

            // ── 데이터 ───────────────────────────
            DramonAssetBuilder.EnsureFolder(BattleSettingsFolder);
            DramonAssetBuilder.EnsureFolder(CardFolder);
            DramonAssetBuilder.EnsureFolder(ActionFolder);
            DramonAssetBuilder.EnsureFolder(PrefabFolder);
            var rules = EnsureAsset<BattleRules>(RulesPath);
            var cards = EnsureCards();
            var starter = EnsureStarterDeck(cards);
            UpdateDragons();
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();
            var cardPrefab = EnsureCardPrefab();

            // ── Managers/Battle ──────────────────
            var battle = FindOrCreateChild(managers, "Battle");
            var controller = GetOrAdd<CardBattleController>(FindOrCreateChild(battle, "CardBattleController"));
            var playerTurn = GetOrAdd<PlayerTurn>(FindOrCreateChild(battle, "PlayerTurn"));
            var dragonTurn = GetOrAdd<DragonTurn>(FindOrCreateChild(battle, "DragonTurn"));
            var deckGO = FindOrCreateChild(battle, "Deck");
            var playerDeck = GetOrAdd<PlayerDeck>(FindOrCreateChild(deckGO, "PlayerDeck"));
            var drawPile = GetOrAdd<DrawPile>(FindOrCreateChild(deckGO, "DrawPile"));
            var discardPile = GetOrAdd<DiscardPile>(FindOrCreateChild(deckGO, "DiscardPile"));
            var exhaustPile = GetOrAdd<ExhaustPile>(FindOrCreateChild(deckGO, "ExhaustPile"));
            var hand = GetOrAdd<Hand>(FindOrCreateChild(deckGO, "Hand"));
            var statsGO = FindOrCreateChild(battle, "Stats");
            var playerStats = GetOrAdd<PlayerBattleStats>(FindOrCreateChild(statsGO, "PlayerBattleStats"));
            var dragonStats = GetOrAdd<DragonBattleStats>(FindOrCreateChild(statsGO, "DragonBattleStats"));
            var resolver = GetOrAdd<CardEffectResolver>(FindOrCreateChild(battle, "CardEffectResolver"));
            var dragonGO = FindOrCreateChild(battle, "Dragon");
            var intent = GetOrAdd<DragonIntentPicker>(FindOrCreateChild(dragonGO, "DragonIntentPicker"));
            var executor = GetOrAdd<DragonActionExecutor>(FindOrCreateChild(dragonGO, "DragonActionExecutor"));

            // ── 결투 화면 UI ─────────────────────
            var log = screen.GetComponent<BattleLogMessage>();
            bool firstUI = log == null;
            if (log == null)
            {
                log = screen.gameObject.AddComponent<BattleLogMessage>();
                var message = panel.Find("MessageWindow");
                SetRef(log, "window", message.gameObject);
                SetRef(log, "label", message.GetComponentInChildren<Text>(true));
                // 손패가 아래 가운데를 쓰므로 메시지 창을 위 가운데로 (Scene에서 다시 옮겨도 됨)
                SetAnchored((RectTransform)message, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(24, -10), new Vector2(300, 32));
                var text = message.GetComponentInChildren<Text>(true);
                if (text != null) text.rectTransform.sizeDelta = new Vector2(284, 22);
            }

            var battleRootT = panel.Find("Battle");
            if (battleRootT == null) battleRootT = BuildBattleUI(panel, controller, playerTurn, hand, playerStats, drawPile, discardPile, cardPrefab, buttonPrefab);
            var playerHealthUI = BuildPlayerHealth(panel.Find("PlayerHealthWindow"), playerStats);
            BuildDragonHealthExtras(panel.Find("HealthWindow"), dragonHealth, dragonStats);

            // ── 연결 (비어 있는 것만) ─────────────
            SetRefIfEmpty(playerDeck, "starterDeck", starter);
            SetRefIfEmpty(playerDeck, "phaseController", phase);
            SetRefIfEmpty(hand, "rules", rules);
            SetRefIfEmpty(dragonStats, "health", dragonHealth);

            SetRefIfEmpty(resolver, "player", playerStats);
            SetRefIfEmpty(resolver, "dragon", dragonStats);
            SetRefIfEmpty(resolver, "playerTurn", playerTurn);

            SetRefIfEmpty(playerTurn, "stats", playerStats);
            SetRefIfEmpty(playerTurn, "drawPile", drawPile);
            SetRefIfEmpty(playerTurn, "discardPile", discardPile);
            SetRefIfEmpty(playerTurn, "exhaustPile", exhaustPile);
            SetRefIfEmpty(playerTurn, "hand", hand);
            SetRefIfEmpty(playerTurn, "resolver", resolver);
            SetRefIfEmpty(playerTurn, "log", log);

            SetRefIfEmpty(executor, "player", playerStats);
            SetRefIfEmpty(executor, "dragon", dragonStats);
            SetRefIfEmpty(executor, "log", log);
            SetRefIfEmpty(dragonTurn, "dragon", dragonStats);
            SetRefIfEmpty(dragonTurn, "intent", intent);
            SetRefIfEmpty(dragonTurn, "executor", executor);

            SetRefIfEmpty(controller, "rules", rules);
            SetRefIfEmpty(controller, "session", session);
            SetRefIfEmpty(controller, "phaseController", phase);
            SetRefIfEmpty(controller, "playerTurn", playerTurn);
            SetRefIfEmpty(controller, "dragonTurn", dragonTurn);
            SetRefIfEmpty(controller, "intent", intent);
            SetRefIfEmpty(controller, "dragonActions", executor);
            SetRefIfEmpty(controller, "playerDeck", playerDeck);
            SetRefIfEmpty(controller, "drawPile", drawPile);
            SetRefIfEmpty(controller, "discardPile", discardPile);
            SetRefIfEmpty(controller, "exhaustPile", exhaustPile);
            SetRefIfEmpty(controller, "hand", hand);
            SetRefIfEmpty(controller, "playerStats", playerStats);
            SetRefIfEmpty(controller, "dragonStats", dragonStats);
            SetRefIfEmpty(controller, "battleRoot", battleRootT.gameObject);
            SetRefIfEmpty(controller, "playerHealthUI", playerHealthUI);
            SetRefIfEmpty(controller, "log", log);
            SetRefIfEmpty(controller, "duelScreen", screen);
            if (cards.TryGetValue("DragonSlayer", out var testCard)) SetRefIfEmpty(controller, "testCard", testCard);

            SetRefIfEmpty(session, "cardBattle", controller);

            if (firstUI) Debug.Log("[Dramon] 메시지 창을 위 가운데로 옮겼어요 (손패 자리). Scene에서 다시 옮겨도 그대로 유지돼요.");
            AssetDatabase.SaveAssets();
            return true;
        }

        // ── 데이터 ───────────────────────────────
        static T EnsureAsset<T>(string path) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a != null) return a;
            a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        static Dictionary<string, CardData> EnsureCards()
        {
            var dict = new Dictionary<string, CardData>();
            foreach (var spec in Cards)
            {
                string path = $"{CardFolder}/Card_{spec.key}.asset";
                var card = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(card, path);
                    var so = new SerializedObject(card);
                    so.FindProperty("displayName").stringValue = spec.name;
                    so.FindProperty("type").enumValueIndex = (int)spec.type;
                    so.FindProperty("cost").intValue = spec.cost;
                    so.FindProperty("composedImage").objectReferenceValue = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Composed/Card_{spec.key}");
                    so.FindProperty("art").objectReferenceValue = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Art/CardArt_{spec.key}");
                    so.FindProperty("description").stringValue = spec.description;
                    so.FindProperty("tags").intValue = (int)spec.tags;
                    var effects = so.FindProperty("effects");
                    effects.arraySize = spec.effects.Length;
                    for (int i = 0; i < spec.effects.Length; i++)
                    {
                        var e = effects.GetArrayElementAtIndex(i);
                        e.FindPropertyRelative("type").enumValueIndex = (int)spec.effects[i].type;
                        e.FindPropertyRelative("value").intValue = spec.effects[i].value;
                        e.FindPropertyRelative("count").intValue = spec.effects[i].count;
                    }
                    so.FindProperty("exhaust").boolValue = spec.exhaust;
                    so.FindProperty("heroAction").enumValueIndex = (int)spec.hero;
                    so.FindProperty("heroActionRepeat").intValue = Mathf.Max(1, spec.heroRepeat);
                    so.FindProperty("shopPrice").intValue = spec.price;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(card);
                }
                dict[spec.key] = card;
            }
            return dict;
        }

        static StarterDeck EnsureStarterDeck(Dictionary<string, CardData> cards)
        {
            var deck = EnsureAsset<StarterDeck>(StarterDeckPath);
            var so = new SerializedObject(deck);
            var entries = so.FindProperty("entries");
            if (entries.arraySize > 0) return deck;
            entries.arraySize = StarterCards.Length;
            for (int i = 0; i < StarterCards.Length; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("card").objectReferenceValue = cards[StarterCards[i].key];
                e.FindPropertyRelative("count").intValue = StarterCards[i].count;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(deck);
            return deck;
        }

        static void UpdateDragons()
        {
            for (int i = 0; i < DragonKeys.Length; i++)
            {
                string path = $"{DragonFolder}/Dragon{i + 1:00}_{DragonKeys[i]}.asset";
                var species = AssetDatabase.LoadAssetAtPath<DragonSpecies>(path);
                if (species == null) { Debug.LogWarning($"[Dramon] 드래곤 데이터를 찾을 수 없어요: {path}"); continue; }
                var so = new SerializedObject(species);
                var hp = so.FindProperty("maxHealth");
                if (hp.intValue == 100) hp.intValue = DragonHealths[i]; // 처음 값(100)일 때만
                var list = so.FindProperty("battleActions");
                if (list.arraySize == 0)
                {
                    var specs = DragonActions[i];
                    list.arraySize = specs.Length;
                    for (int k = 0; k < specs.Length; k++)
                        list.GetArrayElementAtIndex(k).objectReferenceValue = EnsureAction($"Act{i + 1:00}_{DragonKeys[i]}_{k + 1}", specs[k]);
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(species);
            }
        }

        static DragonActionData EnsureAction(string file, ActionSpec spec)
        {
            string path = $"{ActionFolder}/{file}.asset";
            var a = AssetDatabase.LoadAssetAtPath<DragonActionData>(path);
            if (a != null) return a;
            a = ScriptableObject.CreateInstance<DragonActionData>();
            AssetDatabase.CreateAsset(a, path);
            var so = new SerializedObject(a);
            so.FindProperty("displayName").stringValue = spec.name;
            so.FindProperty("type").enumValueIndex = (int)spec.type;
            so.FindProperty("value").intValue = spec.value;
            so.FindProperty("hits").intValue = spec.hits;
            so.FindProperty("status").enumValueIndex = (int)spec.status;
            so.FindProperty("statusAmount").intValue = spec.statusAmount;
            so.FindProperty("blockIgnoreRatio").floatValue = spec.ignore;
            so.FindProperty("playAttackMotion").boolValue = spec.type != DragonActionType.Defend && spec.type != DragonActionType.Heal && spec.type != DragonActionType.Buff;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(a);
            return a;
        }

        // ── 카드 Prefab (원본 크기 260×408, 글자 위치 = CardFrame_Layout.json) ──
        static CardView EnsureCardPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CardView>(CardPrefabPath);
            if (existing != null) return existing;

            var root = new GameObject("CardView", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            root.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)root.transform;
            rt.sizeDelta = new Vector2(260, 408);
            rt.pivot = new Vector2(0.5f, 0f);
            var frame = root.GetComponent<Image>();
            frame.sprite = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Frames/CardFrame_Attack");
            frame.raycastTarget = true;

            var topLeft = new Vector2(0, 1);
            var artGO = NewUI("Art", root.transform);
            SetAnchored((RectTransform)artGO.transform, topLeft, topLeft, new Vector2(48, -47), new Vector2(164, 148));
            var art = artGO.AddComponent<Image>();
            art.raycastTarget = false;
            art.enabled = false;

            var cost = NewText("Cost", root.transform, topLeft, new Vector2(0.5f, 0.5f), new Vector2(39, -36), new Vector2(40, 40),
                TextAnchor.MiddleCenter, 30, "1");
            cost.color = Brown;
            var nameLabel = NewText("Name", root.transform, topLeft, new Vector2(0.5f, 0.5f), new Vector2(130, -238), new Vector2(170, 26),
                TextAnchor.MiddleCenter, 20, "카드 이름");
            nameLabel.color = Color.white;
            var outline = nameLabel.gameObject.AddComponent<Outline>();
            outline.effectColor = Brown;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            var desc = NewText("Description", root.transform, topLeft, topLeft, new Vector2(40, -268), new Vector2(180, 114),
                TextAnchor.MiddleCenter, 18, "피해 6");
            desc.color = Brown;
            desc.fontStyle = FontStyle.Bold;
            desc.supportRichText = true;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Overflow;
            desc.lineSpacing = 0.9f;

            var view = root.AddComponent<CardView>();
            SetRef(view, "frame", frame);
            SetRef(view, "art", art);
            SetRef(view, "costLabel", cost);
            SetRef(view, "nameLabel", nameLabel);
            SetRef(view, "descriptionLabel", desc);
            SetRef(view, "canvasGroup", root.GetComponent<CanvasGroup>());
            SetRef(view, "attackFrame", DramonAssetBuilder.LoadSprite($"{CardArtPath}/Frames/CardFrame_Attack"));
            SetRef(view, "defenseFrame", DramonAssetBuilder.LoadSprite($"{CardArtPath}/Frames/CardFrame_Defense"));
            SetRef(view, "specialFrame", DramonAssetBuilder.LoadSprite($"{CardArtPath}/Frames/CardFrame_Special"));
            SetRef(view, "powerFrame", DramonAssetBuilder.LoadSprite($"{CardArtPath}/Frames/CardFrame_Power"));

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<CardView>();
        }

        // ── DuelScreen/Panel/Battle ─────────────────
        static Transform BuildBattleUI(Transform panel, CardBattleController controller, PlayerTurn playerTurn, Hand hand, PlayerBattleStats stats,
            DrawPile drawPile, DiscardPile discardPile, CardView cardPrefab, Button buttonPrefab)
        {
            var root = NewUI("Battle", panel);
            Stretch((RectTransform)root.transform);
            var loot = panel.Find("LootWindow");
            if (loot != null) root.transform.SetSiblingIndex(loot.GetSiblingIndex()); // 전리품 창보다 아래
            var bottom = new Vector2(0.5f, 0f);

            // 손패
            var handGO = NewUI("Hand", root.transform);
            SetAnchored((RectTransform)handGO.transform, bottom, bottom, new Vector2(0, 0), new Vector2(380, 180));
            var handView = handGO.AddComponent<HandView>();
            SetRef(handView, "hand", hand);
            SetRef(handView, "playerTurn", playerTurn);
            SetRef(handView, "playerStats", stats);
            SetRef(handView, "cardPrefab", cardPrefab);
            SetRef(handView, "container", handGO.transform);

            // 에너지 (손패 왼쪽)
            var energyGO = NewUI("Energy", root.transform);
            SetAnchored((RectTransform)energyGO.transform, bottom, bottom, new Vector2(-218, 26), new Vector2(44, 44));
            var energyIcon = energyGO.AddComponent<Image>();
            energyIcon.sprite = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Icons/Icon_LemonEnergy");
            energyIcon.preserveAspect = true;
            energyIcon.raycastTarget = false;
            var energyText = NewText("EnergyText", energyGO.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 20),
                TextAnchor.MiddleCenter, 14, "3/3");
            var energyOutline = energyText.gameObject.AddComponent<Outline>();
            energyOutline.effectColor = Color.white;
            var energy = energyGO.AddComponent<EnergyUI>();
            SetRef(energy, "stats", stats);
            SetRef(energy, "label", energyText);

            // 뽑을 더미 (왼쪽 아래) · 버린 더미 (오른쪽 아래)
            BuildPile(root.transform, "DrawPile", new Vector2(0, 0), new Vector2(10, 8), drawPile, "뽑을 더미");
            BuildPile(root.transform, "DiscardPile", new Vector2(1, 0), new Vector2(-10, 8), discardPile, "버린 더미");

            // 턴 종료 (오른쪽, 손패 위 · 체력 창 위)
            var endTurn = NewButton(buttonPrefab, root.transform, "EndTurnButton", "턴 종료");
            SetAnchored((RectTransform)endTurn.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-16, 124), new Vector2(76, 26));
            PrefabUtility.RecordPrefabInstancePropertyModifications(endTurn.transform);
            UnityEventTools.AddPersistentListener(endTurn.onClick, new UnityAction(controller.RequestEndTurn));
            PrefabUtility.RecordPrefabInstancePropertyModifications(endTurn);
            var endTurnUI = endTurn.gameObject.AddComponent<EndTurnButtonUI>();
            SetRef(endTurnUI, "controller", controller);
            SetRef(endTurnUI, "button", endTurn);

            root.SetActive(false);
            return root.transform;
        }

        static void BuildPile(Transform parent, string name, Vector2 anchor, Vector2 pos, CardPile pile, string caption)
        {
            var go = NewUI(name, parent);
            SetAnchored((RectTransform)go.transform, anchor, anchor, pos, new Vector2(30, 47));
            var img = go.AddComponent<Image>();
            img.sprite = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Composed/Card_Back");
            img.preserveAspect = true;
            img.raycastTarget = false;
            var count = NewText("Count", go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 20),
                TextAnchor.MiddleCenter, 14, "0");
            count.color = Color.white;
            var o = count.gameObject.AddComponent<Outline>();
            o.effectColor = Brown;
            var ui = go.AddComponent<PileCountUI>();
            SetRef(ui, "pile", pile);
            SetRef(ui, "label", count);
        }

        // ── 플레이어 체력 창 (막대 + 숫자 + 방어) ─────
        static PlayerHealthUI BuildPlayerHealth(Transform window, PlayerBattleStats stats)
        {
            var ui = window.GetComponent<PlayerHealthUI>();
            if (ui != null) return ui;

            var top = new Vector2(0.5f, 1f);
            var title = NewText("Title", window, new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -5), new Vector2(60, 14), TextAnchor.MiddleLeft, 10, "검사");
            var healthText = NewText("HealthText", window, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -5), new Vector2(80, 14), TextAnchor.MiddleRight, 11, "50 / 50");

            var barBack = NewUI("BarBack", window);
            SetAnchored((RectTransform)barBack.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(140, 10));
            var backImg = barBack.AddComponent<Image>();
            backImg.color = Brown;
            backImg.raycastTarget = false;
            var area = NewUI("BarArea", barBack.transform);
            var art = (RectTransform)area.transform;
            art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one; art.offsetMin = new Vector2(1, 1); art.offsetMax = new Vector2(-1, -1);
            var fillGO = NewUI("BarFill", area.transform);
            Stretch((RectTransform)fillGO.transform);
            var fill = fillGO.AddComponent<Image>();
            fill.color = new Color32(0x58, 0xC8, 0x48, 0xFF);
            fill.raycastTarget = false;

            // 방어 (창 왼쪽에 걸친 방패 + 숫자)
            var block = NewUI("Block", window);
            SetAnchored((RectTransform)block.transform, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-4, 0), new Vector2(28, 28));
            var blockImg = block.AddComponent<Image>();
            blockImg.sprite = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Icons/Icon_Block");
            blockImg.preserveAspect = true;
            blockImg.raycastTarget = false;
            var blockText = NewText("BlockText", block.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28, 18),
                TextAnchor.MiddleCenter, 12, "0");
            blockText.color = Color.white;
            var bo = blockText.gameObject.AddComponent<Outline>();
            bo.effectColor = Brown;
            block.SetActive(false);

            ui = window.gameObject.AddComponent<PlayerHealthUI>();
            SetRef(ui, "stats", stats);
            SetRef(ui, "window", window.gameObject);
            SetRef(ui, "fill", fill);
            SetRef(ui, "healthLabel", healthText);
            SetRef(ui, "blockRoot", block);
            SetRef(ui, "blockLabel", blockText);
            return ui;
        }

        // ── 드래곤 체력 창: 숫자 + 방어 ─────────────
        static void BuildDragonHealthExtras(Transform window, DragonHealth health, DragonBattleStats stats)
        {
            if (window == null) return;
            if (window.GetComponent<DragonHealthTextUI>() == null)
            {
                var text = NewText("HealthText", window, new Vector2(0, 0), new Vector2(0, 1), new Vector2(6, -1), new Vector2(100, 14), TextAnchor.MiddleLeft, 11, "40 / 40");
                text.color = Color.white;
                var o = text.gameObject.AddComponent<Outline>();
                o.effectColor = Brown;
                var ui = window.gameObject.AddComponent<DragonHealthTextUI>();
                SetRef(ui, "health", health);
                SetRef(ui, "label", text);
            }
            if (window.GetComponent<DragonBlockUI>() == null)
            {
                var block = NewUI("Block", window);
                SetAnchored((RectTransform)block.transform, new Vector2(1, 0.5f), new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(26, 26));
                var img = block.AddComponent<Image>();
                img.sprite = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Icons/Icon_Block");
                img.preserveAspect = true;
                img.raycastTarget = false;
                var t = NewText("BlockText", block.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26, 18),
                    TextAnchor.MiddleCenter, 12, "0");
                t.color = Color.white;
                var o = t.gameObject.AddComponent<Outline>();
                o.effectColor = Brown;
                block.SetActive(false);
                var ui = window.gameObject.AddComponent<DragonBlockUI>();
                SetRef(ui, "stats", stats);
                SetRef(ui, "root", block);
                SetRef(ui, "label", t);
            }
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
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
