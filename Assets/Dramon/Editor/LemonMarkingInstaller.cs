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
    /// 레몬 표시 단계(설계 변경)를 지금 열린 Forest Scene에 실제 오브젝트로 추가한다.
    /// 예전 R키 도구 전환 관련 오브젝트는 지우지 않고 꺼 둔다(비활성 + ToolSwitcher의 '도구 전환 사용' 끔).
    /// 이미 있는 오브젝트는 다시 만들지 않고, 사용자가 바꾼 위치·설정도 덮어쓰지 않는다.
    /// </summary>
    public static class LemonMarkingInstaller
    {
        static readonly Color TitleRed = new Color32(0x9B, 0x1C, 0x1C, 0xFF);

        [MenuItem("Dramon/기능 설치 - 레몬 표시 단계 (현재 Scene)")]
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
                Debug.Log("[Dramon] 레몬 표시 단계 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            var managers = GameObject.Find("Managers");
            var board = Object.FindAnyObjectByType<ForestBoard>(FindObjectsInactive.Include);
            var hudCanvas = GameObject.Find("HUD Canvas");
            var cursorCanvas = GameObject.Find("Cursor Canvas");
            var cam = Camera.main;
            if (managers == null || board == null || hudCanvas == null || cursorCanvas == null)
            {
                EditorUtility.DisplayDialog("Dramon", "Forest Scene이 열려 있어야 해요. 먼저 'Dramon/1단계 - Forest Scene 만들기'와 '기능 설치 - 도구 전환'을 실행해 주세요.", "확인");
                return false;
            }
            var topBar = hudCanvas.transform.Find("TopBar");

            // ── 에셋 ─────────────────────────────
            DramonAssetBuilder.EnsureTileSpriteSet();
            DramonAssetBuilder.UpgradeTilePrefab();
            var rewardSettings = DramonAssetBuilder.EnsureMarkingRewardSettings();
            var cursorSettings = DramonAssetBuilder.EnsurePhaseCursorSettings();
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();

            // ── 예전 R키 도구 전환 기능 끄기 (지우지 않음) ──
            var oldSwitcher = managers.transform.Find("ToolSwitcher");
            if (oldSwitcher != null)
            {
                var sw = oldSwitcher.GetComponent<ToolSwitcher>();
                if (sw != null) SetBool(sw, "switchingEnabled", false);
            }
            foreach (var name in new[] { "ExplorationState", "ForestInput", "ExplorationScore", "ToolSwitcher", "TileActions", "DuelManager" })
                Deactivate(managers.transform.Find(name));
            foreach (var name in new[] { "ToolIndicator", "DuelPlaceholder" })
                Deactivate(hudCanvas.transform.Find(name));
            if (topBar != null)
                foreach (var name in new[] { "NestCounterText", "ScoreText", "ResultText" })
                    Deactivate(topBar.Find(name));
            Deactivate(cursorCanvas.transform.Find("ToolCursor"));

            // 안전한 칸을 다 열어도 자동 완료하지 않음 (굴은 레몬으로 직접 표시해야 함)
            SetBool(board, "autoCompleteWhenSafeCleared", false);

            // ── Managers ─────────────────────────
            var phase = GetOrAdd<GamePhaseController>(FindOrCreateChild(managers, "GamePhaseController"));
            var wallet = GetOrAdd<PlayerWallet>(FindOrCreateChild(managers, "PlayerWallet"));
            var marking = FindOrCreateChild(managers, "Marking");
            var input = GetOrAdd<MarkingInput>(FindOrCreateChild(marking, "MarkingInput"));
            var opener = GetOrAdd<TileOpener>(FindOrCreateChild(marking, "TileOpener"));
            var lemonMarker = GetOrAdd<LemonMarker>(FindOrCreateChild(marking, "LemonMarker"));
            var counter = GetOrAdd<NestMarkCounter>(FindOrCreateChild(marking, "NestMarkCounter"));
            var timer = GetOrAdd<MarkingTimer>(FindOrCreateChild(marking, "MarkingTimer"));
            var reward = GetOrAdd<MarkingRewardCalculator>(FindOrCreateChild(marking, "MarkingRewardCalculator"));

            SetRef(phase, "board", board);
            SetRef(phase, "nestMarkCounter", counter);
            SetRef(wallet, "phaseController", phase);
            SetRef(input, "targetCamera", cam);
            SetRef(input, "board", board);
            SetRef(input, "tileOpener", opener);
            SetRef(input, "lemonMarker", lemonMarker);
            SetRef(opener, "board", board);
            SetRef(opener, "phaseController", phase);
            SetRef(lemonMarker, "board", board);
            SetRef(lemonMarker, "phaseController", phase);
            SetRef(lemonMarker, "nestMarkCounter", counter);
            SetRef(counter, "board", board);
            SetRef(timer, "board", board);
            SetRef(timer, "phaseController", phase);
            SetRef(reward, "settings", rewardSettings);
            SetRef(reward, "timer", timer);
            SetRef(reward, "wallet", wallet);
            SetRef(reward, "phaseController", phase);

            // ── HUD (TopBar) ─────────────────────
            if (topBar != null)
            {
                if (topBar.Find("LemonCounter") == null) BuildLemonCounter(topBar, counter);
                if (topBar.Find("TimerPanel") == null) BuildTimerPanel(topBar, timer);
                if (topBar.Find("PhasePanel") == null) BuildPhasePanel(topBar, phase);
                if (topBar.Find("ShopButton") == null) BuildShopButton(topBar, phase, buttonPrefab);
                RepairExisting(topBar);

                var restart = topBar.Find("RestartButton");
                if (restart != null) RewireToRestart(restart.GetComponent<Button>(), phase);
            }

            // ── 창 (평소에는 꺼 둠) ──────────────
            if (hudCanvas.transform.Find("MarkingResult") == null) BuildResultWindow(hudCanvas.transform, reward, phase, buttonPrefab);
            if (hudCanvas.transform.Find("GameOver") == null) BuildGameOverWindow(hudCanvas.transform, phase, buttonPrefab);

            // ── 커서 ─────────────────────────────
            if (cursorCanvas.transform.Find("PhaseCursor") == null) BuildPhaseCursor(cursorCanvas.transform, phase, cursorSettings);

            return true;
        }

        static void BuildLemonCounter(Transform topBar, NestMarkCounter counter)
        {
            var frameGO = NewUI("LemonCounter", topBar);
            SetAnchored((RectTransform)frameGO.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(96, 32));
            var frame = frameGO.AddComponent<Image>();
            frame.sprite = DramonAssetBuilder.LoadSprite("UI/UI_LemonCounter_Frame");
            frame.raycastTarget = false;

            // 틀 그림에 이미 레몬이 그려져 있어서 아이콘은 꺼 둔다 (필요하면 켜서 사용)
            var iconGO = NewUI("Icon", frameGO.transform);
            SetAnchored((RectTransform)iconGO.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(24, 24));
            var icon = iconGO.AddComponent<Image>();
            icon.sprite = DramonAssetBuilder.LoadSprite("Lemon/Lemon_Icon");
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            iconGO.SetActive(false);

            // 숫자 칸: 틀 왼쪽 위 기준 x 32~87, y 8~23
            var textGO = NewUI("CountText", frameGO.transform);
            var rt = (RectTransform)textGO.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(32, 9); rt.offsetMax = new Vector2(-9, -8);
            var text = textGO.AddComponent<Text>();
            text.font = DramonAssetBuilder.UIFont;
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = DramonAssetBuilder.TextBrown;
            text.text = "10";
            text.raycastTarget = false;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var ui = textGO.AddComponent<NestCounterUI>();
            SetRef(ui, "markCounter", counter);
            SetRef(ui, "label", text);
            SetString(ui, "format", "{0}");
        }

        static void BuildTimerPanel(Transform topBar, MarkingTimer timer)
        {
            var panel = NewSmallWindow("TimerPanel", topBar);
            SetAnchored(panel.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(112, 0), new Vector2(72, 28));
            var text = FillText("TimerText", panel.transform, 14, "00:00");
            var ui = text.gameObject.AddComponent<MarkingTimerUI>();
            SetRef(ui, "timer", timer);
            SetRef(ui, "label", text);
        }

        static void BuildPhasePanel(Transform topBar, GamePhaseController phase)
        {
            var panel = NewSmallWindow("PhasePanel", topBar);
            SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(112, 28));
            var text = FillText("PhaseText", panel.transform, 13, "레몬 표시");
            var ui = text.gameObject.AddComponent<PhaseNameUI>();
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "label", text);
        }

        static void BuildShopButton(Transform topBar, GamePhaseController phase, Button buttonPrefab)
        {
            var shop = NewButton(buttonPrefab, topBar, "ShopButton", "상점");
            var rt = (RectTransform)shop.transform;
            SetAnchored(rt, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-88, 0), new Vector2(64, 24)); // Restart 버튼 왼쪽
            PrefabUtility.RecordPrefabInstancePropertyModifications(rt);
            shop.interactable = false; // 검 사냥 단계 전에는 비활성
            PrefabUtility.RecordPrefabInstancePropertyModifications(shop);
            var ui = shop.gameObject.AddComponent<ShopButtonUI>();
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "button", shop);
            EnsureShopDimming(ui);
        }

        static void EnsureShopDimming(ShopButtonUI ui)
        {
            var so = new SerializedObject(ui);
            var prop = so.FindProperty("canvasGroup");
            if (prop == null || prop.objectReferenceValue != null) return;
            var group = GetOrAdd<CanvasGroup>(ui.gameObject);
            group.alpha = 0.45f; // 시작은 비활성 (레몬 표시 단계)
            prop.objectReferenceValue = group;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>이전 설치본의 빠진 설정을 채운다 (사용자가 바꾼 값은 건드리지 않음).</summary>
        static void RepairExisting(Transform topBar)
        {
            var countText = topBar.Find("LemonCounter/CountText");
            var text = countText != null ? countText.GetComponent<Text>() : null;
            if (text != null && text.verticalOverflow == VerticalWrapMode.Truncate) text.verticalOverflow = VerticalWrapMode.Overflow;

            var shop = topBar.Find("ShopButton");
            var ui = shop != null ? shop.GetComponent<ShopButtonUI>() : null;
            if (ui != null) EnsureShopDimming(ui);
        }

        static void RewireToRestart(Button button, GamePhaseController phase)
        {
            if (button == null) return;
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(phase.Restart));
            PrefabUtility.RecordPrefabInstancePropertyModifications(button);
        }

        static void BuildResultWindow(Transform hud, MarkingRewardCalculator reward, GamePhaseController phase, Button buttonPrefab)
        {
            var root = NewStretchRoot("MarkingResult", hud);
            var panel = NewWindow("Panel", root.transform);
            SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 128));

            NewText("Title", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -14), new Vector2(220, 22),
                TextAnchor.MiddleCenter, 14, "숲의 굴을 모두 찾았어요!");
            var timeText = NewText("TimeText", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(200, 18),
                TextAnchor.MiddleCenter, 12, "걸린 시간  00:00");
            var rewardText = NewText("RewardText", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(200, 18),
                TextAnchor.MiddleCenter, 12, "보상금  0");

            var ok = NewButton(buttonPrefab, panel.transform, "ConfirmButton", "확인");
            SetAnchored((RectTransform)ok.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(64, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(ok.transform);
            UnityEventTools.AddPersistentListener(ok.onClick, new UnityAction(phase.BeginSwordHunt));
            PrefabUtility.RecordPrefabInstancePropertyModifications(ok);

            panel.gameObject.SetActive(false);

            var ui = root.AddComponent<MarkingResultUI>();
            SetRef(ui, "rewardCalculator", reward);
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "panel", panel.gameObject);
            SetRef(ui, "timeLabel", timeText);
            SetRef(ui, "rewardLabel", rewardText);
        }

        static void BuildGameOverWindow(Transform hud, GamePhaseController phase, Button buttonPrefab)
        {
            var root = NewStretchRoot("GameOver", hud);
            var panel = NewWindow("Panel", root.transform);
            SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 112));

            var title = NewText("Title", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -14), new Vector2(220, 24),
                TextAnchor.MiddleCenter, 18, "게임 오버");
            title.color = TitleRed;
            var reason = NewText("ReasonText", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -42), new Vector2(220, 18),
                TextAnchor.MiddleCenter, 12, "드래곤 굴을 열어 버렸어요!");

            var retry = NewButton(buttonPrefab, panel.transform, "RestartButton", "다시 시작");
            SetAnchored((RectTransform)retry.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(64, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(retry.transform);
            UnityEventTools.AddPersistentListener(retry.onClick, new UnityAction(phase.Restart));
            PrefabUtility.RecordPrefabInstancePropertyModifications(retry);

            panel.gameObject.SetActive(false);

            var ui = root.AddComponent<GameOverUI>();
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "panel", panel.gameObject);
            SetRef(ui, "reasonLabel", reason);
        }

        static void BuildPhaseCursor(Transform cursorCanvas, GamePhaseController phase, PhaseCursorSettings settings)
        {
            var go = NewUI("PhaseCursor", cursorCanvas);
            var look = settings.Get(GamePhase.LemonMarking);
            SetAnchored((RectTransform)go.transform, new Vector2(0.5f, 0.5f), look.hotspot, Vector2.zero, look.size);
            var img = go.AddComponent<Image>();
            img.sprite = look.sprite;
            img.preserveAspect = true;
            img.raycastTarget = false; // 클릭을 막지 않음

            var view = go.AddComponent<PhaseCursorView>();
            SetRef(view, "phaseController", phase);
            SetRef(view, "cursorSettings", settings);
            SetRef(view, "cursorImage", img);
        }

        // ── 작은 도우미 ──────────────────────────
        static GameObject NewStretchRoot(string name, Transform parent)
        {
            var root = NewUI(name, parent);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return root;
        }

        /// <summary>작은 패널용 9-slice 창 (테두리를 절반 크기로 그려 높이 28px에서도 안쪽이 보이게)</summary>
        static Image NewSmallWindow(string name, Transform parent)
        {
            var img = NewWindow(name, parent);
            img.pixelsPerUnitMultiplier = 2f;
            img.raycastTarget = false;
            return img;
        }

        static Text FillText(string name, Transform parent, int fontSize, string initial)
        {
            var go = NewUI(name, parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var text = go.AddComponent<Text>();
            text.font = DramonAssetBuilder.UIFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = DramonAssetBuilder.TextBrown;
            text.text = initial;
            text.raycastTarget = false;
            return text;
        }

        static void Deactivate(Transform t)
        {
            if (t != null && t.gameObject.activeSelf) t.gameObject.SetActive(false);
        }

        static void SetBool(Object target, string property, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null) return;
            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetString(Object target, string property, string value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null) return;
            prop.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
