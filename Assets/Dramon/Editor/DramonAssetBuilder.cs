using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon.EditorTools
{
    /// <summary>
    /// Dramon용 ScriptableObject·Prefab을 만든다. 이미 있으면 건드리지 않는다.
    /// Art 폴더의 임포트 설정·파일 이름은 절대 바꾸지 않고 읽기만 한다.
    /// </summary>
    public static class DramonAssetBuilder
    {
        public const string Root = "Assets/Dramon";
        public const string ArtFolder = Root + "/Art";
        public const string PrefabFolder = Root + "/Prefabs";
        public const string SettingsFolder = Root + "/Settings";

        public const string TileSpriteSetPath = SettingsFolder + "/ForestTileSpriteSet.asset";
        public const string TilePrefabPath = PrefabFolder + "/Board/ForestTile.prefab";
        public const string HudButtonPrefabPath = PrefabFolder + "/UI/HudButton.prefab";

        public static readonly Color TextBrown = new Color32(0x3B, 0x24, 0x14, 0xFF);

        public static Font UIFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static ForestTileSpriteSet EnsureTileSpriteSet()
        {
            EnsureFolder(SettingsFolder);
            var set = AssetDatabase.LoadAssetAtPath<ForestTileSpriteSet>(TileSpriteSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ForestTileSpriteSet>();
                AssetDatabase.CreateAsset(set, TileSpriteSetPath);
            }

            var so = new SerializedObject(set);
            FillIfEmpty(so.FindProperty("hidden"), "Board/Tile_Forest_Hidden");
            FillIfEmpty(so.FindProperty("hover"), "Board/Tile_Forest_Hover");
            FillIfEmpty(so.FindProperty("warningMarker"), "Board/Tile_Forest_WarningMarker");
            FillIfEmpty(so.FindProperty("grassPlain"), "Board/Tile_Grass_Plain");
            FillIfEmpty(so.FindProperty("nestRevealed"), "Board/Nest_Revealed");
            FillIfEmpty(so.FindProperty("nestPurified"), "Board/Nest_Purified");
            var active = so.FindProperty("nestActiveFrames");
            if (active.arraySize < 2) active.arraySize = 2;
            for (int i = 0; i < 2; i++) FillIfEmpty(active.GetArrayElementAtIndex(i), "Board/Nest_Active_Sheet", $"Nest_Active_{i}");
            var numbers = so.FindProperty("numbers");
            if (numbers.arraySize < 8) numbers.arraySize = 8;
            for (int i = 0; i < 8; i++) FillIfEmpty(numbers.GetArrayElementAtIndex(i), $"Board/Numbers/Number_{i + 1}");
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return set;
        }

        public static ForestTile EnsureTilePrefab(ForestTileSpriteSet spriteSet)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ForestTile>(TilePrefabPath);
            if (existing != null) return existing;
            EnsureFolder(PrefabFolder + "/Board");

            var root = new GameObject("ForestTile");
            var col = root.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var ground = NewSpriteChild(root, "Ground", 0, spriteSet.Hidden);
            var number = NewSpriteChild(root, "Number", 1, null);
            number.enabled = false;

            var tile = root.AddComponent<ForestTile>();
            var so = new SerializedObject(tile);
            so.FindProperty("groundRenderer").objectReferenceValue = ground;
            so.FindProperty("numberRenderer").objectReferenceValue = number;
            so.FindProperty("spriteSet").objectReferenceValue = spriteSet;
            so.ApplyModifiedPropertiesWithoutUndo();
            AddGroundAnimator(tile, ground);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, TilePrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<ForestTile>();
        }

        public static Button EnsureHudButtonPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Button>(HudButtonPrefabPath);
            if (existing != null) return existing;
            EnsureFolder(PrefabFolder + "/UI");

            var go = new GameObject("HudButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.layer = LayerMask.NameToLayer("UI");
            ((RectTransform)go.transform).sizeDelta = new Vector2(64, 24);

            var img = go.GetComponent<Image>();
            img.sprite = LoadSprite("UI/UI_Button_Normal");
            img.type = Image.Type.Simple;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            var states = button.spriteState;
            states.pressedSprite = LoadSprite("UI/UI_Button_Pressed");
            button.spriteState = states;

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 64;
            le.preferredHeight = 24;

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGO.layer = go.layer;
            labelGO.transform.SetParent(go.transform, false);
            var rt = (RectTransform)labelGO.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 3); rt.offsetMax = new Vector2(-2, -1);
            var text = labelGO.GetComponent<Text>();
            text.font = UIFont;
            text.fontSize = 11;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = TextBrown;
            text.text = "Button";

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, HudButtonPrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Button>();
        }

        /// <summary>예전에 만든 ForestTile Prefab에 새 기능(바닥 프레임 애니메이션)을 추가한다. 이미 있으면 그대로 둔다.</summary>
        public static void UpgradeTilePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TilePrefabPath) == null) return;
            var root = PrefabUtility.LoadPrefabContents(TilePrefabPath);
            try
            {
                var tile = root.GetComponent<ForestTile>();
                var groundT = root.transform.Find("Ground");
                if (tile == null || groundT == null) return;
                var so = new SerializedObject(tile);
                if (so.FindProperty("groundAnimator").objectReferenceValue != null) return;
                AddGroundAnimator(tile, groundT.GetComponent<SpriteRenderer>());
                PrefabUtility.SaveAsPrefabAsset(root, TilePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void AddGroundAnimator(ForestTile tile, SpriteRenderer ground)
        {
            var animator = ground.GetComponent<SpriteFrameAnimator>();
            if (animator == null) animator = ground.gameObject.AddComponent<SpriteFrameAnimator>();
            var animSo = new SerializedObject(animator);
            animSo.FindProperty("target").objectReferenceValue = ground;
            animSo.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(tile);
            so.FindProperty("groundAnimator").objectReferenceValue = animator;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public const string ToolSettingsPath = SettingsFolder + "/ToolSettings.asset";

        /// <summary>도구 설정 에셋을 만든다. 이미 있으면 비어 있는 스프라이트만 채운다.</summary>
        public static ToolSettings EnsureToolSettings()
        {
            EnsureFolder(SettingsFolder);
            var settings = AssetDatabase.LoadAssetAtPath<ToolSettings>(ToolSettingsPath);
            bool created = settings == null;
            if (created)
            {
                settings = ScriptableObject.CreateInstance<ToolSettings>();
                AssetDatabase.CreateAsset(settings, ToolSettingsPath);
            }

            var so = new SerializedObject(settings);
            var lemon = so.FindProperty("lemon");
            var sword = so.FindProperty("sword");
            FillIfEmpty(lemon.FindPropertyRelative("cursorSprite"), "Lemon/Lemon_Icon");
            // 검 전용 커서 에셋이 아직 없어서 임시로 카드 그림을 쓴다
            FillIfEmpty(sword.FindPropertyRelative("cursorSprite"), "Battle/CardArt/CardArt_SwordSlash");
            if (created)
            {
                lemon.FindPropertyRelative("cursorSize").vector2Value = new Vector2(24, 24);
                lemon.FindPropertyRelative("hotspot").vector2Value = new Vector2(0.5f, 0.5f);
                sword.FindPropertyRelative("cursorSize").vector2Value = new Vector2(32, 32);
                sword.FindPropertyRelative("hotspot").vector2Value = new Vector2(0.83f, 0.85f); // 칼끝
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return settings;
        }

        static SpriteRenderer NewSpriteChild(GameObject parent, string name, int sortingOrder, Sprite sprite)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            var sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        static void FillIfEmpty(SerializedProperty prop, string artPath, string spriteName = null)
        {
            if (prop.objectReferenceValue == null) prop.objectReferenceValue = LoadSprite(artPath, spriteName);
        }

        /// <summary>Art 폴더 기준 경로(확장자 제외)로 스프라이트를 읽는다. 시트 안 스프라이트는 spriteName으로 지정.</summary>
        public static Sprite LoadSprite(string artPath, string spriteName = null)
        {
            string path = $"{ArtFolder}/{artPath}.png";
            if (spriteName == null)
            {
                var single = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (single == null) Debug.LogWarning($"[Dramon] 스프라이트를 찾을 수 없어요: {path}");
                return single;
            }
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                if (o is Sprite s && s.name == spriteName) return s;
            Debug.LogWarning($"[Dramon] {path} 안에서 {spriteName}을 찾을 수 없어요.");
            return null;
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
