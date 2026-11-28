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
    /// v3 정식 그림 연결 (현재 Scene):
    /// - Import 설정 (처음 한 번만): Sprite, Point, 압축 없음, PPU 32. FX_BossEnrage_Sheet(4칸 128×128)·FX_Breath_Sheet(5칸 160×120)는 프레임으로 자르기
    /// - 카드: 레몬 폭탄(그림 + 완성 카드), 연막 구르기(그림만, 완성 카드는 비워 둠)
    /// - 숲: ForestTileSpriteSet의 잠든 둥지(Nest_Dormant)
    /// - Scene: 스테이지 클리어 띠, 붉은 기운, 보스 이름 띠, 모드 선택 배경·제목·잠금, 엔딩 그림, 연타·필살 숨결 예고, 반사·무효 상태 아이콘
    /// - 새 오브젝트: DuelScreen/Panel/DragonSide/Dragon/EnrageFX(분노 이펙트), DuelScreen/Panel/BreathFX(숨결 이펙트)
    /// 이미 있으면 다시 만들지 않고, 사용자가 바꾼 연결·값은 덮어쓰지 않는다 (임시 그림이 연결돼 있거나 비어 있을 때만 바꿈).
    /// </summary>
    public static class ArtV3Installer
    {
        const string ImportMark = "dramon-import-v3-art";
        const string CardFolder = DramonAssetBuilder.SettingsFolder + "/Cards";
        const string Art = DramonAssetBuilder.ArtFolder;

        // Art 폴더 기준 경로 (확장자 없음)
        const string LemonBombArt = "Battle/Cards/Art/CardArt_LemonBomb";
        const string LemonBombCard = "Battle/Cards/Composed/Card_LemonBomb";
        const string SmokeRollArt = "Battle/Cards/Art/CardArt_SmokeRoll";
        const string MultiHitIcon = "Battle/Cards/Icons/Icon_Intent_MultiHit";
        const string FinisherIcon = "Battle/Cards/Icons/Icon_Intent_Finisher";
        const string ReflectIcon = "Battle/Cards/Icons/Icon_Status_Reflect";
        const string NegateIcon = "Battle/Cards/Icons/Icon_Status_Negate";
        const string NestDormant = "Board/Nest_Dormant";
        const string StageClearBanner = "UI/UI_StageClear_Banner";
        const string BossNameBand = "UI/UI_BossNameBand";
        const string TitleLogo = "UI/UI_Title_Logo";
        const string LockIcon = "UI/UI_Icon_Lock";
        const string ModeSelectBG = "UI/UI_ModeSelect_BG";
        const string EndingPicture = "UI/UI_Ending_Picture";
        const string BossAura = "Battle/FX/FX_BossAura";
        const string EnrageSheet = "Battle/FX/FX_BossEnrage_Sheet";
        const string BreathSheet = "Battle/FX/FX_Breath_Sheet";

        static readonly string[] SinglePaths =
        {
            LemonBombArt, LemonBombCard, SmokeRollArt, MultiHitIcon, FinisherIcon, ReflectIcon, NegateIcon, NestDormant,
            StageClearBanner, BossNameBand, TitleLogo, LockIcon, ModeSelectBG, EndingPicture, BossAura,
        };

        [MenuItem("Dramon/기능 설치 - v3 정식 그림 (현재 Scene)")]
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
                AssetDatabase.SaveAssets();
                Debug.Log("[Dramon] v3 정식 그림 연결 완료: " + scene.path);
            }
        }

        static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        public static bool Install()
        {
            var battler = Find<DragonBattlerView>();
            var boss = Find<BossFight>();
            var screen = Find<DuelScreenUI>();
            var duelPanel = screen != null ? screen.transform.Find("Panel") : null;
            if (battler == null || boss == null || duelPanel == null)
            {
                EditorUtility.DisplayDialog("Dramon", "Forest Scene이 열려 있고 v3 1~4단계가 설치돼 있어야 해요.", "확인");
                return false;
            }

            // ── Import 설정 ──────────────────────
            foreach (var p in SinglePaths) SetupImport(p, SpriteImportMode.Single);
            SetupImport(EnrageSheet, SpriteImportMode.Multiple);
            SetupImport(BreathSheet, SpriteImportMode.Multiple);
            Slice(EnrageSheet, "FX_BossEnrage", 128, 128, 4);
            Slice(BreathSheet, "FX_Breath", 160, 120, 5);

            // ── 카드 ────────────────────────────
            SetupCard("Card_LemonBomb", "CardArt_ThrownLemon", Load(LemonBombArt), Load(LemonBombCard));
            SetupCard("Card_SmokeRoll", "CardArt_DodgeRoll", Load(SmokeRollArt), null); // 완성 카드는 비워 둠 (기본 방어 틀 + 그림)

            // ── 숲: 잠든 둥지 ─────────────────────
            var tileSet = AssetDatabase.LoadAssetAtPath<ForestTileSpriteSet>(DramonAssetBuilder.TileSpriteSetPath);
            if (tileSet != null) { SetRefIfEmpty(tileSet, "nestDormant", Load(NestDormant)); EditorUtility.SetDirty(tileSet); }

            // ── 스테이지 클리어 띠 ─────────────────
            var clear = Find<StageClearUI>();
            if (clear != null)
            {
                SetImageIfEmpty(GetRef<Image>(clear, "banner"), Load(StageClearBanner));
                ClearNote(clear, "임시: 기본 창(UI_Window_9Slice)과 글자만 씀");
            }

            // ── 붉은 기운 ────────────────────────
            var aura = Find<BossAuraView>();
            if (aura != null)
            {
                var img = aura.GetComponent<Image>();
                if (img != null && (img.sprite == null || img.sprite.name == "Knob"))
                {
                    img.sprite = Load(BossAura);
                    img.preserveAspect = true;
                    EditorUtility.SetDirty(img);
                    // 기본 원을 붉게 칠하던 값이면 그림 색 그대로 + 조금 더 진하게
                    var so = new SerializedObject(aura);
                    var c = so.FindProperty("color");
                    if (Mathf.Abs(c.colorValue.g - 0.1f) < 0.01f && Mathf.Abs(c.colorValue.r - 0.9f) < 0.01f)
                    {
                        c.colorValue = Color.white;
                        SetFloatIf(so, "minAlpha", 0.15f, 0.5f);
                        SetFloatIf(so, "maxAlpha", 0.45f, 0.85f);
                        SetFloatIf(so, "enragedMinAlpha", 0.35f, 0.8f);
                        SetFloatIf(so, "enragedMaxAlpha", 0.8f, 1f);
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                ClearNote(aura, "임시: Unity 기본 원(Knob)");
            }

            // ── 보스 이름 띠 ─────────────────────
            var band = Find<BossNameBandUI>();
            if (band != null)
            {
                var bandGO = GetRef<GameObject>(band, "band");
                var img = bandGO != null ? bandGO.GetComponent<Image>() : null;
                if (img != null && (img.sprite == null || img.sprite.name == "UI_Window_9Slice"))
                {
                    img.sprite = Load(BossNameBand);
                    img.type = Image.Type.Simple;
                    img.color = Color.white;
                    EditorUtility.SetDirty(img);
                }
                ClearNote(band, "임시: 기본 창(UI_Window_9Slice)에 붉은 색만 입힘");
            }

            // ── 모드 선택 화면 ────────────────────
            var mode = Find<ModeSelectUI>();
            if (mode != null)
            {
                SetImageIfEmpty(GetRef<Image>(mode, "backgroundPicture"), Load(ModeSelectBG));
                SetImageIfEmpty(GetRef<Image>(mode, "titlePicture"), Load(TitleLogo));
                SetImageIfEmpty(GetRef<Image>(mode, "lockIcon"), Load(LockIcon));
                ClearNote(mode, "임시: 기본 창과 글자 버튼만 씀");
            }

            // ── 엔딩 ────────────────────────────
            var ending = Find<EndingUI>();
            if (ending != null)
            {
                SetImageIfEmpty(GetRef<Image>(ending, "picture"), Load(EndingPicture));
                ClearNote(ending, "임시: 기본 창과 글자만 씀");
            }

            // ── 행동 예고 아이콘 ───────────────────
            var intentUI = Find<DragonIntentUI>();
            if (intentUI != null)
            {
                SetRefIfEmpty(intentUI, "multiHitIcon", Load(MultiHitIcon));
                SetRefIfEmpty(intentUI, "finisherIcon", Load(FinisherIcon));
                SetRefIfEmpty(intentUI, "bossFight", boss);
            }

            // ── 반사·무효 상태 아이콘 (플레이어 줄) ───────
            var playerStats = Find<PlayerBattleStats>();
            foreach (var row in Object.FindObjectsByType<StatusIconsUI>(FindObjectsInactive.Include))
            {
                var src = GetRef<StatusEffects>(row, "source");
                if (src == null || src.gameObject.name != "PlayerStatus") continue;
                SetRefIfEmpty(row, "guardSource", playerStats);
                SetRefIfEmpty(row, "reflectIcon", Load(ReflectIcon));
                SetRefIfEmpty(row, "negateIcon", Load(NegateIcon));
            }

            // ── 분노 이펙트 (Dragon/EnrageFX) ───────
            var enrage = BuildEnrageFX(battler.transform);
            RefreshFrames(enrage, EnrageSheet, "FX_BossEnrage_");
            SetRefIfEmpty(boss, "enrageEffect", enrage);

            // ── 숨결 이펙트 (Panel/BreathFX) ────────
            var breath = BuildBreathFX(duelPanel);
            RefreshFrames(breath != null ? breath.GetComponent<UIFramePlayer>() : null, BreathSheet, "FX_Breath_");
            var executor = Find<DragonActionExecutor>();
            if (executor != null) SetRefIfEmpty(executor, "breathEffect", breath);
            return true;
        }

        // ── 오브젝트 ────────────────────────────
        static UIFramePlayer BuildEnrageFX(Transform dragon)
        {
            var existing = dragon.Find("EnrageFX");
            if (existing != null) return existing.GetComponent<UIFramePlayer>();
            var go = NewUI("EnrageFX", dragon);
            SetAnchored((RectTransform)go.transform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160, 160));
            var motion = dragon.Find("Motion");
            if (motion != null) go.transform.SetSiblingIndex(motion.GetSiblingIndex() + 1); // 드래곤 그림 앞
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.enabled = false;
            var player = go.AddComponent<UIFramePlayer>();
            SetRef(player, "image", img);
            SetFrames(player, LoadFrames(EnrageSheet), 10f);
            return player;
        }

        static BreathEffectView BuildBreathFX(Transform panel)
        {
            var existing = panel.Find("BreathFX");
            if (existing != null) return existing.GetComponent<BreathEffectView>();
            var go = NewUI("BreathFX", panel);
            var rt = (RectTransform)go.transform;
            // 드래곤(오른쪽 위)에서 검사(왼쪽 아래) 쪽으로: 화면 가운데쯤, 조금 기울임
            rt.anchorMin = rt.anchorMax = new Vector2(0.45f, 0.48f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(260, 195);
            rt.localEulerAngles = new Vector3(0f, 0f, 15f);
            var playerSide = panel.Find("PlayerSide");
            if (playerSide != null) go.transform.SetSiblingIndex(playerSide.GetSiblingIndex() + 1); // 드래곤·검사 앞, 창들 뒤
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.enabled = false;
            var player = go.AddComponent<UIFramePlayer>();
            SetRef(player, "image", img);
            SetFrames(player, LoadFrames(BreathSheet), 12f);
            var view = go.AddComponent<BreathEffectView>();
            SetRef(view, "player", player);
            return view;
        }

        /// <summary>
        /// 프레임이 비었거나 이 시트의 예전 조각(자동으로 잘린 것)을 가리키면 지금 칸들로 다시 넣는다.
        /// 다른 그림으로 바꿔 둔 프레임은 그대로.
        /// </summary>
        static void RefreshFrames(UIFramePlayer player, string sheetPath, string framePrefix)
        {
            if (player == null) return;
            var expected = LoadFrames(sheetPath)
                .Where(f => f.name.Length > framePrefix.Length && f.name.StartsWith(framePrefix) && f.name.Substring(framePrefix.Length).All(char.IsDigit))
                .ToArray();
            if (expected.Length == 0) return;
            var so = new SerializedObject(player);
            var arr = so.FindProperty("frames");
            string sheetFile = $"{Art}/{sheetPath}.png";
            bool ok = arr.arraySize == expected.Length;
            bool ours = true;
            for (int i = 0; i < arr.arraySize; i++)
            {
                var f = arr.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                if (f == null) { ok = false; continue; }
                if (AssetDatabase.GetAssetPath(f) != sheetFile) ours = false;
                else if (i >= expected.Length || f != expected[i]) ok = false;
            }
            if (ok || !ours) return;
            arr.arraySize = expected.Length;
            for (int i = 0; i < expected.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = expected[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFrames(UIFramePlayer player, Sprite[] frames, float fps)
        {
            var so = new SerializedObject(player);
            var arr = so.FindProperty("frames");
            arr.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            so.FindProperty("fps").floatValue = fps;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── 카드 ────────────────────────────────
        /// <summary>임시 그림이 연결돼 있으면 정식 그림으로, 완성 카드는 비어 있을 때만</summary>
        static void SetupCard(string asset, string tempArtName, Sprite art, Sprite composed)
        {
            var card = AssetDatabase.LoadAssetAtPath<CardData>($"{CardFolder}/{asset}.asset");
            if (card == null) { Debug.LogWarning($"[Dramon] {asset} 카드 데이터가 없어요 (v3 1단계 설치 필요)"); return; }
            var so = new SerializedObject(card);
            var artProp = so.FindProperty("art");
            var current = artProp.objectReferenceValue as Sprite;
            if (art != null && (current == null || current.name == tempArtName))
            {
                artProp.objectReferenceValue = art;
                so.FindProperty("tempAssetNote").stringValue = "";
            }
            var comp = so.FindProperty("composedImage");
            if (composed != null && comp.objectReferenceValue == null) comp.objectReferenceValue = composed;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(card);
        }

        // ── Import ──────────────────────────────
        static void SetupImport(string artPath, SpriteImportMode mode)
        {
            var importer = AssetImporter.GetAtPath($"{Art}/{artPath}.png") as TextureImporter;
            if (importer == null) { Debug.LogWarning($"[Dramon] 그림이 없어요: {artPath}.png"); return; }
            if (importer.userData == ImportMark) return; // 사용자가 바꾼 Import 설정은 그대로
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = mode;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = 32;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.userData = ImportMark;
            importer.SaveAndReimport();
        }

        /// <summary>
        /// 가로 한 줄 시트를 count칸으로 자른다 (처음 한 번만). 이름 = prefix_0, prefix_1 …
        /// Multiple로 바꿀 때 Unity가 자동으로 만든 조각(파일이름_0 …)은 이 칸들로 바꾼다.
        /// </summary>
        static void Slice(string artPath, string prefix, int w, int h, int count)
        {
            var importer = AssetImporter.GetAtPath($"{Art}/{artPath}.png") as TextureImporter;
            if (importer == null) return;
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            if (provider.GetSpriteRects().Any(r => r.name == prefix + "_0")) return; // 이미 잘라 둠
            var rects = new List<SpriteRect>();
            for (int i = 0; i < count; i++)
                rects.Add(new SpriteRect
                {
                    name = $"{prefix}_{i}",
                    rect = new Rect(i * w, 0, w, h),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = GUID.Generate(),
                });
            provider.SetSpriteRects(rects.ToArray());
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null) names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        static Sprite Load(string artPath) => DramonAssetBuilder.LoadSprite(artPath);

        static Sprite[] LoadFrames(string artPath) =>
            AssetDatabase.LoadAllAssetsAtPath($"{Art}/{artPath}.png").OfType<Sprite>()
                .OrderBy(s => int.TryParse(s.name.Substring(s.name.LastIndexOf('_') + 1), out var n) ? n : 0).ToArray();

        // ── 도우미 ──────────────────────────────
        static T GetRef<T>(Object target, string property) where T : Object
        {
            if (target == null) return null;
            var prop = new SerializedObject(target).FindProperty(property);
            return prop != null ? prop.objectReferenceValue as T : null;
        }

        static void SetImageIfEmpty(Image img, Sprite sprite)
        {
            if (img == null || sprite == null || img.sprite != null) return;
            img.sprite = sprite;
            EditorUtility.SetDirty(img);
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

        static void SetFloatIf(SerializedObject so, string property, float expected, float value)
        {
            var p = so.FindProperty(property);
            if (p != null && Mathf.Abs(p.floatValue - expected) < 0.001f) p.floatValue = value;
        }

        /// <summary>임시 에셋 메모가 설치 메뉴가 넣은 문구 그대로면 비운다 (직접 고친 메모는 그대로)</summary>
        static void ClearNote(Object target, string startsWith)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty("tempAssetNote");
            if (p == null || !p.stringValue.StartsWith(startsWith)) return;
            p.stringValue = "";
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
