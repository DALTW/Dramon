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
    /// v3 4단계: 모드와 보스를 지금 열린 Forest Scene에 추가한다. (v3 3단계까지 먼저 설치)
    /// - Managers/Stage: ProgressSave(기록 저장), BossFight(보스 정하기·보스전·분노)
    /// - 결투 화면: Dragon/TEMP_BossAura(붉은 기운, 임시 원 그림), Panel/BossNameBand(이름 띠, 임시 창)
    /// - HUD Canvas: BossHint(보스 힌트), ModeSelect(모드 선택 화면), Ending(엔딩), RunRecord(게임 오버 기록)
    /// 이미 있는 오브젝트는 다시 만들지 않고, 사용자가 바꾼 연결은 덮어쓰지 않는다.
    /// </summary>
    public static class ModeBossInstaller
    {
        const string NormalModePath = DramonAssetBuilder.SettingsFolder + "/Modes/Mode_Normal.asset";
        const string EndlessModePath = DramonAssetBuilder.SettingsFolder + "/Modes/Mode_Endless.asset";
        static readonly Color Brown = new Color32(0x3B, 0x24, 0x14, 0xFF);

        [MenuItem("Dramon/기능 설치 - v3 4단계 (모드·보스, 현재 Scene)")]
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
                Debug.Log("[Dramon] v3 4단계 설치 완료: " + scene.path);
            }
        }

        static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        public static bool Install()
        {
            if (Find<CardPurchaser>() == null && !ShopV3Installer.Install()) return false;

            var progress = Find<StageProgress>();
            var phase = Find<GamePhaseController>();
            var session = Find<DuelSession>();
            var board = Find<ForestBoard>();
            var battler = Find<DragonBattlerView>();
            var health = Find<DragonHealth>();
            var intent = Find<DragonIntentPicker>();
            var floating = Find<FloatingNumberUI>();
            var screen = Find<DuelScreenUI>();
            var log = screen != null ? screen.GetComponent<BattleLogMessage>() : null;
            var hud = GameObject.Find("HUD Canvas");
            var duelPanel = screen != null ? screen.transform.Find("Panel") : null;
            var normal = AssetDatabase.LoadAssetAtPath<GameModeData>(NormalModePath);
            var endless = AssetDatabase.LoadAssetAtPath<GameModeData>(EndlessModePath);
            if (progress == null || phase == null || session == null || battler == null || hud == null || duelPanel == null)
            {
                EditorUtility.DisplayDialog("Dramon", "Forest Scene이 열려 있고 v3 1~3단계가 설치돼 있어야 해요.", "확인");
                return false;
            }
            var buttonPrefab = DramonAssetBuilder.EnsureHudButtonPrefab();
            var spawnList = new SerializedObject(session).FindProperty("spawnList").objectReferenceValue as DragonSpawnList;

            // ── Managers/Stage ───────────────────
            var stageGO = progress.transform.parent != null ? progress.transform.parent.gameObject : progress.gameObject;
            var save = GetOrAdd<ProgressSave>(FindOrCreateChild(stageGO, "ProgressSave"));
            var boss = GetOrAdd<BossFight>(FindOrCreateChild(stageGO, "BossFight"));

            // ── 결투 화면 ────────────────────────
            var aura = BuildAura(battler.transform);
            var band = BuildNameBand(duelPanel);

            // ── HUD ──────────────────────────────
            var hint = BuildHint(hud.transform, progress, boss);
            var record = BuildRecord(hud.transform, progress, phase, save, buttonPrefab);
            var ending = BuildEnding(hud.transform, progress, phase, save, buttonPrefab);
            var modeSelect = BuildModeSelect(hud.transform, progress, save, normal, endless, buttonPrefab);
            // 엔딩·기록의 [모드 선택] 버튼
            ConnectModeButton(ending != null ? ending.transform.Find("Panel/Window/ModeButton") : null, modeSelect);
            ConnectModeButton(record != null ? record.transform.Find("Panel/ModeButton") : null, modeSelect);
            modeSelect.transform.SetAsLastSibling();
            // 모드 선택 화면은 Play를 누르면 열려요 (Show On Start). 편집 중에는 Scene을 가리지 않게 꺼 둠
            var modePanel = new SerializedObject(modeSelect).FindProperty("panel").objectReferenceValue as GameObject;
            if (modePanel != null && modePanel.activeSelf) modePanel.SetActive(false);

            // ── 연결 (비어 있는 것만) ─────────────
            SetRefIfEmpty(progress, "bossFight", boss);
            SetRefIfEmpty(progress, "save", save);
            SetRefIfEmpty(boss, "phaseController", phase);
            SetRefIfEmpty(boss, "duelSession", session);
            SetRefIfEmpty(boss, "bossCandidates", spawnList);
            SetRefIfEmpty(boss, "board", board);
            SetRefIfEmpty(boss, "battler", battler);
            SetRefIfEmpty(boss, "dragonHealth", health);
            SetRefIfEmpty(boss, "intent", intent);
            SetRefIfEmpty(boss, "log", log);
            SetRefIfEmpty(boss, "floating", floating);
            SetRefIfEmpty(boss, "aura", aura);
            SetRefIfEmpty(boss, "nameBand", band);
            return true;
        }

        // ── 붉은 기운 (임시: 기본 원) ─────────────
        static BossAuraView BuildAura(Transform dragon)
        {
            var existing = dragon.Find("TEMP_BossAura");
            if (existing != null) return existing.GetComponent<BossAuraView>();
            var go = NewUI("TEMP_BossAura", dragon);
            go.transform.SetAsFirstSibling(); // 드래곤 그림 뒤
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.05f, 0f); rt.anchorMax = new Vector2(0.95f, 0.9f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            img.color = new Color(0.9f, 0.1f, 0.05f, 0.3f);
            img.raycastTarget = false;
            img.enabled = false;
            var view = go.AddComponent<BossAuraView>();
            SetRef(view, "image", img);
            return view;
        }

        // ── 보스 이름 띠 (임시: 붉은 창) ─────────
        static BossNameBandUI BuildNameBand(Transform panel)
        {
            var existing = panel.Find("BossNameBand");
            if (existing != null) return existing.GetComponent<BossNameBandUI>();
            var root = NewUI("BossNameBand", panel);
            SetAnchored((RectTransform)root.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(24, -46), new Vector2(230, 22));
            var band = NewWindow("TEMP_Band", root.transform);
            band.pixelsPerUnitMultiplier = 2f;
            band.color = new Color(1f, 0.55f, 0.5f, 1f);
            band.raycastTarget = false;
            var brt = band.rectTransform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = brt.offsetMax = Vector2.zero;
            var text = NewText("Name", band.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(222, 18),
                TextAnchor.MiddleCenter, 12, "숲의 주인 – 드래곤");
            text.color = Color.white;
            text.gameObject.AddComponent<Outline>().effectColor = new Color(0.45f, 0.05f, 0.02f, 1f);
            band.gameObject.SetActive(false);
            var ui = root.AddComponent<BossNameBandUI>();
            SetRef(ui, "band", band.gameObject);
            SetRef(ui, "label", text);
            return ui;
        }

        // ── 보스 힌트 (위 막대 아래 가운데) ─────────
        static BossHintUI BuildHint(Transform hud, StageProgress progress, BossFight boss)
        {
            var existing = hud.Find("BossHint");
            if (existing != null) return existing.GetComponent<BossHintUI>();
            var root = NewUI("BossHint", hud);
            Stretch((RectTransform)root.transform);
            var label = hud.Find("StageLabel");
            if (label != null) root.transform.SetSiblingIndex(label.GetSiblingIndex() + 1);
            var window = NewWindow("Window", root.transform);
            window.pixelsPerUnitMultiplier = 2f;
            window.raycastTarget = false;
            SetAnchored(window.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -44), new Vector2(300, 24));
            var group = window.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var text = NewText("Text", window.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(290, 20),
                TextAnchor.MiddleCenter, 11, "숲 깊은 곳에서 낯선 기척이 느껴진다…");
            text.fontStyle = FontStyle.Italic;
            window.gameObject.SetActive(false);
            var ui = root.AddComponent<BossHintUI>();
            SetRef(ui, "stageProgress", progress);
            SetRef(ui, "bossFight", boss);
            SetRef(ui, "window", window.gameObject);
            SetRef(ui, "label", text);
            SetRef(ui, "group", group);
            return ui;
        }

        // ── 게임 오버 기록 창 ───────────────────
        static RunRecordUI BuildRecord(Transform hud, StageProgress progress, GamePhaseController phase, ProgressSave save, Button buttonPrefab)
        {
            var existing = hud.Find("RunRecord");
            if (existing != null) return existing.GetComponent<RunRecordUI>();
            var root = NewUI("RunRecord", hud);
            Stretch((RectTransform)root.transform);
            var gameOver = hud.Find("GameOver");
            if (gameOver != null) root.transform.SetSiblingIndex(gameOver.GetSiblingIndex() + 1);
            var panel = NewWindow("Panel", root.transform);
            SetAnchored(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -92), new Vector2(280, 58));
            var text = NewText("Text", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -8), new Vector2(260, 18),
                TextAnchor.MiddleCenter, 11, "무한 모드 1 스테이지 도달");
            var button = NewButton(buttonPrefab, panel.transform, "ModeButton", "모드 선택");
            SetAnchored((RectTransform)button.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 6), new Vector2(84, 22));
            RecordInstance(button.transform);
            panel.gameObject.SetActive(false);
            var ui = root.AddComponent<RunRecordUI>();
            SetRef(ui, "stageProgress", progress);
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "save", save);
            SetRef(ui, "panel", panel.gameObject);
            SetRef(ui, "label", text);
            return ui;
        }

        // ── 엔딩 ────────────────────────────────
        static EndingUI BuildEnding(Transform hud, StageProgress progress, GamePhaseController phase, ProgressSave save, Button buttonPrefab)
        {
            var existing = hud.Find("Ending");
            if (existing != null) return existing.GetComponent<EndingUI>();
            var root = NewUI("Ending", hud);
            Stretch((RectTransform)root.transform);
            var panel = NewUI("Panel", root.transform);
            Stretch((RectTransform)panel.transform);
            var dim = panel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.6f);
            var window = NewWindow("Window", panel.transform);
            SetAnchored(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 176));
            var picGO = NewUI("TEMP_Picture", window.transform);
            SetAnchored((RectTransform)picGO.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0, -4), new Vector2(260, 80));
            var pic = picGO.AddComponent<Image>();
            pic.preserveAspect = true;
            pic.raycastTarget = false;
            picGO.SetActive(false);
            var top = new Vector2(0.5f, 1f);
            var title = NewText("Title", window.transform, top, top, new Vector2(0, -14), new Vector2(280, 24), TextAnchor.MiddleCenter, 17, "숲의 주인을 쓰러뜨렸다!");
            var bossText = NewText("Boss", window.transform, top, top, new Vector2(0, -44), new Vector2(280, 20), TextAnchor.MiddleCenter, 13, "숲의 주인 – 드래곤");
            var time = NewText("Time", window.transform, top, top, new Vector2(0, -70), new Vector2(280, 18), TextAnchor.MiddleCenter, 11, "클리어 시간 00:00");
            var unlock = NewText("Unlock", window.transform, top, top, new Vector2(0, -94), new Vector2(280, 18), TextAnchor.MiddleCenter, 12, "");
            unlock.color = new Color(0.12f, 0.48f, 0.12f, 1f);
            var modeButton = NewButton(buttonPrefab, window.transform, "ModeButton", "모드 선택");
            SetAnchored((RectTransform)modeButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-52, 14), new Vector2(88, 24));
            RecordInstance(modeButton.transform);
            var retry = NewButton(buttonPrefab, window.transform, "RetryButton", "다시 하기");
            SetAnchored((RectTransform)retry.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(52, 14), new Vector2(88, 24));
            RecordInstance(retry.transform);
            UnityEventTools.AddPersistentListener(retry.onClick, new UnityAction(phase.Restart));
            RecordInstance(retry);
            panel.SetActive(false);
            var ui = root.AddComponent<EndingUI>();
            SetRef(ui, "stageProgress", progress);
            SetRef(ui, "phaseController", phase);
            SetRef(ui, "save", save);
            SetRef(ui, "panel", panel);
            SetRef(ui, "titleLabel", title);
            SetRef(ui, "bossLabel", bossText);
            SetRef(ui, "timeLabel", time);
            SetRef(ui, "unlockLabel", unlock);
            SetRef(ui, "picture", pic);
            return ui;
        }

        // ── 모드 선택 화면 ──────────────────────
        static ModeSelectUI BuildModeSelect(Transform hud, StageProgress progress, ProgressSave save, GameModeData normal, GameModeData endless, Button buttonPrefab)
        {
            var existing = hud.Find("ModeSelect");
            if (existing != null) return existing.GetComponent<ModeSelectUI>();
            var root = NewUI("ModeSelect", hud);
            Stretch((RectTransform)root.transform);
            var panel = NewUI("Panel", root.transform);
            Stretch((RectTransform)panel.transform);
            var dim = panel.AddComponent<Image>();
            dim.color = new Color(0.04f, 0.07f, 0.06f, 0.92f); // 뒤쪽 숲·HUD 클릭 막기
            var window = NewWindow("Window", panel.transform);
            SetAnchored(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 220));
            var top = new Vector2(0.5f, 1f);
            NewText("Title", window.transform, top, top, new Vector2(0, -14), new Vector2(320, 30), TextAnchor.MiddleCenter, 24, "Dramon");
            NewText("Subtitle", window.transform, top, top, new Vector2(0, -44), new Vector2(320, 18), TextAnchor.MiddleCenter, 12, "모드를 골라 주세요");

            var normalButton = NewButton(buttonPrefab, window.transform, "NormalButton", "일반 모드");
            SetAnchored((RectTransform)normalButton.transform, top, top, new Vector2(-84, -72), new Vector2(132, 30));
            RecordInstance(normalButton.transform);
            var endlessButton = NewButton(buttonPrefab, window.transform, "EndlessButton", "무한 모드");
            SetAnchored((RectTransform)endlessButton.transform, top, top, new Vector2(84, -72), new Vector2(132, 30));
            RecordInstance(endlessButton.transform);
            var lockGO = NewUI("TEMP_LockIcon", endlessButton.transform);
            SetAnchored((RectTransform)lockGO.transform, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-10, 0), new Vector2(16, 16));
            var lockImg = lockGO.AddComponent<Image>();
            lockImg.preserveAspect = true;
            lockImg.raycastTarget = false;
            lockGO.SetActive(false);

            var normalDesc = NewText("NormalDescription", window.transform, top, top, new Vector2(-84, -110), new Vector2(160, 40), TextAnchor.UpperCenter, 10, "");
            normalDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
            var endlessDesc = NewText("EndlessDescription", window.transform, top, top, new Vector2(84, -110), new Vector2(160, 40), TextAnchor.UpperCenter, 10, "");
            endlessDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
            var recordText = NewText("Record", window.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(340, 18),
                TextAnchor.MiddleCenter, 10, "");
            var message = NewText("Message", window.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10), new Vector2(340, 18),
                TextAnchor.MiddleCenter, 11, "");
            message.color = new Color(0.62f, 0.14f, 0.1f, 1f);

            var ui = root.AddComponent<ModeSelectUI>();
            UnityEventTools.AddPersistentListener(normalButton.onClick, new UnityAction(ui.ChooseNormal));
            RecordInstance(normalButton);
            UnityEventTools.AddPersistentListener(endlessButton.onClick, new UnityAction(ui.ChooseEndless));
            RecordInstance(endlessButton);
            SetRef(ui, "stageProgress", progress);
            SetRef(ui, "save", save);
            SetRef(ui, "panel", panel);
            SetRef(ui, "normalMode", normal);
            SetRef(ui, "endlessMode", endless);
            SetRef(ui, "normalDescription", normalDesc);
            SetRef(ui, "endlessDescription", endlessDesc);
            SetRef(ui, "endlessButton", endlessButton);
            SetRef(ui, "lockIcon", lockImg);
            SetRef(ui, "recordLabel", recordText);
            SetRef(ui, "messageLabel", message);
            return ui;
        }

        static void ConnectModeButton(Transform t, ModeSelectUI modeSelect)
        {
            var b = t != null ? t.GetComponent<Button>() : null;
            if (b == null || modeSelect == null) return;
            for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                if (b.onClick.GetPersistentTarget(i) == modeSelect) return;
            UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(modeSelect.Open));
            RecordInstance(b);
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void RecordInstance(Object o)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(o)) PrefabUtility.RecordPrefabInstancePropertyModifications(o);
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
