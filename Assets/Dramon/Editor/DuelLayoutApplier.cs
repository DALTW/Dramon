using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dramon.EditorTools
{
    /// <summary>
    /// "포켓몬 구도 적용": DuelLayoutSettings 값을 지금 열린 Scene의 PlayerSide·플레이어 체력 창에 써 넣는다.
    /// 버튼을 눌렀을 때만 쓰고, 실행 중에는 아무것도 하지 않는다 (그 뒤 Scene에서 옮긴 위치는 그대로 유지).
    /// 메시지 창이 검사보다 앞에 그려지도록 순서도 맞춘다.
    /// </summary>
    public static class DuelLayoutApplier
    {
        public const string SettingsPath = DramonAssetBuilder.SettingsFolder + "/DuelLayoutSettings.asset";

        [MenuItem("Dramon/포켓몬 구도 적용 (현재 Scene)")]
        static void ApplyFromMenu()
        {
            var settings = AssetDatabase.LoadAssetAtPath<DuelLayoutSettings>(SettingsPath);
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Dramon", "Settings/DuelLayoutSettings가 없어요. 먼저 '기능 설치 - 결투 검사'를 실행해 주세요.", "확인");
                return;
            }
            if (Apply(settings)) EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        public static bool Apply(DuelLayoutSettings settings)
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Dramon", "Play 모드를 끈 뒤 눌러 주세요.", "확인");
                return false;
            }
            var screen = Object.FindAnyObjectByType<DuelScreenUI>(FindObjectsInactive.Include);
            var panel = screen != null ? screen.transform.Find("Panel") : null;
            var playerSide = panel != null ? panel.Find("PlayerSide") as RectTransform : null;
            if (settings == null || playerSide == null)
            {
                EditorUtility.DisplayDialog("Dramon", "HUD Canvas/DuelScreen/Panel/PlayerSide를 찾을 수 없어요.", "확인");
                return false;
            }

            Undo.RecordObject(playerSide, "포켓몬 구도 적용");
            playerSide.anchorMin = playerSide.anchorMax = settings.HeroFootPoint;
            playerSide.pivot = new Vector2(0.5f, 0f); // 발밑
            playerSide.anchoredPosition = Vector2.zero;
            playerSide.sizeDelta = new Vector2(settings.HeroSize, settings.HeroSize);

            if (panel.Find("PlayerHealthWindow") is RectTransform health)
            {
                Undo.RecordObject(health, "포켓몬 구도 적용");
                health.anchorMin = health.anchorMax = settings.PlayerHealthAnchor;
                health.pivot = settings.PlayerHealthPivot;
                health.anchoredPosition = settings.PlayerHealthPosition;
                health.sizeDelta = settings.PlayerHealthSize;
            }

            // 검사를 드래곤별 작은 발판에 세울지
            var slide = Object.FindAnyObjectByType<DuelStageSlideIn>(FindObjectsInactive.Include);
            if (slide != null)
            {
                var so = new SerializedObject(slide);
                so.FindProperty("usePlayerStandPoint").boolValue = settings.StandOnNestPlatform;
                so.ApplyModifiedProperties();
            }

            // 메시지 창이 검사보다 앞(나중)에 그려지게
            var message = panel.Find("MessageWindow");
            if (message != null && message.GetSiblingIndex() < playerSide.GetSiblingIndex())
            {
                Undo.SetSiblingIndex(message, playerSide.GetSiblingIndex(), "포켓몬 구도 적용");
            }

            EditorSceneManager.MarkSceneDirty(playerSide.gameObject.scene);
            Debug.Log("[Dramon] 포켓몬 구도 적용: PlayerSide·플레이어 체력 창 위치를 바꿨어요.");
            return true;
        }
    }

    /// <summary>DuelLayoutSettings Inspector 아래에 [포켓몬 구도 적용] 버튼</summary>
    [CustomEditor(typeof(DuelLayoutSettings))]
    public class DuelLayoutSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("값을 바꾼 뒤 아래 버튼을 눌러야 Scene에 반영돼요. 실행 중에는 자동으로 적용하지 않아요.", MessageType.Info);
            if (GUILayout.Button("포켓몬 구도 적용 (현재 Scene)", GUILayout.Height(26)))
            {
                if (DuelLayoutApplier.Apply((DuelLayoutSettings)target))
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
