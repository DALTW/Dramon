using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Dramon.EditorTools.DramonEditorUtil;

namespace Dramon.EditorTools
{
    /// <summary>
    /// v3 2단계: 스테이지 진행을 지금 열린 Forest Scene에 추가한다. (v3 1단계가 없으면 먼저 설치)
    /// - Managers/Stage: StageProgress(스테이지 진행), AwakenedNestPicker(깨어난 둥지 고르기)
    /// - Main Camera: ForestCameraFitter(칸 크기·카메라 맞춤), ForestCameraPan(넘칠 때 카메라 이동)
    /// - HUD Canvas: StageLabel(스테이지 번호), StageClear(클리어 창)
    /// - 연결: 레몬 표시 보상 시간(스테이지 칸 수 기준), DuelSession 배율
    /// 이미 있는 오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결은 덮어쓰지 않는다.
    /// </summary>
    public static class StageProgressInstaller
    {
        const string StageRulesPath = DramonAssetBuilder.SettingsFolder + "/Stage/StageRules.asset";
        const string NormalModePath = DramonAssetBuilder.SettingsFolder + "/Modes/Mode_Normal.asset";

        [MenuItem("Dramon/기능 설치 - v3 2단계 (스테이지 진행, 현재 Scene)")]
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
                Debug.Log("[Dramon] v3 2단계 설치 완료: " + scene.path);
            }
        }

        public static bool Install()
        {
            if (Object.FindAnyObjectByType<DragonScaling>(FindObjectsInactive.Include) == null && !BattleV3Installer.Install()) return false;

            var managers = GameObject.Find("Managers");
            var hud = GameObject.Find("HUD Canvas");
            var phase = Object.FindAnyObjectByType<GamePhaseController>(FindObjectsInactive.Include);
            var board = Object.FindAnyObjectByType<ForestBoard>(FindObjectsInactive.Include);
            var session = Object.FindAnyObjectByType<DuelSession>(FindObjectsInactive.Include);
            var scaling = Object.FindAnyObjectByType<DragonScaling>(FindObjectsInactive.Include);
            var wallet = Object.FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
            var reward = Object.FindAnyObjectByType<MarkingRewardCalculator>(FindObjectsInactive.Include);
            var cam = Camera.main;
            if (managers == null || hud == null || phase == null || board == null || session == null || cam == null)
            {
                EditorUtility.DisplayDialog("Dramon", "Forest Scene이 열려 있어야 해요 (Managers·HUD Canvas·ForestBoard·DuelSession·Main Camera).", "확인");
                return false;
            }
            var stageRules = AssetDatabase.LoadAssetAtPath<StageRules>(StageRulesPath);
            var normalMode = AssetDatabase.LoadAssetAtPath<GameModeData>(NormalModePath);
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();

            // ── Managers/Stage ───────────────────
            var stageGO = FindOrCreateChild(managers, "Stage");
            var progress = GetOrAdd<StageProgress>(FindOrCreateChild(stageGO, "StageProgress"));
            var picker = GetOrAdd<AwakenedNestPicker>(FindOrCreateChild(stageGO, "AwakenedNestPicker"));

            // ── Main Camera ──────────────────────
            var fitter = GetOrAdd<ForestCameraFitter>(cam.gameObject);
            var pan = GetOrAdd<ForestCameraPan>(cam.gameObject);

            // ── HUD ──────────────────────────────
            var label = BuildStageLabel(hud.transform, progress);
            SetRefIfEmpty(label, "phaseController", phase);
            BuildStageClearWindow(hud.transform, progress, phase, buttonPrefab);

            // ── 연결 (비어 있는 것만) ─────────────
            SetRefIfEmpty(progress, "mode", normalMode);
            SetRefIfEmpty(progress, "fallbackRules", stageRules);
            SetRefIfEmpty(progress, "phaseController", phase);
            SetRefIfEmpty(progress, "board", board);
            SetRefIfEmpty(progress, "nestPlacer", board.GetComponent<NestPlacer>());
            SetRefIfEmpty(progress, "duelSession", session);
            SetRefIfEmpty(progress, "scaling", scaling);
            SetRefIfEmpty(progress, "wallet", wallet);
            SetRefIfEmpty(progress, "cameraFitter", fitter);

            SetRefIfEmpty(picker, "phaseController", phase);
            SetRefIfEmpty(picker, "board", board);
            SetRefIfEmpty(picker, "stageProgress", progress);

            SetRefIfEmpty(fitter, "board", board);
            SetRefIfEmpty(fitter, "pixelPerfect", cam.GetComponent<PixelPerfectCamera>());
            SetRefIfEmpty(pan, "fitter", fitter);
            SetRefIfEmpty(pan, "phaseController", phase);

            SetRefIfEmpty(reward, "stageProgress", progress);
            SetRefIfEmpty(session, "scaling", scaling);
            var clearUI = Object.FindAnyObjectByType<StageClearUI>(FindObjectsInactive.Include);
            if (clearUI != null) SetRefIfEmpty(clearUI, "nestPicker", picker);

            if (wallet == null) Debug.LogWarning("[Dramon] PlayerWallet을 찾지 못해 클리어 보상 연결을 건너뛰었어요.");
            return true;
        }

        // ── 스테이지 번호 (위 막대 아래 왼쪽) ─────
        static StageLabelUI BuildStageLabel(Transform hud, StageProgress progress)
        {
            var existing = hud.Find("StageLabel");
            if (existing != null)
            {
                var old = existing.GetComponent<StageLabelUI>();
                var g = existing.GetComponent<CanvasGroup>();
                if (g == null) { g = existing.gameObject.AddComponent<CanvasGroup>(); g.blocksRaycasts = false; }
                SetRefIfEmpty(old, "group", g);
                return old;
            }
            var window = NewWindow("StageLabel", hud);
            window.pixelsPerUnitMultiplier = 2f;
            window.raycastTarget = false;
            SetAnchored(window.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -44), new Vector2(92, 24));
            var topBar = hud.Find("TopBar");
            if (topBar != null) window.transform.SetSiblingIndex(topBar.GetSiblingIndex() + 1); // 상점·결과 창보다 아래에 그려지게
            var text = NewText("Text", window.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(88, 20),
                TextAnchor.MiddleCenter, 12, "1 스테이지");
            var group = window.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var ui = window.gameObject.AddComponent<StageLabelUI>();
            SetRef(ui, "stageProgress", progress);
            SetRef(ui, "label", text);
            SetRef(ui, "group", group);
            return ui;
        }

        // ── 스테이지 클리어 창 ─────────────────
        static void BuildStageClearWindow(Transform hud, StageProgress progress, GamePhaseController phase, Button buttonPrefab)
        {
            if (hud.Find("StageClear") != null) return;
            var root = NewUI("StageClear", hud);
            var rrt = (RectTransform)root.transform;
            rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one; rrt.offsetMin = rrt.offsetMax = Vector2.zero;
            var marking = hud.Find("MarkingResult");
            if (marking != null) root.transform.SetSiblingIndex(marking.GetSiblingIndex() + 1);

            var panel = NewWindow("Panel", root.transform);
            SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 156));

            // 클리어 그림 칸 (임시: 비어 있어서 숨김)
            var bannerGO = NewUI("Banner", panel.transform);
            SetAnchored((RectTransform)bannerGO.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0, -4), new Vector2(200, 48));
            var banner = bannerGO.AddComponent<Image>();
            banner.preserveAspect = true;
            banner.raycastTarget = false;
            bannerGO.SetActive(false);

            var title = NewText("Title", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -12), new Vector2(240, 24),
                TextAnchor.MiddleCenter, 16, "1 스테이지 클리어!");
            var rewardText = NewText("RewardText", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(220, 18),
                TextAnchor.MiddleCenter, 12, "클리어 보상  30G");
            var next = NewText("NextText", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -62), new Vector2(240, 36),
                TextAnchor.MiddleCenter, 11, "다음: 2 스테이지\n숲 6×6 · 둥지 6 · 결투 2 · 에너지 4");

            var button = NewButton(buttonPrefab, panel.transform, "NextStageButton", "다음 스테이지");
            SetAnchored((RectTransform)button.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 12), new Vector2(96, 24));
            PrefabUtility.RecordPrefabInstancePropertyModifications(button.transform);
            UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(progress.GoToNextStage));
            PrefabUtility.RecordPrefabInstancePropertyModifications(button);

            panel.gameObject.SetActive(false);

            var ui = root.AddComponent<StageClearUI>();
            SetRef(ui, "stageProgress", progress);
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "panel", panel.gameObject);
            SetRef(ui, "titleLabel", title);
            SetRef(ui, "rewardLabel", rewardText);
            SetRef(ui, "nextLabel", next);
            SetRef(ui, "banner", banner);
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
