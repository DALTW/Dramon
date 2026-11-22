using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dramon.EditorTools
{
    [CustomEditor(typeof(ForestBoard))]
    public class ForestBoardEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var board = (ForestBoard)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("에디터 도구", EditorStyles.boldLabel);

            int dupes = CountDuplicatePositions(board);
            EditorGUILayout.HelpBox(
                $"등록된 칸: {board.Tiles.Count}개" + (dupes > 0 ? $"\n⚠ 좌표가 겹치는 칸 {dupes}개 — ForestTile의 Grid Position을 확인하세요." : ""),
                dupes > 0 ? MessageType.Warning : MessageType.Info);

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("칸 다시 만들기 (가로×세로 크기로 새로 배치)", GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog("칸 다시 만들기",
                            $"이 보드의 기존 칸을 모두 지우고 {board.Width}×{board.Height} 칸을 Prefab으로 새로 배치해요.\n직접 옮긴 칸 위치는 초기화돼요. (Ctrl+Z로 되돌릴 수 있어요)",
                            "다시 만들기", "취소"))
                        ForestBoardBuilder.RebuildTiles(board);
                }

                if (GUILayout.Button("자식 칸 다시 등록 (칸을 직접 복제·삭제한 뒤 누르기)"))
                    ForestBoardBuilder.CollectChildTiles(board);
            }
        }

        static int CountDuplicatePositions(ForestBoard board)
        {
            var seen = new HashSet<Vector2Int>();
            int dupes = 0;
            foreach (var tile in board.Tiles)
                if (tile != null && !seen.Add(tile.GridPosition)) dupes++;
            return dupes;
        }
    }
}
