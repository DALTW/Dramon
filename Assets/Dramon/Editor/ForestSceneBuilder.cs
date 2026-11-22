using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Dramon.EditorTools
{
    /// <summary>
    /// 숲 탐험 Scene(1단계)을 한 번 구성해 저장하는 메뉴. 결과물은 전부 Scene/Prefab에 저장되며,
    /// 이미 구성된 Scene에서는 아무것도 덮어쓰지 않는다.
    /// </summary>
    public static class ForestSceneBuilder
    {
        const string ScenePath = DramonAssetBuilder.Root + "/Scenes/Forest.unity";
        const string TemplateScenePath = "Assets/Scenes/SampleScene.unity";

        // 픽셀아트 기준 해상도 (32px = 1유닛). 세로 360px = 11.25칸이 보인다.
        const int ReferenceWidth = 640;
        const int ReferenceHeight = 360;
        const int PixelsPerUnit = 32;
        const float TopBarHeight = 40f;

        const int DefaultWidth = 9;
        const int DefaultHeight = 9;
        const int DefaultNests = 10;

        [MenuItem("Dramon/1단계 - Forest Scene 만들기")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Dramon", "Play 모드를 끈 뒤 실행해 주세요.", "확인");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            DramonAssetBuilder.EnsureFolder(DramonAssetBuilder.Root + "/Scenes");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                AssetDatabase.CopyAsset(TemplateScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("Managers") != null)
            {
                EditorUtility.DisplayDialog("Dramon", "이 Scene은 이미 구성돼 있어요. 기존 배치를 덮어쓰지 않도록 아무것도 바꾸지 않았어요.", "확인");
                return;
            }

            // Scene을 연 뒤에 에셋을 불러와야 Scene 전환 때 메모리에서 해제되지 않는다.
            var spriteSet = DramonAssetBuilder.EnsureTileSpriteSet();
            var tilePrefab = DramonAssetBuilder.EnsureTilePrefab(spriteSet);
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();

            // ── Camera (초기 배치만. 실행 중에는 스크립트가 옮기지 않음) ──
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color32(0x0E, 0x14, 0x12, 0xFF);
                float hudUnits = TopBarHeight / PixelsPerUnit;
                cam.transform.position = new Vector3(0f, hudUnits * 0.5f, -10f);
                cam.orthographicSize = ReferenceHeight * 0.5f / PixelsPerUnit;

                var ppc = cam.GetComponent<PixelPerfectCamera>();
                if (ppc == null) ppc = cam.gameObject.AddComponent<PixelPerfectCamera>();
                ppc.assetsPPU = PixelsPerUnit;
                ppc.refResolutionX = ReferenceWidth;
                ppc.refResolutionY = ReferenceHeight;
            }

            // ── Managers ─────────────────────────────
            var managers = new GameObject("Managers");
            var state = NewChild(managers, "ExplorationState").AddComponent<ExplorationState>();
            var input = NewChild(managers, "ForestInput").AddComponent<ForestInput>();

            // ── Forest (보드) ─────────────────────────
            var forestRoot = new GameObject("Forest");
            var boardGO = NewChild(forestRoot, "ForestBoard");
            var placer = boardGO.AddComponent<NestPlacer>();
            var board = boardGO.AddComponent<ForestBoard>();

            var placerSo = new SerializedObject(placer);
            placerSo.FindProperty("nestCount").intValue = DefaultNests;
            placerSo.ApplyModifiedPropertiesWithoutUndo();

            var boardSo = new SerializedObject(board);
            boardSo.FindProperty("width").intValue = DefaultWidth;
            boardSo.FindProperty("height").intValue = DefaultHeight;
            boardSo.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;
            boardSo.FindProperty("nestPlacer").objectReferenceValue = placer;
            boardSo.ApplyModifiedPropertiesWithoutUndo();
            ForestBoardBuilder.RebuildTiles(board);

            SetRef(state, "board", board);
            SetRef(input, "targetCamera", cam);
            SetRef(input, "board", board);

            // ── UI ───────────────────────────────────
            BuildUI(state, board, buttonPrefab);

            // Hierarchy 순서 정리
            int order = 0;
            if (cam != null) cam.transform.SetSiblingIndex(order++);
            var light = GameObject.Find("Global Light 2D");
            if (light != null) light.transform.SetSiblingIndex(order++);
            managers.transform.SetSiblingIndex(order++);
            forestRoot.transform.SetSiblingIndex(order++);

            // 이후 추가된 기능들도 함께 설치
            ToolFeatureInstaller.Install();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AddSceneToBuildSettings(ScenePath);
            Selection.activeGameObject = boardGO;
            Debug.Log("[Dramon] 1단계 Forest Scene 구성 완료: " + ScenePath);
        }

        static void BuildUI(ExplorationState state, ForestBoard board, Button buttonPrefab)
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            var uiRoot = new GameObject("UI");

            var canvasGO = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.layer = uiLayer;
            canvasGO.transform.SetParent(uiRoot.transform, false);
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 1f;
            scaler.referencePixelsPerUnit = PixelsPerUnit;

            var topBar = NewUI("TopBar", canvasGO.transform);
            var barRt = (RectTransform)topBar.transform;
            barRt.anchorMin = new Vector2(0, 1); barRt.anchorMax = new Vector2(1, 1); barRt.pivot = new Vector2(0.5f, 1);
            barRt.sizeDelta = new Vector2(0, TopBarHeight); barRt.anchoredPosition = Vector2.zero;
            var barImage = topBar.AddComponent<Image>();
            barImage.sprite = DramonAssetBuilder.LoadSprite("UI/UI_Window_9Slice");
            barImage.type = Image.Type.Sliced;

            var nestText = NewText("NestCounterText", topBar.transform, new Vector2(0, 0.5f), new Vector2(20, 0), 160, TextAnchor.MiddleLeft, "Nests 10");
            var nestUI = nestText.gameObject.AddComponent<NestCounterUI>();
            SetRef(nestUI, "board", board);
            SetRef(nestUI, "label", nestText);

            var resultText = NewText("ResultText", topBar.transform, new Vector2(0.5f, 0.5f), Vector2.zero, 240, TextAnchor.MiddleCenter, "");
            var resultUI = resultText.gameObject.AddComponent<ExplorationResultUI>();
            SetRef(resultUI, "explorationState", state);
            SetRef(resultUI, "label", resultText);

            var restartGO = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab.gameObject, topBar.transform);
            restartGO.name = "RestartButton";
            var restartRt = (RectTransform)restartGO.transform;
            restartRt.anchorMin = restartRt.anchorMax = new Vector2(1, 0.5f);
            restartRt.pivot = new Vector2(1, 0.5f);
            restartRt.anchoredPosition = new Vector2(-16, 0);
            var label = restartGO.GetComponentInChildren<Text>();
            label.text = "Restart";
            PrefabUtility.RecordPrefabInstancePropertyModifications(label);
            var restart = restartGO.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(state.Restart));
            PrefabUtility.RecordPrefabInstancePropertyModifications(restart);
            PrefabUtility.RecordPrefabInstancePropertyModifications(restartRt);

            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.transform.SetParent(uiRoot.transform, false);
        }

        static Text NewText(string name, Transform parent, Vector2 anchor, Vector2 position, float width, TextAnchor alignment, string initial)
        {
            var go = NewUI(name, parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(alignment == TextAnchor.MiddleLeft ? 0f : 0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(width, 24);
            var text = go.AddComponent<Text>();
            text.font = DramonAssetBuilder.UIFont;
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = DramonAssetBuilder.TextBrown;
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

        static void AddSceneToBuildSettings(string path)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
