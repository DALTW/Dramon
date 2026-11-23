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
    /// 드래곤 등장 시스템(깜짝 등장 + 등장 연출 + 결투 화면)을 지금 열린 Forest Scene에 실제 오브젝트로 추가한다.
    /// - 에셋: 드래곤 10종 DragonSpecies, 종류별 NestSpriteSet(기본 둥지 그림으로 임시 연결), 숲 드래곤 목록 DragonSpawnList
    /// - Managers/SwordHunt: DuelSession, DragonEncounterTrigger, DragonHealth, EncounterIntro
    /// - Effects/NestWakeSprite: 결투 직전 둥지 위에 깨어나는 드래곤
    /// - HUD Canvas/DuelScreen: 결투 화면 (배경·발판·드래곤·플레이어·체력 표시 창·메시지 창·임시 버튼), 평소에는 꺼짐
    /// - HUD Canvas/EncounterOverlay: 화면 번쩍임·검은 전환
    /// 이미 있는 에셋·오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결·설정도 덮어쓰지 않는다.
    /// </summary>
    public static class DragonEncounterInstaller
    {
        const string DragonArtFolder = "Dragon/Types";
        const string DragonDataFolder = DramonAssetBuilder.SettingsFolder + "/Dragons";
        const string NestDataFolder = DragonDataFolder + "/Nests";
        const string SpawnListPath = DragonDataFolder + "/ForestDragonSpawnList.asset";

        struct DragonInfo
        {
            public string key, name;
            public Color breath;
            public DragonInfo(string key, string name, Color breath) { this.key = key; this.name = name; this.breath = breath; }
        }

        static readonly DragonInfo[] Dragons =
        {
            new DragonInfo("Ember",   "화염 드래곤",   new Color(1.00f, 0.60f, 0.10f)),
            new DragonInfo("Forest",  "숲 드래곤",     new Color(1.00f, 0.88f, 0.25f)),
            new DragonInfo("Storm",   "폭풍 드래곤",   new Color(1.00f, 0.95f, 0.35f)),
            new DragonInfo("Shadow",  "그림자 드래곤", new Color(0.62f, 0.28f, 0.92f)),
            new DragonInfo("Gold",    "황금 드래곤",   new Color(1.00f, 0.72f, 0.20f)),
            new DragonInfo("Frost",   "서리 드래곤",   new Color(0.40f, 0.75f, 1.00f)),
            new DragonInfo("Toxic",   "독늪 드래곤",   new Color(0.62f, 1.00f, 0.30f)),
            new DragonInfo("Tide",    "해류 드래곤",   new Color(0.30f, 0.60f, 1.00f)),
            new DragonInfo("Stone",   "바위 드래곤",   new Color(1.00f, 0.86f, 0.30f)),
            new DragonInfo("Crystal", "수정 드래곤",   new Color(1.00f, 0.30f, 0.85f)),
        };

        [MenuItem("Dramon/기능 설치 - 드래곤 등장·결투 화면 (현재 Scene)")]
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
                Debug.Log("[Dramon] 드래곤 등장·결투 화면 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            var managers = GameObject.Find("Managers");
            var hudCanvas = GameObject.Find("HUD Canvas");
            var board = Object.FindAnyObjectByType<ForestBoard>(FindObjectsInactive.Include);
            var phase = Object.FindAnyObjectByType<GamePhaseController>(FindObjectsInactive.Include);
            var detector = Object.FindAnyObjectByType<SealedNestHoverDetector>(FindObjectsInactive.Include);
            if (managers == null || hudCanvas == null || board == null || phase == null || detector == null)
            {
                EditorUtility.DisplayDialog("Dramon",
                    "Forest Scene이 열려 있어야 하고, 레몬 표시 단계와 봉인된 굴 감지기(Managers/SwordHunt/SealedNestHoverDetector)가 있어야 해요.", "확인");
                return false;
            }

            // ── 에셋 ─────────────────────────────
            var spawnList = EnsureSpawnList();
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();

            // ── Managers/SwordHunt ───────────────
            var swordHunt = FindOrCreateChild(managers, "SwordHunt");
            var session = GetOrAdd<DuelSession>(FindOrCreateChild(swordHunt, "DuelSession"));
            var trigger = GetOrAdd<DragonEncounterTrigger>(FindOrCreateChild(swordHunt, "DragonEncounterTrigger"));
            var health = GetOrAdd<DragonHealth>(FindOrCreateChild(swordHunt, "DragonHealth"));
            var intro = GetOrAdd<EncounterIntroSequence>(FindOrCreateChild(swordHunt, "EncounterIntro"));

            // ── 둥지 위 깨어나는 드래곤 (월드) ──
            var wake = EnsureWakeSprite();

            // ── 결투 화면 + 연출 UI ──────────────
            var duelScreenT = hudCanvas.transform.Find("DuelScreen");
            var duelScreenRoot = duelScreenT != null ? duelScreenT.gameObject : BuildDuelScreen(hudCanvas.transform, phase, session, health, buttonPrefab);
            var overlayT = hudCanvas.transform.Find("EncounterOverlay");
            var overlay = overlayT != null ? overlayT.gameObject : BuildOverlay(hudCanvas.transform);
            overlay.transform.SetAsLastSibling(); // 번쩍임·전환은 결투 화면보다 위에

            // ── 연결 (비어 있는 것만) ─────────────
            SetRefIfEmpty(session, "phaseController", phase);
            SetRefIfEmpty(session, "board", board);
            SetRefIfEmpty(session, "spawnList", spawnList);
            SetRefIfEmpty(session, "dragonHealth", health);
            SetRefIfEmpty(session, "introSequence", intro);
            SetRefIfEmpty(trigger, "hoverDetector", detector);
            SetRefIfEmpty(trigger, "duelSession", session);

            SetRefIfEmpty(intro, "phaseController", phase);
            SetRefIfEmpty(intro, "duelScreen", duelScreenRoot.GetComponent<DuelScreenUI>());
            SetRefIfEmpty(intro, "slideIn", duelScreenRoot.GetComponent<DuelStageSlideIn>());
            SetRefIfEmpty(intro, "healthUI", duelScreenRoot.GetComponent<DragonHealthUI>());
            SetRefIfEmpty(intro, "message", duelScreenRoot.GetComponent<EncounterMessageUI>());
            SetRefIfEmpty(intro, "battler", duelScreenRoot.GetComponentInChildren<DragonBattlerView>(true));
            SetRefIfEmpty(intro, "flash", overlay.GetComponentInChildren<ScreenFlashUI>(true));
            SetRefIfEmpty(intro, "wipe", overlay.GetComponentInChildren<ScreenWipeTransition>(true));
            SetRefIfEmpty(intro, "wakeSprite", wake);

            AssetDatabase.SaveAssets();
            return true;
        }

        // ── 에셋 ─────────────────────────────────
        static DragonSpawnList EnsureSpawnList()
        {
            DramonAssetBuilder.EnsureFolder(NestDataFolder);
            var tileSprites = DramonAssetBuilder.EnsureTileSpriteSet();

            var list = AssetDatabase.LoadAssetAtPath<DragonSpawnList>(SpawnListPath);
            bool created = list == null;
            if (created)
            {
                list = ScriptableObject.CreateInstance<DragonSpawnList>();
                AssetDatabase.CreateAsset(list, SpawnListPath);
            }

            // 종 데이터는 없는 것만 만든다. 목록은 처음 만들 때만 채운다 (사용자가 뺀 드래곤을 되살리지 않음)
            var so = new SerializedObject(list);
            var arr = so.FindProperty("species");
            for (int i = 0; i < Dragons.Length; i++)
            {
                var nest = EnsureNestSprites(i + 1, Dragons[i].key, tileSprites);
                var species = EnsureSpecies(i + 1, Dragons[i], nest);
                if (!created || species == null) continue;
                arr.arraySize++;
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = species;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(list);
            return list;
        }

        static NestSpriteSet EnsureNestSprites(int index, string key, ForestTileSpriteSet tileSprites)
        {
            string path = $"{NestDataFolder}/NestSprites{index:00}_{key}.asset";
            var set = AssetDatabase.LoadAssetAtPath<NestSpriteSet>(path);
            if (set != null) return set;

            set = ScriptableObject.CreateInstance<NestSpriteSet>();
            AssetDatabase.CreateAsset(set, path);
            // 종류별 둥지 그림이 아직 없어서 기본 둥지 그림으로 임시 연결
            var so = new SerializedObject(set);
            if (tileSprites != null)
            {
                so.FindProperty("revealed").objectReferenceValue = tileSprites.NestRevealed;
                so.FindProperty("purified").objectReferenceValue = tileSprites.NestPurified;
                var frames = so.FindProperty("activeFrames");
                var src = tileSprites.NestActiveFrames ?? new Sprite[0];
                frames.arraySize = src.Length;
                for (int i = 0; i < src.Length; i++)
                    frames.GetArrayElementAtIndex(i).objectReferenceValue = src[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            return set;
        }

        static DragonSpecies EnsureSpecies(int index, DragonInfo info, NestSpriteSet nest)
        {
            string path = $"{DragonDataFolder}/Dragon{index:00}_{info.key}.asset";
            var species = AssetDatabase.LoadAssetAtPath<DragonSpecies>(path);
            if (species != null) return species;

            species = ScriptableObject.CreateInstance<DragonSpecies>();
            AssetDatabase.CreateAsset(species, path);
            var so = new SerializedObject(species);
            so.FindProperty("number").intValue = index;
            so.FindProperty("displayName").stringValue = info.name;
            so.FindProperty("breathColor").colorValue = info.breath;
            so.FindProperty("maxHealth").intValue = 100;
            string art = $"{DragonArtFolder}/{index:00}_{info.key}/Dragon{index:00}_{info.key}_";
            SetSprite(so, "battleIdle", art + "Battle_Idle");
            SetSprite(so, "battleAttack", art + "Battle_Attack");
            SetSprite(so, "battleHurt", art + "Battle_Hurt");
            SetSprite(so, "fieldSleeping", art + "Field_Sleeping");
            SetSprite(so, "fieldSleepingGlow", art + "Field_Sleeping_Glow");
            so.FindProperty("nestSprites").objectReferenceValue = nest;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(species);
            return species;
        }

        static void SetSprite(SerializedObject so, string property, string artPath)
        {
            var sprite = DramonAssetBuilder.LoadSprite(artPath);
            if (sprite == null) // 그림이 아직 없는 종류는 기존 드래곤 그림으로 임시 연결
                sprite = DramonAssetBuilder.LoadSprite(FallbackArt(property));
            so.FindProperty(property).objectReferenceValue = sprite;
        }

        static string FallbackArt(string property)
        {
            switch (property)
            {
                case "battleAttack": return "Dragon/Dragon_Battle_Attack";
                case "battleHurt": return "Dragon/Dragon_Battle_Hurt";
                case "fieldSleeping":
                case "fieldSleepingGlow": return "Dragon/Dragon_Field_Sleeping";
                default: return "Dragon/Dragon_Battle_Idle";
            }
        }

        // ── 월드: 둥지 위 깨어나는 드래곤 ───────
        static NestWakeSpriteView EnsureWakeSprite()
        {
            var effects = GameObject.Find("Effects");
            if (effects == null) effects = new GameObject("Effects");
            var go = FindOrCreateChild(effects, "NestWakeSprite");
            var sr = GetOrAdd<SpriteRenderer>(go);
            var view = go.GetComponent<NestWakeSpriteView>();
            if (view == null)
            {
                sr.sortingOrder = 100; // 숲 칸·레몬보다 앞
                sr.enabled = false;
                view = go.AddComponent<NestWakeSpriteView>();
                SetRef(view, "spriteRenderer", sr);
            }
            return view;
        }

        // ── 결투 화면 ────────────────────────────
        static GameObject BuildDuelScreen(Transform hud, GamePhaseController phase, DuelSession session, DragonHealth health, Button buttonPrefab)
        {
            var root = NewUI("DuelScreen", hud);
            Stretch((RectTransform)root.transform);

            // 배경 (뒤의 보드 클릭도 막음)
            var panel = NewUI("Panel", root.transform);
            Stretch((RectTransform)panel.transform);
            var canvasGroup = panel.AddComponent<CanvasGroup>();
            var bg = panel.AddComponent<Image>();
            bg.sprite = DramonAssetBuilder.LoadSprite("Battle/BattleBG_ForestNight");
            bg.raycastTarget = true;

            // 드래곤 쪽 (오른쪽 위 발판, 발밑 기준)
            var dragonSide = NewUI("DragonSide", panel.transform);
            SetAnchored((RectTransform)dragonSide.transform, new Vector2(0.76f, 0.4f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(277, 192));
            var dragonGO = NewUI("Dragon", dragonSide.transform);
            Stretch((RectTransform)dragonGO.transform);
            var dragonImage = dragonGO.AddComponent<Image>();
            dragonImage.sprite = DramonAssetBuilder.LoadSprite($"{DragonArtFolder}/01_Ember/Dragon01_Ember_Battle_Idle");
            dragonImage.preserveAspect = true;
            dragonImage.raycastTarget = false;
            var audio = dragonGO.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            var battler = dragonGO.AddComponent<DragonBattlerView>();
            SetRef(battler, "image", dragonImage);
            SetRef(battler, "audioSource", audio);

            // 플레이어 쪽 (왼쪽 아래 발판, 뒷모습)
            var playerSide = NewUI("PlayerSide", panel.transform);
            SetAnchored((RectTransform)playerSide.transform, new Vector2(0.25f, 0.19f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(43, 64));
            var playerGO = NewUI("Player", playerSide.transform);
            Stretch((RectTransform)playerGO.transform);
            var player = playerGO.AddComponent<Image>();
            player.sprite = DramonAssetBuilder.LoadSprite("Player/Player_Walk_Sheet", "Player_Walk_Up_0");
            player.preserveAspect = true;
            player.raycastTarget = false;

            // 체력 표시 창 (막대만)
            var healthWindow = NewSmallWindow("HealthWindow", panel.transform);
            SetAnchored(healthWindow.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12, -12), new Vector2(160, 24));
            var barBack = NewUI("BarBack", healthWindow.transform);
            Inset((RectTransform)barBack.transform, 7);
            var barBackImage = barBack.AddComponent<Image>();
            barBackImage.color = new Color32(0x3B, 0x24, 0x14, 0xFF);
            barBackImage.raycastTarget = false;
            var barArea = NewUI("BarArea", barBack.transform);
            Inset((RectTransform)barArea.transform, 1);
            var barFillGO = NewUI("BarFill", barArea.transform);
            Stretch((RectTransform)barFillGO.transform);
            var barFill = barFillGO.AddComponent<Image>();
            barFill.color = new Color32(0x58, 0xC8, 0x48, 0xFF);
            barFill.raycastTarget = false;

            // 메시지 창
            var messageWindow = NewSmallWindow("MessageWindow", panel.transform);
            SetAnchored(messageWindow.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(360, 32));
            var messageText = NewText("MessageText", messageWindow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(340, 22), TextAnchor.MiddleLeft, 12, "");

            // [임시] 승패 버튼 — 카드 결투가 생기면 교체
            var buttons = NewUI("Buttons", panel.transform);
            SetAnchored((RectTransform)buttons.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(160, 24));
            var win = NewButton(buttonPrefab, buttons.transform, "WinButton", "결투 승리");
            SetAnchored((RectTransform)win.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-40, 0), new Vector2(64, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(win.transform);
            UnityEventTools.AddPersistentListener(win.onClick, new UnityAction(session.Win));
            PrefabUtility.RecordPrefabInstancePropertyModifications(win);
            var lose = NewButton(buttonPrefab, buttons.transform, "LoseButton", "결투 패배");
            SetAnchored((RectTransform)lose.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40, 0), new Vector2(64, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(lose.transform);
            UnityEventTools.AddPersistentListener(lose.onClick, new UnityAction(session.Lose));
            PrefabUtility.RecordPrefabInstancePropertyModifications(lose);
            var note = NewText("TempNote", panel.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-6, 2), new Vector2(220, 12),
                TextAnchor.LowerRight, 9, "[임시] 카드 결투 전까지 쓰는 버튼");
            note.color = Color.white;
            note.transform.SetParent(buttons.transform, true); // 버튼과 함께 보이고 숨겨지게

            buttons.SetActive(false);
            healthWindow.gameObject.SetActive(false);
            messageWindow.gameObject.SetActive(false);
            panel.SetActive(false);

            // 루트에 붙는 표시 스크립트들 (루트는 항상 켜져 있음)
            var screen = root.AddComponent<DuelScreenUI>();
            SetRef(screen, "phaseController", phase);
            SetRef(screen, "panel", panel);
            SetRef(screen, "canvasGroup", canvasGroup);
            SetRef(screen, "buttons", buttons);

            var slide = root.AddComponent<DuelStageSlideIn>();
            SetRef(slide, "dragonSide", dragonSide.transform);
            SetRef(slide, "playerSide", playerSide.transform);

            var healthUI = root.AddComponent<DragonHealthUI>();
            SetRef(healthUI, "health", health);
            SetRef(healthUI, "window", healthWindow.gameObject);
            SetRef(healthUI, "fill", barFill);

            var messageUI = root.AddComponent<EncounterMessageUI>();
            SetRef(messageUI, "window", messageWindow.gameObject);
            SetRef(messageUI, "label", messageText);

            return root;
        }

        // ── 연출 오버레이 (번쩍임·검은 전환) ─────
        static GameObject BuildOverlay(Transform hud)
        {
            var root = NewUI("EncounterOverlay", hud);
            Stretch((RectTransform)root.transform);

            var flashGO = NewUI("Flash", root.transform);
            Stretch((RectTransform)flashGO.transform);
            var flashImage = flashGO.AddComponent<Image>();
            flashImage.color = new Color(1f, 1f, 1f, 0f);
            flashImage.raycastTarget = false;
            flashImage.enabled = false;
            var flash = flashGO.AddComponent<ScreenFlashUI>();
            SetRef(flash, "image", flashImage);

            var wipeGO = NewUI("Wipe", root.transform);
            var wipeRt = (RectTransform)wipeGO.transform;
            wipeRt.anchorMin = Vector2.zero;
            wipeRt.anchorMax = new Vector2(0f, 1f);
            wipeRt.offsetMin = wipeRt.offsetMax = Vector2.zero;
            var wipeImage = wipeGO.AddComponent<Image>();
            wipeImage.color = Color.black;
            wipeImage.raycastTarget = false;
            wipeImage.enabled = false;
            var wipe = wipeGO.AddComponent<ScreenWipeTransition>();
            SetRef(wipe, "image", wipeImage);

            return root;
        }

        // ── 작은 도우미 ──────────────────────────
        static Image NewSmallWindow(string name, Transform parent)
        {
            var img = NewWindow(name, parent);
            img.pixelsPerUnitMultiplier = 2f; // 테두리를 절반 크기로 그려 낮은 창에서도 안쪽이 보이게
            img.raycastTarget = false;
            return img;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void Inset(RectTransform rt, float px)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(px, px);
            rt.offsetMax = new Vector2(-px, -px);
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
