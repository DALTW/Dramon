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
    /// 드래곤 비행 모션(프레임 애니메이션)을 지금 열린 Forest Scene에 추가한다.
    /// - 에셋: 드래곤별 MotionSheet Import 설정 (Sprite Multiple, Point, 압축 없음, Max 2048, PPU 32),
    ///   4열 × 3행으로 자르기 (Fly_0~3 / Attack_0~3 / Hurt_0~1·Defeat_0~1), Pivot = 발밑 가운데 (필요한 칸만 미세 조정)
    /// - DragonSpecies 10종: 모션 프레임·fps·흔들림 기본값, 결투 그림 3장을 Fly_0 / Attack_2 / Hurt_0으로
    /// - HUD Canvas/DuelScreen/Panel/DragonSide/Dragon 아래 모션 전용 자식 Motion/Sprite + DragonFrameAnimator, DragonHoverMotion
    /// 이미 있는 것은 다시 만들지 않고, 사용자가 바꾼 연결·값(Import 설정, 자른 영역 포함)도 덮어쓰지 않는다.
    /// </summary>
    public static class DragonMotionInstaller
    {
        const string DragonArtFolder = DramonAssetBuilder.ArtFolder + "/Dragon/Types";
        const string DragonDataFolder = DramonAssetBuilder.SettingsFolder + "/Dragons";
        const string ImportSetupMark = "dramon-import-setup";
        const int CellWidth = 344, CellHeight = 256, Columns = 4, Rows = 3;

        static readonly string[] Keys = { "Ember", "Forest", "Storm", "Shadow", "Gold", "Frost", "Toxic", "Tide", "Stone", "Crystal" };

        // 칸 이름 (행마다 왼쪽 → 오른쪽)
        static readonly string[][] CellNames =
        {
            new[] { "Fly_0", "Fly_1", "Fly_2", "Fly_3" },
            new[] { "Attack_0", "Attack_1", "Attack_2", "Attack_3" },
            new[] { "Hurt_0", "Hurt_1", "Defeat_0", "Defeat_1" },
        };

        // 드래곤별 비행 fps · 흔들림 (진폭 픽셀, 주기 초)
        static readonly float[] FlyFps = { 8, 6, 12, 6, 7, 6, 7, 8, 5, 8 };
        static readonly float[] HoverAmplitude = { 6, 3, 2, 4, 6, 3, 4, 5, 2, 4 };
        static readonly float[] HoverPeriod = { 1.0f, 1.6f, 0.4f, 1.8f, 1.4f, 1.4f, 1.2f, 1.2f, 1.6f, 1.3f };

        // 화면에서 드래곤 크기가 예전(208×144 그림)과 비슷하게 보이도록 하는 그림 크기 배율 (시트를 재서 정함)
        const float DefaultSizeMultiplier = 1f;

        // Pivot을 따로 정한 칸 (칸 안 픽셀 기준 Pivot, 왼쪽 아래 = 0,0)
        // - 비행: 프레임끼리 몸 위치가 어긋나서 맞춘 칸
        // - 패배: 쓰러진 몸의 가운데·맨 아래를 Pivot으로 해서 둥지 안(발판 위치)에 눕게 한 칸
        static readonly Dictionary<string, Vector2> PivotOverrides = new Dictionary<string, Vector2>
        {
            { "Dragon01_Ember_Fly_2", new Vector2(178, 0) },
            { "Dragon01_Ember_Fly_3", new Vector2(178, 0) },
            { "Dragon02_Forest_Fly_1", new Vector2(168, 1) },
            { "Dragon02_Forest_Fly_2", new Vector2(169, 1) },
            { "Dragon02_Forest_Fly_3", new Vector2(165, 1) },
            { "Dragon03_Storm_Fly_1", new Vector2(176, 0) },
            { "Dragon04_Shadow_Fly_1", new Vector2(176, -11) },
            { "Dragon04_Shadow_Fly_2", new Vector2(178, -4) },
            { "Dragon04_Shadow_Fly_3", new Vector2(181, 3) },
            { "Dragon05_Gold_Fly_2", new Vector2(175, 0) },
            { "Dragon05_Gold_Fly_3", new Vector2(178, 0) },
            { "Dragon07_Toxic_Fly_2", new Vector2(171, 7) },
            { "Dragon08_Tide_Fly_1", new Vector2(171, -5) },
            { "Dragon08_Tide_Fly_2", new Vector2(181, -8) },
            { "Dragon08_Tide_Fly_3", new Vector2(179, -14) },
            { "Dragon09_Stone_Fly_2", new Vector2(177, -12) },
            { "Dragon09_Stone_Fly_3", new Vector2(175, -2) },
            { "Dragon01_Ember_Defeat_0", new Vector2(174, 14) },
            { "Dragon01_Ember_Defeat_1", new Vector2(180, 25) },
            { "Dragon02_Forest_Defeat_0", new Vector2(164, 21) },
            { "Dragon02_Forest_Defeat_1", new Vector2(174, 11) },
            { "Dragon03_Storm_Defeat_0", new Vector2(177, 23) },
            { "Dragon03_Storm_Defeat_1", new Vector2(176, 40) },
            { "Dragon04_Shadow_Defeat_0", new Vector2(180, 14) },
            { "Dragon04_Shadow_Defeat_1", new Vector2(178, 37) },
            { "Dragon05_Gold_Defeat_0", new Vector2(162, 18) },
            { "Dragon05_Gold_Defeat_1", new Vector2(177, 24) },
            { "Dragon06_Frost_Defeat_0", new Vector2(176, 21) },
            { "Dragon06_Frost_Defeat_1", new Vector2(172, 15) },
            { "Dragon07_Toxic_Defeat_0", new Vector2(174, 21) },
            { "Dragon07_Toxic_Defeat_1", new Vector2(180, 14) },
            { "Dragon08_Tide_Defeat_0", new Vector2(164, 11) },
            { "Dragon08_Tide_Defeat_1", new Vector2(168, 8) },
            { "Dragon09_Stone_Defeat_0", new Vector2(178, 8) },
            { "Dragon09_Stone_Defeat_1", new Vector2(174, 9) },
            { "Dragon10_Crystal_Defeat_0", new Vector2(174, 6) },
            { "Dragon10_Crystal_Defeat_1", new Vector2(174, 20) },
        };

        // 드래곤별 결투 그림 크기 배율 (둥지 안에 쓰러진 모습이 들어가고, 날 때 화면 위로 잘리지 않게)
        static readonly float[] BattleSizeMultipliers = { 0.81f, 0.78f, 0.76f, 0.80f, 0.74f, 0.91f, 0.88f, 0.84f, 0.90f, 0.94f };

        [MenuItem("Dramon/기능 설치 - 드래곤 비행 모션 (현재 Scene)")]
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
                Debug.Log("[Dramon] 드래곤 비행 모션 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            var battler = Object.FindAnyObjectByType<DragonBattlerView>(FindObjectsInactive.Include);
            var session = Object.FindAnyObjectByType<DuelSession>(FindObjectsInactive.Include);
            if (battler == null || session == null)
            {
                EditorUtility.DisplayDialog("Dramon",
                    "Forest Scene이 열려 있어야 하고, 먼저 '기능 설치 - 드래곤 등장·결투 화면'이 설치돼 있어야 해요.", "확인");
                return false;
            }

            // ── 에셋 ─────────────────────────────
            AssetDatabase.Refresh();
            for (int i = 0; i < Keys.Length; i++)
            {
                string path = SheetPath(i);
                if (AssetImporter.GetAtPath(path) == null) { Debug.LogWarning($"[Dramon] 모션 시트가 없어요: {path}"); continue; }
                SetupImport(path);
                Slice(i, path);
                ApplyPivotOverrides(path);
                UpdateSpecies(i, path);
            }

            // ── 결투 화면: 모션 전용 자식 ──────────
            BuildMotionChildren(battler);
            SetRefIfEmpty(session, "battler", battler);

            AssetDatabase.SaveAssets();
            return true;
        }

        static string Prefix(int i) => $"Dragon{i + 1:00}_{Keys[i]}";
        static string SheetPath(int i) => $"{DragonArtFolder}/{i + 1:00}_{Keys[i]}/{Prefix(i)}_MotionSheet.png";

        // ── Import 설정 (처음 한 번만) ────────────
        static void SetupImport(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.userData == ImportSetupMark) return; // 사용자가 바꾼 Import 설정은 그대로
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = 32;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.userData = ImportSetupMark;
            importer.SaveAndReimport();
        }

        // ── 4×3 자르기 (처음 한 번만) ─────────────
        static void Slice(int i, string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            string prefix = Prefix(i);
            if (provider.GetSpriteRects().Any(r => r.name.StartsWith(prefix + "_Fly"))) return; // 이미 잘라 둠

            var rects = new List<SpriteRect>();
            for (int row = 0; row < Rows; row++)
            for (int col = 0; col < Columns; col++)
            {
                string name = $"{prefix}_{CellNames[row][col]}";
                var rect = new SpriteRect
                {
                    name = name,
                    rect = new Rect(col * CellWidth, (Rows - 1 - row) * CellHeight, CellWidth, CellHeight), // 1행 = 맨 위
                    alignment = SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0f),
                    spriteID = GUID.Generate(),
                };
                if (PivotOverrides.TryGetValue(name, out var px))
                {
                    rect.alignment = SpriteAlignment.Custom;
                    rect.pivot = new Vector2(px.x / CellWidth, px.y / CellHeight);
                }
                rects.Add(rect);
            }
            provider.SetSpriteRects(rects.ToArray());
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null)
                names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        // ── 이미 자른 칸에 Pivot 보정 넣기 (아직 기본 발밑 가운데인 칸만) ─
        static void ApplyPivotOverrides(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var rects = provider.GetSpriteRects();
            bool changed = false;
            foreach (var rect in rects)
            {
                if (rect.alignment != SpriteAlignment.BottomCenter) continue; // 사용자가 바꾼 Pivot은 그대로
                if (!PivotOverrides.TryGetValue(rect.name, out var px)) continue;
                rect.alignment = SpriteAlignment.Custom;
                rect.pivot = new Vector2(px.x / CellWidth, px.y / CellHeight);
                changed = true;
            }
            if (!changed) return;
            provider.SetSpriteRects(rects);
            provider.Apply();
            importer.SaveAndReimport();
        }

        // ── DragonSpecies: 모션 프레임 (비어 있을 때만) ─
        static void UpdateSpecies(int i, string sheetPath)
        {
            string path = $"{DragonDataFolder}/{Prefix(i)}.asset";
            var species = AssetDatabase.LoadAssetAtPath<DragonSpecies>(path);
            if (species == null) { Debug.LogWarning($"[Dramon] 드래곤 데이터를 찾을 수 없어요: {path}"); return; }
            if (species.HasFlyFrames) return; // 이미 연결됨 — 사용자가 바꾼 값은 그대로

            var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath).OfType<Sprite>()
                .GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First());
            Sprite Get(string cell) => sprites.TryGetValue($"{Prefix(i)}_{cell}", out var s) ? s : null;

            var so = new SerializedObject(species);
            SetFrames(so, "flyFrames", Get("Fly_0"), Get("Fly_1"), Get("Fly_2"), Get("Fly_3"));
            SetFrames(so, "attackFrames", Get("Attack_0"), Get("Attack_1"), Get("Attack_2"), Get("Attack_3"));
            SetFrames(so, "hurtFrames", Get("Hurt_0"), Get("Hurt_1"));
            SetFrames(so, "defeatFrames", Get("Defeat_0"), Get("Defeat_1"));
            so.FindProperty("flyFps").floatValue = FlyFps[i];
            so.FindProperty("hoverAmplitude").floatValue = HoverAmplitude[i];
            so.FindProperty("hoverPeriod").floatValue = HoverPeriod[i];
            so.FindProperty("battleSizeMultiplier").floatValue = BattleSizeMultipliers[i];
            // 기존 결투 그림 3장도 새 시트의 칸으로
            if (Get("Fly_0") != null) so.FindProperty("battleIdle").objectReferenceValue = Get("Fly_0");
            if (Get("Attack_2") != null) so.FindProperty("battleAttack").objectReferenceValue = Get("Attack_2");
            if (Get("Hurt_0") != null) so.FindProperty("battleHurt").objectReferenceValue = Get("Hurt_0");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(species);
        }

        static void SetFrames(SerializedObject so, string property, params Sprite[] frames)
        {
            var arr = so.FindProperty(property);
            arr.arraySize = frames.Length;
            for (int k = 0; k < frames.Length; k++)
                arr.GetArrayElementAtIndex(k).objectReferenceValue = frames[k];
        }

        // ── Dragon/Motion/Sprite (처음 한 번만) ──────
        static void BuildMotionChildren(DragonBattlerView battler)
        {
            var dragon = battler.gameObject;
            if (dragon.transform.Find("Motion") != null)
            {
                // 이미 있음: 비어 있는 연결만 채움
                var hoverExisting = dragon.GetComponent<DragonHoverMotion>();
                SetRefIfEmpty(battler, "hover", hoverExisting);
                SetRefIfEmpty(battler, "frameAnimator", dragon.GetComponentInChildren<DragonFrameAnimator>(true));
                return;
            }

            var oldImage = dragon.GetComponent<Image>();

            // 위아래 흔들림만 받는 모션 전용 자식 (Dragon 영역에 꽉 맞춤, 기준점 = 발밑 가운데)
            var motion = NewUI("Motion", dragon.transform);
            StretchBottomCenter((RectTransform)motion.transform);

            // 실제 그림 (프레임마다 Pivot 맞추기로 위치가 조금씩 바뀜)
            var spriteGO = NewUI("Sprite", motion.transform);
            StretchBottomCenter((RectTransform)spriteGO.transform);
            var image = spriteGO.AddComponent<Image>();
            image.sprite = oldImage != null ? oldImage.sprite : null;
            image.color = oldImage != null ? oldImage.color : Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            var animator = spriteGO.AddComponent<DragonFrameAnimator>();
            SetRef(animator, "image", image);

            var hover = GetOrAdd<DragonHoverMotion>(dragon);
            SetRef(hover, "target", motion.transform);

            // Dragon 자신의 Image는 꺼 두고(지우지 않음), 그림은 Motion/Sprite가 그림
            if (oldImage != null) oldImage.enabled = false;

            SetRef(battler, "image", image);
            SetRef(battler, "frameAnimator", animator);
            SetRef(battler, "hover", hover);
            var so = new SerializedObject(battler);
            so.FindProperty("sizeMultiplier").floatValue = DefaultSizeMultiplier;
            so.ApplyModifiedPropertiesWithoutUndo();
            spriteGO.transform.localScale = new Vector3(DefaultSizeMultiplier, DefaultSizeMultiplier, 1f);
        }

        static void StretchBottomCenter(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0f);
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
