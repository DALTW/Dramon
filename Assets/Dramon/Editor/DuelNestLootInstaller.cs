using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Dramon.EditorTools.DramonEditorUtil;

namespace Dramon.EditorTools
{
    /// <summary>
    /// 드래곤별 결투 둥지 배경 + 승리 시 재료 드랍을 지금 열린 Forest Scene에 추가한다.
    /// - 에셋: 둥지 배경 10장·재료 시트 Import 설정, 투명 시트를 아이콘 30개로 자르기, 재료 MaterialData 30개
    /// - DragonSpecies 10종: 결투 배경·발판 위치, 재료 드랍표 채우기
    /// - Managers/PlayerInventory, Managers/SwordHunt/LootRoller
    /// - HUD Canvas/DuelScreen: DuelBackgroundView, 전리품 창(Panel/LootWindow), LootRow Prefab
    /// - HUD Canvas/TopBar/MaterialPanel: 재료 개수 표시
    /// 이미 있는 에셋·오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결·값(Import 설정, 자른 영역 포함)도 덮어쓰지 않는다.
    /// </summary>
    public static class DuelNestLootInstaller
    {
        const string NestArtPath = "Battle/Nests";
        const string MaterialArtFolder = DramonAssetBuilder.ArtFolder + "/Items/Materials";
        const string SheetPath = MaterialArtFolder + "/DragonMaterials_Sheet.png";
        const string TransparentSheetPath = MaterialArtFolder + "/DragonMaterials_Sheet_Transparent.png";
        const string MaterialDataFolder = DramonAssetBuilder.SettingsFolder + "/Materials";
        const string DragonDataFolder = DramonAssetBuilder.SettingsFolder + "/Dragons";
        const string LootRowPrefabPath = DramonAssetBuilder.PrefabFolder + "/UI/LootRow.prefab";
        const string ImportSetupMark = "dramon-import-setup";

        static readonly string[] Keys = { "Ember", "Forest", "Storm", "Shadow", "Gold", "Frost", "Toxic", "Tide", "Stone", "Crystal" };
        static readonly string[] RarityKeys = { "Common", "Uncommon", "Rare" };

        // 배경 그림을 열어서 잰 발판 윗면 가운데 (배경 왼쪽 아래 = 0,0)
        static readonly Vector2[] DragonStandPoints =
        {
            new Vector2(0.70f, 0.48f), new Vector2(0.75f, 0.59f), new Vector2(0.79f, 0.58f), new Vector2(0.71f, 0.59f), new Vector2(0.75f, 0.62f),
            new Vector2(0.73f, 0.56f), new Vector2(0.68f, 0.51f), new Vector2(0.72f, 0.56f), new Vector2(0.72f, 0.55f), new Vector2(0.69f, 0.56f),
        };
        static readonly Vector2[] PlayerStandPoints =
        {
            new Vector2(0.29f, 0.31f), new Vector2(0.20f, 0.23f), new Vector2(0.20f, 0.21f), new Vector2(0.24f, 0.23f), new Vector2(0.20f, 0.23f),
            new Vector2(0.22f, 0.21f), new Vector2(0.28f, 0.22f), new Vector2(0.20f, 0.21f), new Vector2(0.20f, 0.20f), new Vector2(0.21f, 0.27f),
        };

        // 재료 이름 (흔함, 보통, 희귀) / 판매가
        static readonly string[,] MaterialNames =
        {
            { "불꽃 비늘", "용암 발톱", "불씨 심장" },
            { "이끼 비늘", "가지 뿔 조각", "생명의 씨앗" },
            { "뇌운 비늘", "폭풍 날개막", "번개 결정" },
            { "흑요 비늘", "핏빛 날개막", "그림자 눈" },
            { "황금 비늘", "보석 박힌 발톱", "황금 진주" },
            { "서리 비늘", "얼음 송곳니", "영원의 얼음 결정" },
            { "독 비늘", "독샘 주머니", "보랏빛 독침" },
            { "물결 비늘", "산호 뿔 조각", "바다 진주" },
            { "바위 비늘", "화강 발톱", "대지의 핵" },
            { "수정 비늘", "수정 뿔 조각", "마력 수정 핵" },
        };
        static readonly int[,] SellPrices =
        {
            { 15, 40, 120 }, { 15, 40, 120 }, { 15, 40, 120 }, { 20, 50, 150 }, { 30, 70, 200 },
            { 15, 40, 120 }, { 15, 40, 120 }, { 15, 40, 120 }, { 15, 40, 120 }, { 20, 50, 150 },
        };
        // 희귀도별 드랍 확률과 나올 때의 개수 (최소~최대)
        static readonly float[] DropChances = { 0.8f, 0.4f, 0.1f };
        static readonly int[] MinCounts = { 1, 1, 1 };
        static readonly int[] MaxCounts = { 3, 2, 1 };

        struct SliceRect
        {
            public string name; public int x, y, w, h;
            public SliceRect(string name, int x, int y, int w, int h) { this.name = name; this.x = x; this.y = y; this.w = w; this.h = h; }
        }

        // DragonMaterials_Sheet_Transparent (768×1376)에서 아이콘 영역을 찾아 낸 결과 (왼쪽 아래 기준 픽셀).
        // 칸 격자가 아니라 그림 덩어리로 찾았고, 반짝이처럼 떨어진 조각은 가장 가까운 아이콘에 포함했다.
        static readonly SliceRect[] Slices =
        {
            new SliceRect("Mat01_Ember_Common", 39, 1249, 108, 108),
            new SliceRect("Mat01_Ember_Uncommon", 325, 1241, 120, 122),
            new SliceRect("Mat01_Ember_Rare", 625, 1245, 108, 120),
            new SliceRect("Mat02_Forest_Common", 39, 1109, 108, 114),
            new SliceRect("Mat02_Forest_Uncommon", 326, 1105, 121, 121),
            new SliceRect("Mat02_Forest_Rare", 630, 1102, 98, 124),
            new SliceRect("Mat03_Storm_Common", 39, 971, 111, 111),
            new SliceRect("Mat03_Storm_Uncommon", 315, 967, 145, 121),
            new SliceRect("Mat03_Storm_Rare", 633, 967, 95, 118),
            new SliceRect("Mat04_Shadow_Common", 42, 839, 101, 104),
            new SliceRect("Mat04_Shadow_Uncommon", 312, 839, 149, 104),
            new SliceRect("Mat04_Shadow_Rare", 623, 833, 114, 114),
            new SliceRect("Mat05_Gold_Common", 39, 699, 108, 112),
            new SliceRect("Mat05_Gold_Uncommon", 328, 699, 116, 114),
            new SliceRect("Mat05_Gold_Rare", 623, 697, 114, 114),
            new SliceRect("Mat06_Frost_Common", 39, 564, 108, 112),
            new SliceRect("Mat06_Frost_Uncommon", 328, 561, 119, 118),
            new SliceRect("Mat06_Frost_Rare", 627, 561, 108, 118),
            new SliceRect("Mat07_Toxic_Common", 39, 428, 108, 111),
            new SliceRect("Mat07_Toxic_Uncommon", 329, 424, 114, 117),
            new SliceRect("Mat07_Toxic_Rare", 625, 427, 110, 114),
            new SliceRect("Mat08_Tide_Common", 39, 292, 108, 111),
            new SliceRect("Mat08_Tide_Uncommon", 328, 287, 119, 121),
            new SliceRect("Mat08_Tide_Rare", 623, 293, 116, 109),
            new SliceRect("Mat09_Stone_Common", 39, 155, 108, 110),
            new SliceRect("Mat09_Stone_Uncommon", 326, 151, 119, 120),
            new SliceRect("Mat09_Stone_Rare", 629, 155, 105, 110),
            new SliceRect("Mat10_Crystal_Common", 39, 17, 108, 110),
            new SliceRect("Mat10_Crystal_Uncommon", 331, 11, 112, 122),
            new SliceRect("Mat10_Crystal_Rare", 629, 13, 106, 120),
        };

        [MenuItem("Dramon/기능 설치 - 결투 둥지 배경·재료 드랍 (현재 Scene)")]
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
                Debug.Log("[Dramon] 결투 둥지 배경·재료 드랍 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            var managers = GameObject.Find("Managers");
            var hudCanvas = GameObject.Find("HUD Canvas");
            var phase = Object.FindAnyObjectByType<GamePhaseController>(FindObjectsInactive.Include);
            var session = Object.FindAnyObjectByType<DuelSession>(FindObjectsInactive.Include);
            var intro = Object.FindAnyObjectByType<EncounterIntroSequence>(FindObjectsInactive.Include);
            var duelScreen = Object.FindAnyObjectByType<DuelScreenUI>(FindObjectsInactive.Include);
            if (managers == null || hudCanvas == null || phase == null || session == null || intro == null || duelScreen == null)
            {
                EditorUtility.DisplayDialog("Dramon",
                    "Forest Scene이 열려 있어야 하고, 먼저 '기능 설치 - 드래곤 등장·결투 화면'이 설치돼 있어야 해요.", "확인");
                return false;
            }

            // ── 에셋 ─────────────────────────────
            AssetDatabase.Refresh();
            for (int i = 0; i < Keys.Length; i++)
                SetupTextureImport($"{DramonAssetBuilder.ArtFolder}/{NestArtPath}/{NestFileName(i)}.png", SpriteImportMode.Single);
            SetupTextureImport(SheetPath, SpriteImportMode.Single);
            SetupTextureImport(TransparentSheetPath, SpriteImportMode.Multiple);
            SliceMaterialSheet();
            var materials = EnsureMaterials();
            UpdateSpecies(materials);
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();
            var rowPrefab = EnsureLootRowPrefab();

            // ── Managers ─────────────────────────
            var inventory = GetOrAdd<PlayerInventory>(FindOrCreateChild(managers, "PlayerInventory"));
            SetRefIfEmpty(inventory, "phaseController", phase);
            var roller = GetOrAdd<LootRoller>(FindOrCreateChild(session.transform.parent.gameObject, "LootRoller"));

            // ── 결투 화면 ────────────────────────
            var root = duelScreen.gameObject;
            var panel = root.transform.Find("Panel");
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Dramon", "HUD Canvas/DuelScreen/Panel을 찾을 수 없어요.", "확인");
                return false;
            }

            var background = root.GetComponent<DuelBackgroundView>();
            if (background == null)
            {
                background = root.AddComponent<DuelBackgroundView>();
                SetRef(background, "background", panel.GetComponent<Image>());
                SetRef(background, "defaultSprite", DramonAssetBuilder.LoadSprite("Battle/BattleBG_ForestNight"));
            }
            var slide = root.GetComponent<DuelStageSlideIn>();
            if (slide != null) SetRefIfEmpty(slide, "standArea", panel);

            var lootUI = root.GetComponent<LootResultUI>();
            if (lootUI == null)
                lootUI = BuildLootWindow(root, panel, phase, rowPrefab, buttonPrefab, root.GetComponent<EncounterMessageUI>());

            // ── HUD (TopBar) ─────────────────────
            var topBar = hudCanvas.transform.Find("TopBar");
            if (topBar != null && topBar.Find("MaterialPanel") == null) BuildMaterialPanel(topBar, inventory);

            // ── 연결 (비어 있는 것만) ─────────────
            SetRefIfEmpty(intro, "background", background);
            SetRefIfEmpty(session, "duelScreen", duelScreen);
            SetRefIfEmpty(session, "lootRoller", roller);
            SetRefIfEmpty(session, "inventory", inventory);
            SetRefIfEmpty(session, "lootResult", lootUI);

            AssetDatabase.SaveAssets();
            return true;
        }

        static string NestFileName(int i) => $"BattleBG_Nest{i + 1:00}_{Keys[i]}";

        // ── Import 설정 (처음 한 번만) ────────────
        static void SetupTextureImport(string path, SpriteImportMode mode)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning($"[Dramon] 그림을 찾을 수 없어요: {path}"); return; }
            if (importer.userData == ImportSetupMark) return; // 이미 설정함 — 사용자가 바꾼 Import 설정은 그대로

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = mode;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.userData = ImportSetupMark;
            importer.SaveAndReimport();
        }

        // ── 재료 시트 자르기 (처음 한 번만) ───────
        static void SliceMaterialSheet()
        {
            var importer = AssetImporter.GetAtPath(TransparentSheetPath) as TextureImporter;
            if (importer == null) return;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            if (provider.GetSpriteRects().Any(r => r.name.StartsWith("Mat"))) return; // 이미 잘라 둠

            var rects = Slices.Select(s => new SpriteRect
            {
                name = s.name,
                rect = new Rect(s.x, s.y, s.w, s.h),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                spriteID = GUID.Generate(),
            }).ToArray();
            provider.SetSpriteRects(rects);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null)
                names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        // ── 재료 데이터 30개 ──────────────────────
        static MaterialData[,] EnsureMaterials()
        {
            DramonAssetBuilder.EnsureFolder(MaterialDataFolder);
            var icons = AssetDatabase.LoadAllAssetRepresentationsAtPath(TransparentSheetPath)
                .OfType<Sprite>().GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First());

            var result = new MaterialData[Keys.Length, RarityKeys.Length];
            for (int i = 0; i < Keys.Length; i++)
            for (int r = 0; r < RarityKeys.Length; r++)
            {
                string assetName = $"Mat{i + 1:00}_{Keys[i]}_{RarityKeys[r]}";
                string path = $"{MaterialDataFolder}/{assetName}.asset";
                icons.TryGetValue(assetName, out var icon);

                var data = AssetDatabase.LoadAssetAtPath<MaterialData>(path);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<MaterialData>();
                    AssetDatabase.CreateAsset(data, path);
                    var so = new SerializedObject(data);
                    so.FindProperty("displayName").stringValue = MaterialNames[i, r];
                    so.FindProperty("icon").objectReferenceValue = icon;
                    so.FindProperty("rarity").enumValueIndex = r;
                    so.FindProperty("sellPrice").intValue = SellPrices[i, r];
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(data);
                }
                else if (data.Icon == null && icon != null)
                {
                    var so = new SerializedObject(data); // 아이콘만 비어 있으면 채움
                    so.FindProperty("icon").objectReferenceValue = icon;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                result[i, r] = data;
            }
            return result;
        }

        // ── 드래곤 10종: 결투 둥지 + 드랍표 ───────
        static void UpdateSpecies(MaterialData[,] materials)
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                string path = $"{DragonDataFolder}/Dragon{i + 1:00}_{Keys[i]}.asset";
                var species = AssetDatabase.LoadAssetAtPath<DragonSpecies>(path);
                if (species == null) { Debug.LogWarning($"[Dramon] 드래곤 데이터를 찾을 수 없어요: {path}"); continue; }

                var so = new SerializedObject(species);
                // 결투 둥지: 배경이 비어 있을 때만 배경과 발판 위치를 함께 넣는다
                var bg = so.FindProperty("duelBackground");
                if (bg.objectReferenceValue == null)
                {
                    bg.objectReferenceValue = DramonAssetBuilder.LoadSprite($"{NestArtPath}/{NestFileName(i)}");
                    so.FindProperty("useStandPoints").boolValue = true;
                    so.FindProperty("dragonStandPoint").vector2Value = DragonStandPoints[i];
                    so.FindProperty("playerStandPoint").vector2Value = PlayerStandPoints[i];
                }
                // 드랍표: 비어 있을 때만 채운다
                var drops = so.FindProperty("materialDrops");
                if (drops.arraySize == 0)
                {
                    for (int r = 0; r < RarityKeys.Length; r++)
                    {
                        drops.arraySize++;
                        var e = drops.GetArrayElementAtIndex(drops.arraySize - 1);
                        e.FindPropertyRelative("material").objectReferenceValue = materials[i, r];
                        e.FindPropertyRelative("chance").floatValue = DropChances[r];
                        e.FindPropertyRelative("minCount").intValue = MinCounts[r];
                        e.FindPropertyRelative("maxCount").intValue = MaxCounts[r];
                    }
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(species);
            }
        }

        // ── 전리품 창의 재료 한 줄 Prefab ─────────
        static LootRowView EnsureLootRowPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<LootRowView>(LootRowPrefabPath);
            if (existing != null) return existing;
            DramonAssetBuilder.EnsureFolder(DramonAssetBuilder.PrefabFolder + "/UI");

            var go = new GameObject("LootRow", typeof(RectTransform), typeof(LayoutElement));
            go.layer = LayerMask.NameToLayer("UI");
            ((RectTransform)go.transform).sizeDelta = new Vector2(176, 26);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = 26;

            var iconGO = NewUI("Icon", go.transform);
            SetAnchored((RectTransform)iconGO.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(2, 0), new Vector2(24, 24));
            var icon = iconGO.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var nameText = NewText("Name", go.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(32, 0), new Vector2(104, 20),
                TextAnchor.MiddleLeft, 11, "재료 이름");
            var countText = NewText("Count", go.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-4, 0), new Vector2(36, 20),
                TextAnchor.MiddleRight, 11, "x1");

            var view = go.AddComponent<LootRowView>();
            SetRef(view, "icon", icon);
            SetRef(view, "nameLabel", nameText);
            SetRef(view, "countLabel", countText);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, LootRowPrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<LootRowView>();
        }

        // ── 전리품 창 (DuelScreen/Panel/LootWindow) ─
        static LootResultUI BuildLootWindow(GameObject root, Transform panel, GamePhaseController phase, LootRowView rowPrefab,
            Button buttonPrefab, EncounterMessageUI message)
        {
            var window = NewWindow("LootWindow", panel);
            window.pixelsPerUnitMultiplier = 2f; // 낮은 창에서도 안쪽이 보이게 테두리를 절반 크기로
            SetAnchored(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 24), new Vector2(200, 140));

            NewText("Title", window.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -8), new Vector2(180, 16),
                TextAnchor.MiddleCenter, 12, "전리품");

            var list = NewUI("List", window.transform);
            SetAnchored((RectTransform)list.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -26), new Vector2(176, 80));
            var layout = list.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 1;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var empty = NewText("EmptyLabel", window.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 6), new Vector2(176, 20),
                TextAnchor.MiddleCenter, 11, "얻은 재료가 없어요");
            empty.gameObject.SetActive(false);

            var confirm = NewButton(buttonPrefab, window.transform, "ConfirmButton", "확인");
            SetAnchored((RectTransform)confirm.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(64, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(confirm.transform);

            window.transform.SetAsLastSibling(); // 결투 화면에서 가장 위에
            window.gameObject.SetActive(false);

            var ui = root.AddComponent<LootResultUI>();
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "window", window.gameObject);
            SetRef(ui, "listRoot", list.transform);
            SetRef(ui, "rowPrefab", rowPrefab);
            SetRef(ui, "emptyLabel", empty.gameObject);
            SetRef(ui, "confirmButton", confirm);
            SetRef(ui, "message", message);

            UnityEventTools.AddPersistentListener(confirm.onClick, new UnityAction(ui.Confirm));
            PrefabUtility.RecordPrefabInstancePropertyModifications(confirm);
            return ui;
        }

        // ── HUD 재료 개수 (TopBar/MaterialPanel) ───
        static void BuildMaterialPanel(Transform topBar, PlayerInventory inventory)
        {
            var panel = NewWindow("MaterialPanel", topBar);
            panel.pixelsPerUnitMultiplier = 2f;
            panel.raycastTarget = false;
            SetAnchored(panel.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-160, 0), new Vector2(76, 28)); // 상점 버튼 왼쪽

            var iconGO = NewUI("Icon", panel.transform);
            SetAnchored((RectTransform)iconGO.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(6, 0), new Vector2(16, 16));
            var icon = iconGO.AddComponent<Image>();
            icon.sprite = AssetDatabase.LoadAllAssetRepresentationsAtPath(TransparentSheetPath)
                .OfType<Sprite>().FirstOrDefault(s => s.name == "Mat01_Ember_Common");
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var label = NewText("CountText", panel.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(48, 20),
                TextAnchor.MiddleLeft, 11, "재료 0개");

            var ui = panel.gameObject.AddComponent<MaterialCountUI>();
            SetRef(ui, "inventory", inventory);
            SetRef(ui, "label", label);
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
