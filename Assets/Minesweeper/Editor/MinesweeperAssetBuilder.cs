using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon.Minesweeper.EditorTools
{
    /// <summary>스프라이트 임포트 설정, CellSpriteSet 에셋, Prefab(칸·HUD 버튼)을 만든다. 이미 있으면 건드리지 않는다.</summary>
    public static class MinesweeperAssetBuilder
    {
        public const string Root = "Assets/Minesweeper";
        public const string ArtFolder = Root + "/Art";
        public const string PrefabFolder = Root + "/Prefabs";
        public const string SettingsFolder = Root + "/Settings";

        public const string SpriteSetPath = SettingsFolder + "/CellSpriteSet.asset";
        public const string CellPrefabPath = PrefabFolder + "/Cell.prefab";
        public const string HudButtonPrefabPath = PrefabFolder + "/HudButton.prefab";

        const string ImportMarker = "dramon-minesweeper-sprite";
        const int PixelsPerUnit = 32;

        public static Font UIFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static CellSpriteSet EnsureSpriteSet()
        {
            EnsureFolder(SettingsFolder);
            var set = AssetDatabase.LoadAssetAtPath<CellSpriteSet>(SpriteSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<CellSpriteSet>();
                AssetDatabase.CreateAsset(set, SpriteSetPath);
            }

            var so = new SerializedObject(set);
            FillIfEmpty(so.FindProperty("hidden"), "Cell_Hidden");
            FillIfEmpty(so.FindProperty("flag"), "Cell_Flag");
            FillIfEmpty(so.FindProperty("mine"), "Cell_Mine");
            FillIfEmpty(so.FindProperty("mineExploded"), "Cell_MineExploded");
            FillIfEmpty(so.FindProperty("wrongFlag"), "Cell_WrongFlag");
            var numbers = so.FindProperty("numbers");
            if (numbers.arraySize < 9) numbers.arraySize = 9;
            for (int i = 0; i < 9; i++) FillIfEmpty(numbers.GetArrayElementAtIndex(i), "Cell_" + i);
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return set;
        }

        public static CellView EnsureCellPrefab(CellSpriteSet spriteSet)
        {
            var existing = AssetDatabase.LoadAssetAtPath<CellView>(CellPrefabPath);
            if (existing != null) return existing;
            EnsureFolder(PrefabFolder);

            var go = new GameObject("Cell");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spriteSet.Hidden;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            var cell = go.AddComponent<CellView>();
            var so = new SerializedObject(cell);
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("spriteSet").objectReferenceValue = spriteSet;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, CellPrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<CellView>();
        }

        public static Button EnsureHudButtonPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Button>(HudButtonPrefabPath);
            if (existing != null) return existing;
            EnsureFolder(PrefabFolder);

            var go = new GameObject("HudButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.layer = LayerMask.NameToLayer("UI");
            ((RectTransform)go.transform).sizeDelta = new Vector2(220, 64);
            var img = go.GetComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            img.color = new Color(0.24f, 0.26f, 0.31f);
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 220;
            le.preferredHeight = 64;

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGO.layer = go.layer;
            labelGO.transform.SetParent(go.transform, false);
            var rt = (RectTransform)labelGO.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var text = labelGO.GetComponent<Text>();
            text.font = UIFont;
            text.fontSize = 26;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = "Button";

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, HudButtonPrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Button>();
        }

        static void FillIfEmpty(SerializedProperty prop, string spriteName)
        {
            if (prop.objectReferenceValue == null) prop.objectReferenceValue = LoadSprite(spriteName);
        }

        static Sprite LoadSprite(string name)
        {
            string path = $"{ArtFolder}/{name}.png";
            if (AssetImporter.GetAtPath(path) is TextureImporter ti && ti.userData != ImportMarker)
            {
                // 처음 한 번만 픽셀아트용 설정을 넣는다. 이후 사용자가 바꾼 설정은 덮어쓰지 않는다.
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spritePixelsPerUnit = PixelsPerUnit;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.mipmapEnabled = false;
                ti.alphaIsTransparency = true;
                ti.userData = ImportMarker;
                ti.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) Debug.LogWarning($"[Minesweeper] 스프라이트를 찾을 수 없어요: {path}");
            return sprite;
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
