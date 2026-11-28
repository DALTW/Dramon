using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Dramon.EditorTools.DramonEditorUtil;

namespace Dramon.EditorTools
{
    /// <summary>
    /// v3 1단계: 스테이지·보스·모드 데이터 + 카드 결투 2단계(상태 효과·산미·의도 아이콘·숨결/회복/강화·모션·떠오르는 숫자·카드 강화 수치)를
    /// 지금 열린 Forest Scene에 추가한다. 카드 결투 1단계가 없으면 먼저 설치한다.
    /// - 데이터: Settings/Stage/StageRules·BossRules, Settings/Modes/Mode_Normal·Mode_Endless,
    ///   Settings/Cards/Card_LemonBomb·Card_SmokeRoll (임시 그림), 카드 15종 강화 수치, 드래곤 보스 힌트
    /// - Prefabs/UI/Battle/FloatingNumber, StatusIconView
    /// - Managers/Battle 아래: CardStatCalculator, BattleMotionLink, StatusEffectProcessor, Stats/PlayerStatus·DragonStatus,
    ///   Deck/CardUpgradeState, Dragon/DragonScaling
    /// - DuelScreen/Panel: Battle/FloatingNumbers, Hero/FloatAnchor, DragonSide/Dragon/FloatAnchor,
    ///   HealthWindow/Intent·DragonStatus, PlayerHealthWindow/PlayerStatus
    /// 이미 있는 에셋·오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결·값은 덮어쓰지 않는다.
    /// </summary>
    public static class BattleV3Installer
    {
        const string StageFolder = DramonAssetBuilder.SettingsFolder + "/Stage";
        const string ModeFolder = DramonAssetBuilder.SettingsFolder + "/Modes";
        const string CardFolder = DramonAssetBuilder.SettingsFolder + "/Cards";
        const string DragonFolder = DramonAssetBuilder.SettingsFolder + "/Dragons";
        const string StageRulesPath = StageFolder + "/StageRules.asset";
        const string BossRulesPath = StageFolder + "/BossRules.asset";
        const string NormalModePath = ModeFolder + "/Mode_Normal.asset";
        const string EndlessModePath = ModeFolder + "/Mode_Endless.asset";
        const string BattleRulesPath = DramonAssetBuilder.SettingsFolder + "/Battle/BattleRules.asset";
        const string PrefabFolder = DramonAssetBuilder.PrefabFolder + "/UI/Battle";
        const string FloatingPrefabPath = PrefabFolder + "/FloatingNumber.prefab";
        const string StatusIconPrefabPath = PrefabFolder + "/StatusIconView.prefab";
        const string IconPath = "Battle/Cards/Icons";
        const string CardArtPath = "Battle/Cards";
        const string DefaultBossHint = "숲 깊은 곳에서 낯선 기척이 느껴진다…";

        static readonly Vector2 IntentPosition = new Vector2(0, -18);
        static readonly Color Brown = new Color32(0x3B, 0x24, 0x14, 0xFF);

        static CardEffect E(CardEffectType t, int v, int c = 1) => new CardEffect(t, v, c);

        // ── 카드 강화 (강화 후 효과는 효과 목록과 같은 순서, 코스트 -1 = 그대로) ──
        static readonly Dictionary<string, (CardEffect[] effects, int cost, int price)> Upgrades = new Dictionary<string, (CardEffect[], int, int)>
        {
            { "SwordSlash", (new[] { E(CardEffectType.Damage, 9) }, -1, 120) },
            { "ShieldBlock", (new[] { E(CardEffectType.Block, 8) }, -1, 110) },
            { "ThrownLemon", (new[] { E(CardEffectType.Damage, 5), E(CardEffectType.Sour, 3) }, -1, 50) },
            { "DodgeRoll", (new[] { E(CardEffectType.Block, 5), E(CardEffectType.Draw, 1) }, -1, 50) },
            { "LemonJuice", (new[] { E(CardEffectType.Damage, 4), E(CardEffectType.Sour, 5) }, -1, 60) },
            { "DoubleSlash", (new[] { E(CardEffectType.MultiDamage, 5, 2) }, -1, 70) },
            { "IronShield", (new[] { E(CardEffectType.Block, 16) }, -1, 70) },
            { "QuickRoll", (new[] { E(CardEffectType.Block, 6), E(CardEffectType.Draw, 1) }, -1, 60) },
            { "HealingHerb", (new[] { E(CardEffectType.Heal, 12) }, -1, 60) },
            { "CounterStance", (new[] { E(CardEffectType.Block, 6), E(CardEffectType.Reflect, 6) }, -1, 70) },
            { "LemonBurst", (new[] { E(CardEffectType.SourScaledDamage, 4) }, -1, 90) },
            { "ZestAura", (new CardEffect[0], 0, 100) },
            { "DragonSlayer", (new CardEffect[0], 2, 120) },
            { "LemonBomb", (new[] { E(CardEffectType.Damage, 11), E(CardEffectType.Sour, 3) }, -1, 60) },
            { "SmokeRoll", (new CardEffect[0], 0, 70) },
        };

        static readonly string[] DragonKeys = { "Ember", "Forest", "Storm", "Shadow", "Gold", "Frost", "Toxic", "Tide", "Stone", "Crystal" };
        static readonly string[] BossHints =
        {
            "숲 깊은 곳에서 불꽃 냄새가 난다…",
            "숲 깊은 곳에서 축축한 이끼 냄새가 난다…",
            "숲 깊은 곳의 공기가 찌릿찌릿하다…",
            "숲 깊은 곳의 그림자가 길어진다…",
            "숲 깊은 곳에서 금빛이 번뜩인다…",
            "숲 깊은 곳에서 차가운 입김이 불어온다…",
            "숲 깊은 곳에서 매캐한 독 냄새가 난다…",
            "숲 깊은 곳에서 물결 소리가 들린다…",
            "숲 깊은 곳에서 땅이 묵직하게 울린다…",
            "숲 깊은 곳에서 수정이 반짝인다…",
        };

        static readonly StatusType[] PlayerStatusOrder =
            { StatusType.Burn, StatusType.Poison, StatusType.Shock, StatusType.Frozen, StatusType.Drenched, StatusType.Fear, StatusType.Bleed };

        [MenuItem("Dramon/기능 설치 - v3 1단계 (결투 상태 효과·스테이지 데이터, 현재 Scene)")]
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
                Debug.Log("[Dramon] v3 1단계 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            if (Object.FindAnyObjectByType<CardBattleController>(FindObjectsInactive.Include) == null && !CardBattleInstaller.Install()) return false;

            var managers = GameObject.Find("Managers");
            var controller = Object.FindAnyObjectByType<CardBattleController>(FindObjectsInactive.Include);
            var phase = Object.FindAnyObjectByType<GamePhaseController>(FindObjectsInactive.Include);
            var screen = Object.FindAnyObjectByType<DuelScreenUI>(FindObjectsInactive.Include);
            var hero = Object.FindAnyObjectByType<HeroBattlerView>(FindObjectsInactive.Include);
            var dragonView = Object.FindAnyObjectByType<DragonBattlerView>(FindObjectsInactive.Include);
            var upgrades = Object.FindAnyObjectByType<PlayerUpgrades>(FindObjectsInactive.Include);
            var panel = screen != null ? screen.transform.Find("Panel") : null;
            var battleT = managers != null ? managers.transform.Find("Battle") : null;
            if (managers == null || controller == null || panel == null || battleT == null || panel.Find("Battle") == null)
            {
                EditorUtility.DisplayDialog("Dramon", "Forest Scene이 열려 있어야 하고 카드 결투 1단계가 설치돼 있어야 해요.", "확인");
                return false;
            }

            // ── 데이터 ───────────────────────────
            DramonAssetBuilder.EnsureFolder(StageFolder);
            DramonAssetBuilder.EnsureFolder(ModeFolder);
            DramonAssetBuilder.EnsureFolder(PrefabFolder);
            bool firstStageRules = AssetDatabase.LoadAssetAtPath<StageRules>(StageRulesPath) == null;
            var stageRules = EnsureStageRules();
            var bossRules = EnsureAsset<BossRules>(BossRulesPath);
            EnsureMode(NormalModePath, false, stageRules, bossRules);
            EnsureMode(EndlessModePath, true, stageRules, bossRules);
            var battleRules = AssetDatabase.LoadAssetAtPath<BattleRules>(BattleRulesPath);
            if (firstStageRules && battleRules != null)
            {
                // 예전 결투 순서 강화(+6%)는 v3 배율로 바뀌었으므로 처음 한 번만 꺼 둔다 (다시 켜도 덮어쓰지 않음)
                var so = new SerializedObject(battleRules);
                so.FindProperty("duelScalingEnabled").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(battleRules);
            }
            var cards = EnsureNewCards();
            ApplyUpgrades();
            ApplyBossHints();
            var floatingPrefab = EnsureFloatingPrefab();
            var statusIconPrefab = EnsureStatusIconPrefab();

            // ── Managers/Battle ──────────────────
            var battle = battleT.gameObject;
            var stats = FindOrCreateChild(battle, "Stats");
            var playerStatus = GetOrAdd<StatusEffects>(FindOrCreateChild(stats, "PlayerStatus"));
            var dragonStatus = GetOrAdd<StatusEffects>(FindOrCreateChild(stats, "DragonStatus"));
            var calculator = GetOrAdd<CardStatCalculator>(FindOrCreateChild(battle, "CardStatCalculator"));
            var motions = GetOrAdd<BattleMotionLink>(FindOrCreateChild(battle, "BattleMotionLink"));
            var processor = GetOrAdd<StatusEffectProcessor>(FindOrCreateChild(battle, "StatusEffectProcessor"));
            var cardUpgrades = GetOrAdd<CardUpgradeState>(FindOrCreateChild(FindOrCreateChild(battle, "Deck"), "CardUpgradeState"));
            var dragonGO = FindOrCreateChild(battle, "Dragon");
            var scaling = GetOrAdd<DragonScaling>(FindOrCreateChild(dragonGO, "DragonScaling"));

            var playerStats = battle.GetComponentInChildren<PlayerBattleStats>(true);
            var dragonStats = battle.GetComponentInChildren<DragonBattleStats>(true);
            var playerTurn = battle.GetComponentInChildren<PlayerTurn>(true);
            var dragonTurn = battle.GetComponentInChildren<DragonTurn>(true);
            var resolver = battle.GetComponentInChildren<CardEffectResolver>(true);
            var executor = battle.GetComponentInChildren<DragonActionExecutor>(true);
            var intent = battle.GetComponentInChildren<DragonIntentPicker>(true);
            var log = screen.GetComponent<BattleLogMessage>();

            // ── 화면 ─────────────────────────────
            var floating = BuildFloatingNumbers(panel, floatingPrefab, hero, dragonView);
            BuildIntent(panel.Find("HealthWindow"), intent, executor, dragonStats, controller);
            BuildStatusIcons(panel.Find("HealthWindow"), "DragonStatus", dragonStatus, statusIconPrefab, new[] { StatusType.Sour }, true);
            BuildStatusIcons(panel.Find("PlayerHealthWindow"), "PlayerStatus", playerStatus, statusIconPrefab, PlayerStatusOrder, false);
            var handView = panel.GetComponentInChildren<HandView>(true);

            // ── 연결 (비어 있는 것만) ─────────────
            SetRefIfEmpty(cardUpgrades, "phaseController", phase);
            SetRefIfEmpty(calculator, "cardUpgrades", cardUpgrades);
            SetRefIfEmpty(calculator, "playerUpgrades", upgrades);
            SetRefIfEmpty(calculator, "playerStatus", playerStatus);
            SetRefIfEmpty(scaling, "stageRules", stageRules);
            SetRefIfEmpty(scaling, "battleRules", battleRules);
            SetRefIfEmpty(motions, "hero", hero);
            SetRefIfEmpty(motions, "dragon", dragonView);
            SetRefIfEmpty(processor, "player", playerStats);
            SetRefIfEmpty(processor, "playerStatus", playerStatus);
            SetRefIfEmpty(processor, "dragonStatus", dragonStatus);
            SetRefIfEmpty(processor, "floating", floating);

            SetRefIfEmpty(resolver, "calculator", calculator);
            SetRefIfEmpty(resolver, "dragonStatus", dragonStatus);
            SetRefIfEmpty(resolver, "floating", floating);
            SetRefIfEmpty(resolver, "motions", motions);

            SetRefIfEmpty(playerTurn, "calculator", calculator);
            SetRefIfEmpty(playerTurn, "statusProcessor", processor);
            SetRefIfEmpty(playerTurn, "dragonStatus", dragonStatus);
            SetRefIfEmpty(playerTurn, "motions", motions);
            SetRefIfEmpty(playerTurn, "floating", floating);
            SetRefIfEmpty(playerTurn, "log", log);

            SetRefIfEmpty(executor, "playerStatus", playerStatus);
            SetRefIfEmpty(executor, "scaling", scaling);
            SetRefIfEmpty(executor, "motions", motions);
            SetRefIfEmpty(executor, "floating", floating);
            SetRefIfEmpty(dragonTurn, "statusProcessor", processor);

            SetRefIfEmpty(controller, "scaling", scaling);
            SetRefIfEmpty(controller, "stageRules", stageRules);
            SetRefIfEmpty(controller, "statusProcessor", processor);
            SetRefIfEmpty(controller, "playerStatus", playerStatus);
            SetRefIfEmpty(controller, "dragonStatus", dragonStatus);
            SetRefIfEmpty(controller, "playerUpgrades", upgrades);
            SetRefIfEmpty(controller, "hero", hero);

            if (handView != null)
            {
                SetRefIfEmpty(handView, "calculator", calculator);
                SetRefIfEmpty(handView, "playerStatus", playerStatus);
                SetRefIfEmpty(handView, "cardUpgrades", cardUpgrades);
            }

            if (upgrades == null) Debug.LogWarning("[Dramon] PlayerUpgrades를 찾지 못해 체력 증가 강화 연결을 건너뛰었어요.");
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

        static StageRules EnsureStageRules()
        {
            var existing = AssetDatabase.LoadAssetAtPath<StageRules>(StageRulesPath);
            if (existing != null) return existing;
            var rules = EnsureAsset<StageRules>(StageRulesPath);
            // 설계표: 1~3 스테이지는 정사각형 숲 (공식은 5×4·6×5·7×6이 나와서 덮어씀)
            var so = new SerializedObject(rules);
            var list = so.FindProperty("overrides");
            var sizes = new[] { (1, 5, 5), (2, 6, 6), (3, 7, 7) };
            list.arraySize = sizes.Length;
            for (int i = 0; i < sizes.Length; i++)
            {
                var o = list.GetArrayElementAtIndex(i);
                o.FindPropertyRelative("stage").intValue = sizes[i].Item1;
                o.FindPropertyRelative("width").intValue = sizes[i].Item2;
                o.FindPropertyRelative("height").intValue = sizes[i].Item3;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(rules);
            return rules;
        }

        static void EnsureMode(string path, bool endless, StageRules stageRules, BossRules bossRules)
        {
            if (AssetDatabase.LoadAssetAtPath<GameModeData>(path) != null) return;
            var mode = EnsureAsset<GameModeData>(path);
            var so = new SerializedObject(mode);
            so.FindProperty("displayName").stringValue = endless ? "무한 모드" : "일반 모드";
            so.FindProperty("description").stringValue = endless
                ? "10종 드래곤이 무작위로 나와요. 어디까지 갈 수 있을까요?"
                : "5 스테이지를 지나 숲의 주인을 쓰러뜨려요";
            so.FindProperty("lockedMessage").stringValue = endless ? "일반 모드를 클리어하면 열려요" : "";
            so.FindProperty("stageRules").objectReferenceValue = stageRules;
            so.FindProperty("lastStage").intValue = endless ? 0 : 5;
            so.FindProperty("useBoss").boolValue = !endless;
            so.FindProperty("bossRules").objectReferenceValue = endless ? null : bossRules;
            so.FindProperty("maxHealthUpgradeLevel").intValue = endless ? 50 : 5;
            so.FindProperty("endScreen").enumValueIndex = endless ? (int)ModeEndScreen.Record : (int)ModeEndScreen.Ending;
            so.FindProperty("unlock").enumValueIndex = endless ? (int)ModeUnlock.NormalCleared : (int)ModeUnlock.Always;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mode);
        }

        struct NewCard
        {
            public string key, name, description, tempArt, note; public CardType type; public int cost, price; public CardTag tags;
            public CardEffect[] effects; public HeroCardAction hero;
        }

        static readonly NewCard[] NewCards =
        {
            new NewCard { key = "LemonBomb", name = "레몬 폭탄", type = CardType.Special, cost = 1, tags = CardTag.Lemon, price = 70,
                effects = new[] { E(CardEffectType.Damage, 8), E(CardEffectType.Sour, 2) }, description = "피해 {0}\n산미 {1}\n소멸",
                hero = HeroCardAction.LemonThrow, tempArt = "ThrownLemon", note = "임시: 레몬 던지기 그림(CardArt_ThrownLemon)을 씀. 정식 그림 CardArt_LemonBomb으로 교체" },
            new NewCard { key = "SmokeRoll", name = "연막 구르기", type = CardType.Defense, cost = 1, price = 80,
                effects = new[] { E(CardEffectType.NegateNextAttack, 1) }, description = "다음 드래곤 공격\n1번 무효\n소멸",
                hero = HeroCardAction.Roll, tempArt = "DodgeRoll", note = "임시: 구르기 그림(CardArt_DodgeRoll)을 씀. 정식 그림 CardArt_SmokeRoll로 교체" },
        };

        static Dictionary<string, CardData> EnsureNewCards()
        {
            var dict = new Dictionary<string, CardData>();
            foreach (var spec in NewCards)
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
                    // 정식 그림이 있으면 그것을, 없으면 임시로 비슷한 카드 그림
                    var art = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Art/CardArt_{spec.key}");
                    bool temp = art == null;
                    if (temp) art = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Art/CardArt_{spec.tempArt}");
                    so.FindProperty("art").objectReferenceValue = art;
                    so.FindProperty("composedImage").objectReferenceValue = DramonAssetBuilder.LoadSprite($"{CardArtPath}/Composed/Card_{spec.key}");
                    so.FindProperty("tempAssetNote").stringValue = temp ? spec.note : "";
                    so.FindProperty("description").stringValue = spec.description;
                    so.FindProperty("tags").intValue = (int)spec.tags;
                    var effects = so.FindProperty("effects");
                    effects.arraySize = spec.effects.Length;
                    for (int i = 0; i < spec.effects.Length; i++) WriteEffect(effects.GetArrayElementAtIndex(i), spec.effects[i]);
                    so.FindProperty("exhaust").boolValue = true;
                    so.FindProperty("heroAction").enumValueIndex = (int)spec.hero;
                    so.FindProperty("heroActionRepeat").intValue = 1;
                    so.FindProperty("shopPrice").intValue = spec.price;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(card);
                }
                dict[spec.key] = card;
            }
            return dict;
        }

        static void WriteEffect(SerializedProperty e, CardEffect value)
        {
            e.FindPropertyRelative("type").enumValueIndex = (int)value.type;
            e.FindPropertyRelative("value").intValue = value.value;
            e.FindPropertyRelative("count").intValue = Mathf.Max(1, value.count);
        }

        /// <summary>강화 수치는 아직 한 번도 정하지 않은 카드에만 (강화 효과가 비고 코스트·가격이 처음 값일 때)</summary>
        static void ApplyUpgrades()
        {
            foreach (var pair in Upgrades)
            {
                var card = AssetDatabase.LoadAssetAtPath<CardData>($"{CardFolder}/Card_{pair.Key}.asset");
                if (card == null) continue;
                var so = new SerializedObject(card);
                var up = so.FindProperty("upgradedEffects");
                var cost = so.FindProperty("upgradedCost");
                var price = so.FindProperty("upgradePrice");
                if (up.arraySize > 0 || cost.intValue != -1 || price.intValue != 100) continue;
                var spec = pair.Value;
                so.FindProperty("upgradable").boolValue = true;
                price.intValue = spec.price;
                cost.intValue = spec.cost;
                up.arraySize = spec.effects.Length;
                for (int i = 0; i < spec.effects.Length; i++) WriteEffect(up.GetArrayElementAtIndex(i), spec.effects[i]);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(card);
            }
        }

        static void ApplyBossHints()
        {
            for (int i = 0; i < DragonKeys.Length; i++)
            {
                var species = AssetDatabase.LoadAssetAtPath<DragonSpecies>($"{DragonFolder}/Dragon{i + 1:00}_{DragonKeys[i]}.asset");
                if (species == null) continue;
                var so = new SerializedObject(species);
                var hint = so.FindProperty("bossHint");
                if (hint == null || (hint.stringValue != DefaultBossHint && !string.IsNullOrEmpty(hint.stringValue))) continue;
                hint.stringValue = BossHints[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(species);
            }
        }

        // ── Prefab ──────────────────────────────
        static FloatingNumber EnsureFloatingPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<FloatingNumber>(FloatingPrefabPath);
            if (existing != null) return existing;
            var root = new GameObject("FloatingNumber", typeof(RectTransform));
            root.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)root.transform;
            rt.sizeDelta = new Vector2(90, 20);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            var text = NewText("Label", root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90, 20),
                TextAnchor.MiddleCenter, 14, "-6");
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            var o = text.gameObject.AddComponent<Outline>();
            o.effectColor = Brown;
            var n = root.AddComponent<FloatingNumber>();
            SetRef(n, "label", text);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, FloatingPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<FloatingNumber>();
        }

        static StatusIconView EnsureStatusIconPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<StatusIconView>(StatusIconPrefabPath);
            if (existing != null) return existing;
            var root = new GameObject("StatusIconView", typeof(RectTransform), typeof(LayoutElement));
            root.layer = LayerMask.NameToLayer("UI");
            ((RectTransform)root.transform).sizeDelta = new Vector2(18, 18);
            var le = root.GetComponent<LayoutElement>();
            le.preferredWidth = 18;
            le.preferredHeight = 18;
            var iconGO = NewUI("Icon", root.transform);
            var irt = (RectTransform)iconGO.transform;
            irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one; irt.offsetMin = irt.offsetMax = Vector2.zero;
            var icon = iconGO.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var count = NewText("Count", root.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(2, -2), new Vector2(14, 12),
                TextAnchor.LowerRight, 10, "2");
            count.color = Color.white;
            count.raycastTarget = false;
            var o = count.gameObject.AddComponent<Outline>();
            o.effectColor = Brown;
            var view = root.AddComponent<StatusIconView>();
            SetRef(view, "icon", icon);
            SetRef(view, "countLabel", count);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, StatusIconPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<StatusIconView>();
        }

        // ── 화면 ────────────────────────────────
        static FloatingNumberUI BuildFloatingNumbers(Transform panel, FloatingNumber prefab, HeroBattlerView hero, DragonBattlerView dragon)
        {
            var battleRoot = panel.Find("Battle");
            var t = battleRoot.Find("FloatingNumbers");
            if (t != null) return t.GetComponent<FloatingNumberUI>();
            var go = NewUI("FloatingNumbers", battleRoot);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.transform.SetAsLastSibling(); // 손패 위
            var ui = go.AddComponent<FloatingNumberUI>();
            SetRef(ui, "prefab", prefab);
            SetRef(ui, "container", rt);
            if (hero != null) SetRef(ui, "playerAnchor", EnsureAnchor(hero.transform, new Vector2(0.5f, 1f), new Vector2(0, 6)));
            if (dragon != null) SetRef(ui, "dragonAnchor", EnsureAnchor(dragon.transform, new Vector2(0.5f, 0.75f), Vector2.zero));
            return ui;
        }

        static RectTransform EnsureAnchor(Transform parent, Vector2 anchor, Vector2 pos)
        {
            var t = parent.Find("FloatAnchor");
            if (t != null) return (RectTransform)t;
            var go = NewUI("FloatAnchor", parent);
            SetAnchored((RectTransform)go.transform, anchor, new Vector2(0.5f, 0.5f), pos, new Vector2(10, 10));
            return (RectTransform)go.transform;
        }

        static void BuildIntent(Transform window, DragonIntentPicker intent, DragonActionExecutor executor, DragonBattleStats stats, CardBattleController controller)
        {
            if (window == null) return;
            var existing = window.Find("Intent") as RectTransform;
            if (existing != null)
            {
                // 처음 설치 위치(체력 숫자와 겹침)에 그대로 있을 때만 숫자 아래로 내림. Scene에서 옮겼으면 그대로 둠
                if (existing.anchoredPosition == new Vector2(0, -3)) existing.anchoredPosition = IntentPosition;
                return;
            }
            // Intent (DragonIntentUI, 항상 켜짐) / Content (켜고 끄는 묶음: 배경 + 아이콘 + 숫자). 드래곤 체력 숫자 아래, Scene에서 옮겨도 유지
            var go = NewUI("Intent", window);
            SetAnchored((RectTransform)go.transform, new Vector2(0, 0), new Vector2(0, 1), IntentPosition, new Vector2(64, 22));
            var content = NewUI("Content", go.transform);
            var crt = (RectTransform)content.transform;
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one; crt.offsetMin = crt.offsetMax = Vector2.zero;
            var bg = content.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.35f);
            bg.raycastTarget = false;
            var iconGO = NewUI("Icon", content.transform);
            SetAnchored((RectTransform)iconGO.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(2, 0), new Vector2(20, 20));
            var icon = iconGO.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.sprite = DramonAssetBuilder.LoadSprite($"{IconPath}/Icon_Intent_Claw");
            var value = NewText("Value", content.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(40, 18),
                TextAnchor.MiddleLeft, 12, "10");
            value.color = Color.white;
            value.raycastTarget = false;
            var o = value.gameObject.AddComponent<Outline>();
            o.effectColor = Brown;
            content.SetActive(false);

            var ui = go.AddComponent<DragonIntentUI>();
            SetRef(ui, "intent", intent);
            SetRef(ui, "executor", executor);
            SetRef(ui, "dragonStats", stats);
            SetRef(ui, "controller", controller);
            SetRef(ui, "root", content);
            SetRef(ui, "icon", icon);
            SetRef(ui, "valueLabel", value);
            SetRef(ui, "clawIcon", DramonAssetBuilder.LoadSprite($"{IconPath}/Icon_Intent_Claw"));
            SetRef(ui, "breathIcon", DramonAssetBuilder.LoadSprite($"{IconPath}/Icon_Intent_Breath"));
            SetRef(ui, "defendIcon", DramonAssetBuilder.LoadSprite($"{IconPath}/Icon_Intent_Defend"));
            SetRef(ui, "healIcon", DramonAssetBuilder.LoadSprite($"{IconPath}/Icon_Intent_Heal"));
            SetRef(ui, "buffIcon", DramonAssetBuilder.LoadSprite($"{IconPath}/Icon_Intent_Buff"));
        }

        static void BuildStatusIcons(Transform window, string name, StatusEffects source, StatusIconView prefab, StatusType[] order, bool belowRight)
        {
            if (window == null || window.Find(name) != null) return;
            var go = NewUI(name, window);
            if (belowRight) SetAnchored((RectTransform)go.transform, new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, -3), new Vector2(80, 18));
            else SetAnchored((RectTransform)go.transform, new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, 3), new Vector2(160, 18));
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 2;
            layout.childAlignment = belowRight ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var ui = go.AddComponent<StatusIconsUI>();
            SetRef(ui, "source", source);
            SetRef(ui, "iconPrefab", prefab);
            SetRef(ui, "container", go.transform);
            var so = new SerializedObject(ui);
            var list = so.FindProperty("icons");
            list.arraySize = order.Length;
            for (int i = 0; i < order.Length; i++)
            {
                var e = list.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("type").enumValueIndex = (int)order[i];
                e.FindPropertyRelative("icon").objectReferenceValue = DramonAssetBuilder.LoadSprite($"{IconPath}/Icon_Status_{order[i]}");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
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
