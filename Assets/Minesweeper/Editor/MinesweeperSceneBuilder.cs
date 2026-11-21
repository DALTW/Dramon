using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Dramon.Minesweeper.EditorTools
{
    /// <summary>
    /// 지뢰찾기 Scene을 한 번 구성해 저장하는 메뉴. 결과물은 전부 Scene/Prefab에 저장되며,
    /// 이미 구성된 Scene에서는 아무것도 덮어쓰지 않는다.
    /// </summary>
    public static class MinesweeperSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Minesweeper.unity";
        const string TemplateScenePath = "Assets/Scenes/SampleScene.unity";

        const float ReferenceHeight = 1080f;
        const float TopBarHeight = 96f;
        const float Aspect = 16f / 9f;

        static readonly (string name, int width, int height, int mines)[] Presets =
        {
            ("Beginner", 9, 9, 10),
            ("Intermediate", 16, 16, 40),
            ("Expert", 30, 16, 99),
        };

        [MenuItem("Dramon/Minesweeper/Build Minesweeper Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Minesweeper", "Play 모드를 끈 뒤 실행해 주세요.", "확인");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            RemoveLegacyObject(TemplateScenePath);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                AssetDatabase.CopyAsset(TemplateScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("Managers") != null)
            {
                EditorUtility.DisplayDialog("Minesweeper", "이 Scene은 이미 구성돼 있어요. 기존 배치를 덮어쓰지 않도록 아무것도 바꾸지 않았어요.", "확인");
                return;
            }

            // Scene을 연 뒤에 에셋을 불러와야 Scene 전환 시 메모리에서 해제되지 않는다.
            var spriteSet = MinesweeperAssetBuilder.EnsureSpriteSet();
            var cellPrefab = MinesweeperAssetBuilder.EnsureCellPrefab(spriteSet);
            var buttonPrefab = MinesweeperAssetBuilder.EnsureHudButtonPrefab();

            var cam = Camera.main;

            // ── Managers ─────────────────────────────
            var managers = new GameObject("Managers");
            var gameState = NewChild(managers, "GameStateManager").AddComponent<GameStateManager>();
            var selector = NewChild(managers, "DifficultySelector").AddComponent<DifficultySelector>();
            var input = NewChild(managers, "BoardInput").AddComponent<BoardInput>();
            var hotkeys = NewChild(managers, "Hotkeys").AddComponent<MinesweeperHotkeys>();

            // ── Boards ───────────────────────────────
            var boardsRoot = new GameObject("Boards");
            var boards = new MinesweeperBoard[Presets.Length];
            var selectorSo = new SerializedObject(selector);
            var entries = selectorSo.FindProperty("difficulties");
            entries.arraySize = Presets.Length;

            for (int i = 0; i < Presets.Length; i++)
            {
                var p = Presets[i];
                var boardGO = NewChild(boardsRoot, "Board_" + p.name);
                var placer = boardGO.AddComponent<MinePlacer>();
                var board = boardGO.AddComponent<MinesweeperBoard>();

                var placerSo = new SerializedObject(placer);
                placerSo.FindProperty("mineCount").intValue = p.mines;
                placerSo.ApplyModifiedPropertiesWithoutUndo();

                var boardSo = new SerializedObject(board);
                boardSo.FindProperty("width").intValue = p.width;
                boardSo.FindProperty("height").intValue = p.height;
                boardSo.FindProperty("cellPrefab").objectReferenceValue = cellPrefab;
                boardSo.FindProperty("minePlacer").objectReferenceValue = placer;
                boardSo.ApplyModifiedPropertiesWithoutUndo();

                MinesweeperBoardBuilder.RebuildCells(board);
                boardGO.SetActive(i == 0);
                boards[i] = board;

                float hudFrac = TopBarHeight / ReferenceHeight;
                float size = Mathf.Max((p.height + 1f) / (2f * (1f - hudFrac)), (p.width + 1f) / (2f * Aspect));
                var entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("displayName").stringValue = p.name;
                entry.FindPropertyRelative("board").objectReferenceValue = board;
                entry.FindPropertyRelative("moveCamera").boolValue = true;
                entry.FindPropertyRelative("cameraPosition").vector3Value = new Vector3(0f, size * hudFrac, -10f);
                entry.FindPropertyRelative("cameraSize").floatValue = size;
            }
            selectorSo.FindProperty("gameState").objectReferenceValue = gameState;
            selectorSo.FindProperty("targetCamera").objectReferenceValue = cam;
            selectorSo.ApplyModifiedPropertiesWithoutUndo();

            SetRef(gameState, "currentBoard", boards[0]);
            SetRef(input, "targetCamera", cam);
            SetRef(input, "gameState", gameState);
            SetRef(hotkeys, "gameState", gameState);
            SetRef(hotkeys, "difficultySelector", selector);

            // ── Camera (초기 배치만. 실행 중에는 스크립트가 옮기지 않음) ──
            if (cam != null)
            {
                var first = entries.GetArrayElementAtIndex(0);
                cam.orthographic = true;
                cam.transform.position = first.FindPropertyRelative("cameraPosition").vector3Value;
                cam.orthographicSize = first.FindPropertyRelative("cameraSize").floatValue;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.13f, 0.16f);
            }

            // ── UI ───────────────────────────────────
            BuildUI(gameState, selector, buttonPrefab);

            // Hierarchy 순서 정리
            int order = 0;
            if (cam != null) cam.transform.SetSiblingIndex(order++);
            var light = GameObject.Find("Global Light 2D");
            if (light != null) light.transform.SetSiblingIndex(order++);
            managers.transform.SetSiblingIndex(order++);
            boardsRoot.transform.SetSiblingIndex(order++);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AddSceneToBuildSettings(ScenePath);
            Selection.activeGameObject = boards[0].gameObject;
            Debug.Log("[Minesweeper] Scene 구성 완료: " + ScenePath);
        }

        static void BuildUI(GameStateManager gameState, DifficultySelector selector, Button buttonPrefab)
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            var uiRoot = new GameObject("UI");

            var canvasGO = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.layer = uiLayer;
            canvasGO.transform.SetParent(uiRoot.transform, false);
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, ReferenceHeight);
            scaler.matchWidthOrHeight = 1f;

            var topBar = NewUI("TopBar", canvasGO.transform);
            var barRt = (RectTransform)topBar.transform;
            barRt.anchorMin = new Vector2(0, 1); barRt.anchorMax = new Vector2(1, 1); barRt.pivot = new Vector2(0.5f, 1);
            barRt.sizeDelta = new Vector2(0, TopBarHeight); barRt.anchoredPosition = Vector2.zero;
            topBar.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.1f, 0.95f);

            var mineText = NewText("MineCounterText", topBar.transform, 32, 260, "Mines 010");
            var mineUI = mineText.gameObject.AddComponent<MineCounterUI>();
            SetRef(mineUI, "gameState", gameState);
            SetRef(mineUI, "label", mineText);

            var timerText = NewText("TimerText", topBar.transform, 310, 260, "Time 000");
            var timerUI = timerText.gameObject.AddComponent<TimerUI>();
            SetRef(timerUI, "gameState", gameState);
            SetRef(timerUI, "label", timerText);

            var resultText = NewText("ResultText", topBar.transform, 590, 300, "");
            var resultUI = resultText.gameObject.AddComponent<GameResultUI>();
            SetRef(resultUI, "gameState", gameState);
            SetRef(resultUI, "label", resultText);

            var group = NewUI("Buttons", topBar.transform);
            var groupRt = (RectTransform)group.transform;
            groupRt.anchorMin = groupRt.anchorMax = new Vector2(1, 0.5f);
            groupRt.pivot = new Vector2(1, 0.5f);
            groupRt.anchoredPosition = new Vector2(-24, 0);
            groupRt.sizeDelta = new Vector2(4 * 220 + 3 * 12, 64);
            var layout = group.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            for (int i = 0; i < Presets.Length; i++)
            {
                var btn = NewButton(buttonPrefab, group.transform, Presets[i].name + "Button", $"{Presets[i].name} ({i + 1})");
                UnityEventTools.AddIntPersistentListener(btn.onClick, new UnityAction<int>(selector.Select), i);
                PrefabUtility.RecordPrefabInstancePropertyModifications(btn);
            }
            var restart = NewButton(buttonPrefab, group.transform, "RestartButton", "Restart (R)");
            UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(gameState.Restart));
            PrefabUtility.RecordPrefabInstancePropertyModifications(restart);

            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.transform.SetParent(uiRoot.transform, false);
        }

        static Button NewButton(Button prefab, Transform parent, string name, string label)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, parent);
            go.name = name;
            var text = go.GetComponentInChildren<Text>();
            text.text = label;
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            return go.GetComponent<Button>();
        }

        static Text NewText(string name, Transform parent, float x, float width, string initial)
        {
            var go = NewUI(name, parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0);
            rt.sizeDelta = new Vector2(width, 64);
            var text = go.AddComponent<Text>();
            text.font = MinesweeperAssetBuilder.UIFont;
            text.fontSize = 40;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
            text.text = initial;
            return text;
        }

        static GameObject NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject NewChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        static void SetRef(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>이전 버전(스크립트 하나로 실행 중 생성하던 방식)이 SampleScene에 남긴 오브젝트를 정리한다.</summary>
        static void RemoveLegacyObject(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null) return;
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "Minesweeper" && GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root) > 0)
                {
                    Object.DestroyImmediate(root);
                    changed = true;
                }
            }
            if (changed) EditorSceneManager.SaveScene(scene);
        }

        static void AddSceneToBuildSettings(string path)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
