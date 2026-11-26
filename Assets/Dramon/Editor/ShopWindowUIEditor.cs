using UnityEditor;
using UnityEngine;

namespace Dramon.EditorTools
{
    /// <summary>ShopWindowUI Inspector 아래에 Play 중 테스트 버튼(상점 열기·닫기, 돈·재료 넣기, 강화 초기화)을 보여 준다.</summary>
    [CustomEditor(typeof(ShopWindowUI))]
    public class ShopWindowUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("테스트 버튼", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play 중에만 눌러요.", MessageType.Info);
                return;
            }

            var ui = (ShopWindowUI)target;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("상점 열기")) ui.Open();
                if (GUILayout.Button("상점 닫기")) ui.Close();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("돈 넣기")) ui.TestAddMoney();
                if (GUILayout.Button("재료 30종 넣기")) ui.TestAddMaterials();
                if (GUILayout.Button("강화 초기화")) ui.TestResetUpgrades();
            }
        }
    }
}
