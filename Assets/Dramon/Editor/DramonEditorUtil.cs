using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon.EditorTools
{
    /// <summary>Scene 구성 도구들이 함께 쓰는 작은 도우미 (오브젝트 생성·참조 연결).</summary>
    public static class DramonEditorUtil
    {
        public static GameObject NewChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent.transform, false);
            return go;
        }

        public static GameObject FindOrCreateChild(GameObject parent, string name)
        {
            var t = parent.transform.Find(name);
            return t != null ? t.gameObject : NewChild(parent, name);
        }

        public static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        public static GameObject NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }

        public static void SetAnchored(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        public static Text NewText(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size,
            TextAnchor alignment, int fontSize, string initial)
        {
            var go = NewUI(name, parent);
            SetAnchored((RectTransform)go.transform, anchor, pivot, position, size);
            var text = go.AddComponent<Text>();
            text.font = DramonAssetBuilder.UIFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = DramonAssetBuilder.TextBrown;
            text.text = initial;
            text.raycastTarget = false;
            return text;
        }

        public static Image NewWindow(string name, Transform parent)
        {
            var go = NewUI(name, parent);
            var img = go.AddComponent<Image>();
            img.sprite = DramonAssetBuilder.LoadSprite("UI/UI_Window_9Slice");
            img.type = Image.Type.Sliced;
            return img;
        }

        public static Button NewButton(Button prefab, Transform parent, string name, string label)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, parent);
            go.name = name;
            var text = go.GetComponentInChildren<Text>();
            text.text = label;
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            return go.GetComponent<Button>();
        }

        public static void SetRef(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null) { Debug.LogError($"[Dramon] {target.GetType().Name}에 '{property}' 항목이 없어요."); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
