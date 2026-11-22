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
    /// 도구 전환(레몬 ↔ 검) 기능을 지금 열린 Forest Scene에 실제 오브젝트로 추가한다.
    /// 이미 있는 오브젝트는 다시 만들지 않고, 사용자가 바꾼 위치·설정도 덮어쓰지 않는다.
    /// </summary>
    public static class ToolFeatureInstaller
    {
        const float ReferenceWidth = 640f;
        const float ReferenceHeight = 360f;

        [MenuItem("Dramon/기능 설치 - 도구 전환 (현재 Scene)")]
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
                Debug.Log("[Dramon] 도구 전환 기능 설치 완료: " + scene.path);
            }
        }

        /// <summary>현재 Scene에 설치. 성공하면 true.</summary>
        public static bool Install()
        {
            var managers = GameObject.Find("Managers");
            var state = Object.FindAnyObjectByType<ExplorationState>();
            var board = Object.FindAnyObjectByType<ForestBoard>();
            var input = Object.FindAnyObjectByType<ForestInput>();
            var hudCanvas = GameObject.Find("HUD Canvas");
            var uiRoot = GameObject.Find("UI");
            if (managers == null || state == null || board == null || input == null || hudCanvas == null || uiRoot == null)
            {
                EditorUtility.DisplayDialog("Dramon", "Forest Scene(1단계)이 열려 있어야 해요. 먼저 'Dramon/1단계 - Forest Scene 만들기'를 실행해 주세요.", "확인");
                return false;
            }

            // ── 에셋 ─────────────────────────────
            DramonAssetBuilder.EnsureTileSpriteSet();
            DramonAssetBuilder.UpgradeTilePrefab();
            var toolSettings = DramonAssetBuilder.EnsureToolSettings();
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();

            // 재시작 단축키 제거 (재시작은 버튼으로만)
            var oldHotkeys = managers.transform.Find("ExplorationHotkeys");
            if (oldHotkeys != null) Object.DestroyImmediate(oldHotkeys.gameObject);

            // ── Managers ─────────────────────────
            var score = GetOrAdd<ExplorationScore>(FindOrCreateChild(managers, "ExplorationScore"));
            SetRef(score, "explorationState", state);

            var switcher = GetOrAdd<ToolSwitcher>(FindOrCreateChild(managers, "ToolSwitcher"));
            SetRef(switcher, "explorationState", state);

            var actions = FindOrCreateChild(managers, "TileActions");
            var resolver = GetOrAdd<TileActionResolver>(FindOrCreateChild(actions, "TileActionResolver"));
            var lemonPlacer = GetOrAdd<LemonPlacer>(FindOrCreateChild(actions, "LemonPlacer"));
            var nestSealer = GetOrAdd<NestSealer>(FindOrCreateChild(actions, "NestSealer"));
            var swordMiss = GetOrAdd<SwordMissHandler>(FindOrCreateChild(actions, "SwordMissHandler"));

            var duel = GetOrAdd<DuelManager>(FindOrCreateChild(managers, "DuelManager"));
            SetRef(duel, "board", board);
            SetRef(duel, "explorationState", state);

            SetRef(lemonPlacer, "board", board);
            SetRef(lemonPlacer, "toolSettings", toolSettings);
            SetRef(lemonPlacer, "score", score);
            SetRef(nestSealer, "board", board);
            SetRef(nestSealer, "toolSettings", toolSettings);
            SetRef(nestSealer, "score", score);
            SetRef(swordMiss, "board", board);
            SetRef(swordMiss, "score", score);

            SetRef(resolver, "board", board);
            SetRef(resolver, "toolSwitcher", switcher);
            SetRef(resolver, "lemonPlacer", lemonPlacer);
            SetRef(resolver, "nestSealer", nestSealer);
            SetRef(resolver, "swordMissHandler", swordMiss);
            SetRef(resolver, "duelManager", duel);

            SetRef(input, "actionResolver", resolver);

            // ── HUD ──────────────────────────────
            var topBar = hudCanvas.transform.Find("TopBar");
            if (topBar != null)
            {
                if (topBar.Find("ScoreText") == null)
                {
                    var scoreText = NewText("ScoreText", topBar, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(120, 0), new Vector2(140, 24),
                        TextAnchor.MiddleLeft, 14, "Score 1000");
                    var scoreUI = scoreText.gameObject.AddComponent<ScoreUI>();
                    SetRef(scoreUI, "score", score);
                    SetRef(scoreUI, "label", scoreText);
                }

                var restartLabel = topBar.Find("RestartButton/Label");
                if (restartLabel != null)
                {
                    var t = restartLabel.GetComponent<Text>();
                    if (t.text == "Restart (R)") { t.text = "Restart"; PrefabUtility.RecordPrefabInstancePropertyModifications(t); }
                }
            }

            if (hudCanvas.transform.Find("ToolIndicator") == null)
                BuildToolIndicator(hudCanvas.transform, switcher, toolSettings);

            if (hudCanvas.transform.Find("DuelPlaceholder") == null)
                BuildDuelPlaceholder(hudCanvas.transform, state, duel, buttonPrefab);

            if (uiRoot.transform.Find("Cursor Canvas") == null)
                BuildCursorCanvas(uiRoot.transform, switcher, toolSettings, state);

            return true;
        }

        static void BuildToolIndicator(Transform hud, ToolSwitcher switcher, ToolSettings settings)
        {
            var frame = NewWindow("ToolIndicator", hud);
            SetAnchored(frame.rectTransform, Vector2.zero, Vector2.zero, new Vector2(8, 8), new Vector2(112, 72));

            var iconGO = NewUI("Icon", frame.transform);
            SetAnchored((RectTransform)iconGO.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(32, 32));
            var icon = iconGO.AddComponent<Image>();
            icon.sprite = settings.Get(PlayerTool.Lemon).IndicatorIcon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var hint = NewText("HintText", frame.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10), new Vector2(100, 18),
                TextAnchor.MiddleCenter, 10, "R: 도구 바꾸기");

            var ui = frame.gameObject.AddComponent<ToolIndicatorUI>();
            SetRef(ui, "toolSwitcher", switcher);
            SetRef(ui, "toolSettings", settings);
            SetRef(ui, "iconImage", icon);
            SetRef(ui, "hintLabel", hint);
        }

        static void BuildDuelPlaceholder(Transform hud, ExplorationState state, DuelManager duel, Button buttonPrefab)
        {
            var root = NewUI("DuelPlaceholder", hud);
            var rootRt = (RectTransform)root.transform;
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;

            var panel = NewWindow("Panel", root.transform);
            SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 96));

            NewText("Title", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -14), new Vector2(210, 24),
                TextAnchor.MiddleCenter, 14, "드래곤과 결투! (임시)");

            var win = NewButton(buttonPrefab, panel.transform, "WinButton", "결투 승리");
            SetAnchored((RectTransform)win.transform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-6, 14), new Vector2(64, 24));
            UnityEventTools.AddPersistentListener(win.onClick, new UnityAction(duel.WinDuel));
            PrefabUtility.RecordPrefabInstancePropertyModifications(win);
            PrefabUtility.RecordPrefabInstancePropertyModifications(win.transform);

            var lose = NewButton(buttonPrefab, panel.transform, "LoseButton", "결투 패배");
            SetAnchored((RectTransform)lose.transform, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(6, 14), new Vector2(64, 24));
            UnityEventTools.AddPersistentListener(lose.onClick, new UnityAction(duel.LoseDuel));
            PrefabUtility.RecordPrefabInstancePropertyModifications(lose);
            PrefabUtility.RecordPrefabInstancePropertyModifications(lose.transform);

            panel.gameObject.SetActive(false); // 결투 중에만 켜짐

            var ui = root.AddComponent<DuelPlaceholderUI>();
            SetRef(ui, "explorationState", state);
            SetRef(ui, "panel", panel.gameObject);
        }

        static void BuildCursorCanvas(Transform uiRoot, ToolSwitcher switcher, ToolSettings settings, ExplorationState state)
        {
            var canvasGO = new GameObject("Cursor Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGO.layer = LayerMask.NameToLayer("UI");
            canvasGO.transform.SetParent(uiRoot, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 가장 앞
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 1f;
            scaler.referencePixelsPerUnit = 32;

            var cursorGO = NewUI("ToolCursor", canvasGO.transform);
            var look = settings.Get(PlayerTool.Lemon);
            SetAnchored((RectTransform)cursorGO.transform, new Vector2(0.5f, 0.5f), look.hotspot, Vector2.zero, look.cursorSize);
            var img = cursorGO.AddComponent<Image>();
            img.sprite = look.cursorSprite;
            img.preserveAspect = true;
            img.raycastTarget = false; // 클릭을 막지 않음

            var view = cursorGO.AddComponent<ToolCursorView>();
            SetRef(view, "toolSwitcher", switcher);
            SetRef(view, "toolSettings", settings);
            SetRef(view, "explorationState", state);
            SetRef(view, "cursorImage", img);
        }
    }
}
