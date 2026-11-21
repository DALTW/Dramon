using UnityEditor;
using UnityEngine;

namespace Dramon.Minesweeper.EditorTools
{
    [CustomEditor(typeof(DifficultySelector))]
    public class DifficultySelectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("에디터 도구", EditorStyles.boldLabel);

            var cam = serializedObject.FindProperty("targetCamera").objectReferenceValue as Camera;
            if (cam == null)
            {
                EditorGUILayout.HelpBox("Target Camera를 연결하면 난이도별 카메라 값을 저장·미리보기 할 수 있어요.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("'이 난이도 보기'를 누르면 해당 보드만 켜고 카메라를 저장된 값으로 옮겨요. 이 상태로 저장하면 게임도 그 난이도로 시작해요.\n카메라를 Scene에서 직접 맞춘 뒤 '현재 카메라 저장'을 누르면 그 값이 난이도 전환 때 쓰여요.", MessageType.None);

            var list = serializedObject.FindProperty("difficulties");
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                for (int i = 0; i < list.arraySize; i++)
                {
                    var entry = list.GetArrayElementAtIndex(i);
                    string label = entry.FindPropertyRelative("displayName").stringValue;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"{i + 1}. {label}", GUILayout.Width(140));
                        if (GUILayout.Button("이 난이도 보기")) Preview(list, i, cam);
                        if (GUILayout.Button("현재 카메라 저장"))
                        {
                            entry.FindPropertyRelative("cameraPosition").vector3Value = cam.transform.position;
                            entry.FindPropertyRelative("cameraSize").floatValue = cam.orthographicSize;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }

        static void Preview(SerializedProperty list, int index, Camera cam)
        {
            Undo.SetCurrentGroupName("Preview Difficulty");
            for (int i = 0; i < list.arraySize; i++)
            {
                var board = list.GetArrayElementAtIndex(i).FindPropertyRelative("board").objectReferenceValue as MinesweeperBoard;
                if (board == null) continue;
                Undo.RecordObject(board.gameObject, "Preview Difficulty");
                board.gameObject.SetActive(i == index);
            }

            var entry = list.GetArrayElementAtIndex(index);
            Undo.RecordObject(cam.transform, "Preview Difficulty");
            Undo.RecordObject(cam, "Preview Difficulty");
            cam.transform.position = entry.FindPropertyRelative("cameraPosition").vector3Value;
            cam.orthographicSize = entry.FindPropertyRelative("cameraSize").floatValue;
        }
    }
}
