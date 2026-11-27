using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;
using static Dramon.EditorTools.DramonEditorUtil;

namespace Dramon.EditorTools
{
    /// <summary>
    /// 결투 화면 플레이어를 검사(레몬 검사)로 바꾸고 포켓몬 팬게임 구도로 세운다 (지금 열린 Forest Scene).
    /// - 에셋: 검사 시트 A v2(투명 사본)·B Import 설정 + 4×4 자르기 (이름·발밑 Pivot), HeroSpriteSet, DuelLayoutSettings
    /// - DragonSpecies 10종: 플레이어 피격 반응 기본값 (비어 있을 때만)
    /// - DuelScreen/Panel/PlayerSide/Hero (HeroBattlerView), 예전 Player 그림은 꺼 둠, 빈 플레이어 체력 창(꺼 둠)
    /// - 처음 설치할 때 한 번 "포켓몬 구도 적용"
    /// 이미 있는 에셋·오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결·값(Import 설정, 자른 영역 포함)도 덮어쓰지 않는다.
    /// </summary>
    public static class HeroInstaller
    {
        const string HeroArtPath = "Player/Hero/Source";
        const string SheetAPath = DramonAssetBuilder.ArtFolder + "/" + HeroArtPath + "/Hero_LemonSwordsman_Back_SheetA_v2.png";
        const string SheetBPath = DramonAssetBuilder.ArtFolder + "/" + HeroArtPath + "/Hero_LemonSwordsman_Back_SheetB.png";
        const string HeroSettingsFolder = DramonAssetBuilder.SettingsFolder + "/Hero";
        const string SpriteSetPath = HeroSettingsFolder + "/HeroSpriteSet.asset";
        const string DragonDataFolder = DramonAssetBuilder.SettingsFolder + "/Dragons";
        const string ImportSetupMark = "dramon-import-setup";
        const int CellSize = 256;

        static readonly string[] DragonKeys = { "Ember", "Forest", "Storm", "Shadow", "Gold", "Frost", "Toxic", "Tide", "Stone", "Crystal" };
        static readonly HeroReactionType[] DefaultReactions =
        {
            HeroReactionType.Burned, HeroReactionType.Poisoned, HeroReactionType.Shocked, HeroReactionType.Fear, HeroReactionType.Burned,
            HeroReactionType.Frozen, HeroReactionType.Poisoned, HeroReactionType.Drenched, HeroReactionType.StoneHit, HeroReactionType.CrystalShards,
        };

        /// <summary>시트 한 칸: 이름, 열·행(위에서부터), 발밑 Pivot(칸 왼쪽 아래 기준 픽셀)</summary>
        struct Cell
        {
            public string name; public int col, row; public float px, py;
            public Cell(string name, int col, int row, float px, float py) { this.name = name; this.col = col; this.row = row; this.px = px; this.py = py; }
        }

        // 칸마다 발 위치를 재서 정한 Pivot: 가로 = 두 발 사이 가운데, 세로 = 신발 밑 (그림은 옮기지 않음)
        // 직접 고른 칸: Roll_0(공 모양 → 몸 아래 가운데), Defeated(누운 몸 가운데), React_Fear(연기에 발이 가려 옆 칸 높이), LowHealth(무릎 꿇은 몸 가운데)
        static readonly Cell[] SheetA =
        {
            new Cell("Hero_Idle_0", 0, 0, 128f, 2f),
            new Cell("Hero_Idle_1", 1, 0, 128f, 2f),
            new Cell("Hero_Idle_2", 2, 0, 131f, 2f),
            new Cell("Hero_Idle_3", 3, 0, 131.5f, 2f),
            new Cell("Hero_Slash_0", 0, 1, 139.5f, 2f),
            new Cell("Hero_Slash_1", 1, 1, 107.5f, 2f),
            new Cell("Hero_Slash_2", 2, 1, 109.5f, 2f),
            new Cell("Hero_Slash_3", 3, 1, 136.5f, 2f),
            new Cell("Hero_Block_0", 0, 2, 124.5f, 2f),
            new Cell("Hero_Block_1", 1, 2, 126f, 2f),
            new Cell("Hero_Roll_0", 2, 2, 160f, 2f),
            new Cell("Hero_Roll_1", 3, 2, 131f, 2f),
            new Cell("Hero_LemonThrow_0", 0, 3, 131f, 2f),
            new Cell("Hero_LemonThrow_1", 1, 3, 107.5f, 2f),
            new Cell("Hero_LemonSqueeze", 2, 3, 123.5f, 2f),
            new Cell("Hero_Herb", 3, 3, 120.5f, 2f),
        };

        static readonly Cell[] SheetB =
        {
            new Cell("Hero_Hit", 0, 0, 139.5f, 16f),
            new Cell("Hero_HitRecover", 1, 0, 116f, 16f),
            new Cell("Hero_Victory", 2, 0, 124.5f, 16f),
            new Cell("Hero_Defeated", 3, 0, 115f, 19f),
            new Cell("Hero_React_Burned", 0, 1, 135f, 16f),
            new Cell("Hero_React_Shocked", 1, 1, 127.5f, 16f),
            new Cell("Hero_React_Frozen", 2, 1, 121f, 36f),
            new Cell("Hero_React_Poisoned", 3, 1, 102.5f, 16f),
            new Cell("Hero_React_Drenched", 0, 2, 122.5f, 27f),
            new Cell("Hero_React_StoneHit", 1, 2, 105.5f, 26f),
            new Cell("Hero_React_Fear", 2, 2, 122f, 27f),
            new Cell("Hero_React_CrystalShards", 3, 2, 117f, 26f),
            new Cell("Hero_Enter_0", 0, 3, 119f, 33f),
            new Cell("Hero_Enter_1", 1, 3, 127f, 32f),
            new Cell("Hero_DrawCard", 2, 3, 111.5f, 28f),
            new Cell("Hero_LowHealth", 3, 3, 112f, 28f),
        };

        [MenuItem("Dramon/기능 설치 - 결투 검사·포켓몬 구도 (현재 Scene)")]
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
                Debug.Log("[Dramon] 결투 검사·포켓몬 구도 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            var phase = Object.FindAnyObjectByType<GamePhaseController>(FindObjectsInactive.Include);
            var screen = Object.FindAnyObjectByType<DuelScreenUI>(FindObjectsInactive.Include);
            var session = Object.FindAnyObjectByType<DuelSession>(FindObjectsInactive.Include);
            var intro = Object.FindAnyObjectByType<EncounterIntroSequence>(FindObjectsInactive.Include);
            var panel = screen != null ? screen.transform.Find("Panel") : null;
            var playerSide = panel != null ? panel.Find("PlayerSide") as RectTransform : null;
            if (phase == null || session == null || intro == null || playerSide == null)
            {
                EditorUtility.DisplayDialog("Dramon",
                    "Forest Scene이 열려 있어야 하고, 먼저 '기능 설치 - 드래곤 등장·결투 화면'이 설치돼 있어야 해요.", "확인");
                return false;
            }

            // ── 에셋 ─────────────────────────────
            AssetDatabase.Refresh();
            SetupSheet(SheetAPath, SheetA);
            SetupSheet(SheetBPath, SheetB);
            var spriteSet = EnsureSpriteSet();
            var layout = EnsureLayoutSettings();
            UpdateSpeciesReactions();

            // ── Scene ────────────────────────────
            bool firstInstall = playerSide.Find("Hero") == null;
            var hero = firstInstall ? BuildHero(playerSide, spriteSet, phase) : playerSide.Find("Hero").GetComponent<HeroBattlerView>();
            if (hero != null)
            {
                SetRefIfEmpty(hero, "spriteSet", spriteSet);
                SetRefIfEmpty(hero, "phaseController", phase);
            }
            if (panel.Find("PlayerHealthWindow") == null) BuildPlayerHealthWindow(panel);

            SetRefIfEmpty(intro, "hero", hero);
            SetRefIfEmpty(session, "hero", hero);

            if (firstInstall)
            {
                // 드래곤 테스트 단축키(1·2·3)와 검사 테스트 단축키(1~4)가 겹치지 않게 드래곤 쪽은 꺼 둠 (Inspector에서 다시 켤 수 있음)
                var dragon = Object.FindAnyObjectByType<DragonBattlerView>(FindObjectsInactive.Include);
                if (dragon != null)
                {
                    var so = new SerializedObject(dragon);
                    so.FindProperty("testHotkeys").boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                DuelLayoutApplier.Apply(layout); // 처음 한 번만 포켓몬 구도
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        // ── 시트 Import + 4×4 자르기 (처음 한 번만) ──
        static void SetupSheet(string path, Cell[] cells)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning($"[Dramon] 그림을 찾을 수 없어요: {path}"); return; }
            if (importer.userData == ImportSetupMark) return; // 이미 설정함 — 사용자가 바꾼 Import 설정·자른 영역은 그대로

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = 32;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            var rects = cells.Select(c => new SpriteRect
            {
                name = c.name,
                rect = new Rect(c.col * CellSize, height - (c.row + 1) * CellSize, CellSize, CellSize),
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(c.px / CellSize, c.py / CellSize),
                spriteID = GUID.Generate(),
            }).ToArray();
            provider.SetSpriteRects(rects); // 예전 자동 자르기 조각은 모두 지우고 16칸으로
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null)
                names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();

            importer.userData = ImportSetupMark;
            importer.SaveAndReimport();
        }

        static Dictionary<string, Sprite> LoadSprites()
        {
            var dict = new Dictionary<string, Sprite>();
            foreach (var path in new[] { SheetAPath, SheetBPath })
                foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                    if (o is Sprite s && !dict.ContainsKey(s.name)) dict[s.name] = s;
            return dict;
        }

        // ── 데이터 ───────────────────────────────
        static HeroSpriteSet EnsureSpriteSet()
        {
            DramonAssetBuilder.EnsureFolder(HeroSettingsFolder);
            var set = AssetDatabase.LoadAssetAtPath<HeroSpriteSet>(SpriteSetPath);
            if (set != null) return set;

            set = ScriptableObject.CreateInstance<HeroSpriteSet>();
            AssetDatabase.CreateAsset(set, SpriteSetPath);
            var sprites = LoadSprites();
            var so = new SerializedObject(set);
            void Fill(string clip, float fps, params string[] frameNames)
            {
                var p = so.FindProperty(clip);
                var frames = p.FindPropertyRelative("frames");
                frames.arraySize = frameNames.Length;
                for (int i = 0; i < frameNames.Length; i++)
                {
                    sprites.TryGetValue(frameNames[i], out var s);
                    if (s == null) Debug.LogWarning($"[Dramon] 스프라이트를 찾을 수 없어요: {frameNames[i]}");
                    frames.GetArrayElementAtIndex(i).objectReferenceValue = s;
                }
                p.FindPropertyRelative("fps").floatValue = fps;
            }
            Fill("idle", 3f, "Hero_Idle_0", "Hero_Idle_3");
            Fill("lowHealth", 2f, "Hero_LowHealth");
            Fill("enter", 6f, "Hero_Enter_0", "Hero_Enter_1");
            Fill("slash", 10f, "Hero_Slash_0", "Hero_Slash_1", "Hero_Slash_2", "Hero_Slash_3");
            Fill("block", 5f, "Hero_Block_0", "Hero_Block_1");
            Fill("roll", 5f, "Hero_Roll_0", "Hero_Roll_1");
            Fill("lemonThrow", 5f, "Hero_LemonThrow_0", "Hero_LemonThrow_1");
            Fill("lemonSqueeze", 2f, "Hero_LemonSqueeze");
            Fill("herb", 2f, "Hero_Herb");
            Fill("drawCard", 2f, "Hero_DrawCard");
            Fill("hit", 4f, "Hero_Hit");
            Fill("hitRecover", 3f, "Hero_HitRecover");
            Fill("victory", 2f, "Hero_Victory");
            Fill("defeated", 2f, "Hero_Defeated");
            Fill("burned", 2f, "Hero_React_Burned");
            Fill("shocked", 2f, "Hero_React_Shocked");
            Fill("frozen", 2f, "Hero_React_Frozen");
            Fill("poisoned", 2f, "Hero_React_Poisoned");
            Fill("drenched", 2f, "Hero_React_Drenched");
            Fill("stoneHit", 2f, "Hero_React_StoneHit");
            Fill("fear", 2f, "Hero_React_Fear");
            Fill("crystalShards", 2f, "Hero_React_CrystalShards");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            return set;
        }

        static DuelLayoutSettings EnsureLayoutSettings()
        {
            var layout = AssetDatabase.LoadAssetAtPath<DuelLayoutSettings>(DuelLayoutApplier.SettingsPath);
            if (layout != null) return layout;
            layout = ScriptableObject.CreateInstance<DuelLayoutSettings>();
            AssetDatabase.CreateAsset(layout, DuelLayoutApplier.SettingsPath);
            return layout;
        }

        /// <summary>드래곤 10종의 플레이어 피격 반응 (없음일 때만 기본값)</summary>
        static void UpdateSpeciesReactions()
        {
            for (int i = 0; i < DragonKeys.Length; i++)
            {
                string path = $"{DragonDataFolder}/Dragon{i + 1:00}_{DragonKeys[i]}.asset";
                var species = AssetDatabase.LoadAssetAtPath<DragonSpecies>(path);
                if (species == null) { Debug.LogWarning($"[Dramon] 드래곤 데이터를 찾을 수 없어요: {path}"); continue; }
                var so = new SerializedObject(species);
                var p = so.FindProperty("playerHitReaction");
                if (p.enumValueIndex != (int)HeroReactionType.None) continue;
                p.enumValueIndex = (int)DefaultReactions[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(species);
            }
        }

        // ── Scene ────────────────────────────────
        static HeroBattlerView BuildHero(RectTransform playerSide, HeroSpriteSet spriteSet, GamePhaseController phase)
        {
            // 예전 뒷모습 그림은 지우지 않고 꺼 둠
            var oldPlayer = playerSide.Find("Player");
            if (oldPlayer != null) oldPlayer.gameObject.SetActive(false);

            var heroGO = NewUI("Hero", playerSide);
            StretchBottom((RectTransform)heroGO.transform);
            var motion = NewUI("Motion", heroGO.transform);
            StretchBottom((RectTransform)motion.transform);
            var spriteGO = NewUI("Sprite", motion.transform);
            StretchBottom((RectTransform)spriteGO.transform);

            var image = spriteGO.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            var idle = spriteSet != null ? spriteSet.Idle.Valid() : null;
            image.sprite = idle != null ? idle[0] : null; // Play 전에도 Scene에서 보이게
            var animator = spriteGO.AddComponent<DragonFrameAnimator>();
            SetRef(animator, "image", image);

            var breathing = heroGO.AddComponent<DragonHoverMotion>();
            SetRef(breathing, "target", motion.transform);

            var hero = heroGO.AddComponent<HeroBattlerView>();
            SetRef(hero, "spriteSet", spriteSet);
            SetRef(hero, "frameAnimator", animator);
            SetRef(hero, "breathing", breathing);
            SetRef(hero, "phaseController", phase);
            return hero;
        }

        static void BuildPlayerHealthWindow(Transform panel)
        {
            var window = NewWindow("PlayerHealthWindow", panel);
            window.raycastTarget = false;
            window.pixelsPerUnitMultiplier = 2f;
            SetAnchored(window.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-16, 76), new Vector2(160, 40));
            var dragonHealth = panel.Find("HealthWindow");
            if (dragonHealth != null) window.transform.SetSiblingIndex(dragonHealth.GetSiblingIndex() + 1);
            window.gameObject.SetActive(false); // 빈 틀 (카드 결투 단계에서 사용)
        }

        static void StretchBottom(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
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
