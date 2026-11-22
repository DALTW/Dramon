using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dramon.EditorTools
{
    /// <summary>에디터에서 숲 보드의 칸 Prefab을 Scene에 배치·등록하는 도구. (실행 중에는 쓰지 않음)</summary>
    public static class ForestBoardBuilder
    {
        /// <summary>보드의 기존 칸을 지우고 가로×세로 크기로 칸 Prefab을 새로 배치한다. Ctrl+Z로 되돌릴 수 있다.</summary>
        public static void RebuildTiles(ForestBoard board)
        {
            if (board.TilePrefab == null)
            {
                EditorUtility.DisplayDialog("칸 다시 만들기", "보드의 'Tile Prefab' 칸이 비어 있어요.", "확인");
                return;
            }

            Undo.SetCurrentGroupName("Rebuild Forest Tiles");
            int group = Undo.GetCurrentGroup();

            var old = new List<GameObject>();
            foreach (Transform child in board.transform)
                if (child.GetComponent<ForestTile>() != null) old.Add(child.gameObject);
            foreach (var go in old) Undo.DestroyObjectImmediate(go);

            var created = new List<ForestTile>();
            for (int y = board.Height - 1; y >= 0; y--)
            for (int x = 0; x < board.Width; x++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(board.TilePrefab.gameObject, board.transform);
                Undo.RegisterCreatedObjectUndo(go, "Create Forest Tile");
                go.name = $"Tile_{x}_{y}";
                go.transform.localPosition = board.GetDefaultTileLocalPosition(x, y);

                var tile = go.GetComponent<ForestTile>();
                var tileSo = new SerializedObject(tile);
                tileSo.FindProperty("gridPosition").vector2IntValue = new Vector2Int(x, y);
                tileSo.FindProperty("board").objectReferenceValue = board;
                tileSo.ApplyModifiedPropertiesWithoutUndo();
                created.Add(tile);
            }

            AssignTileList(board, created);
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
        }

        /// <summary>보드의 자식 중 ForestTile을 모두 찾아 칸 목록에 등록한다. (칸을 직접 복제·삭제한 뒤 사용)</summary>
        public static void CollectChildTiles(ForestBoard board)
        {
            var found = new List<ForestTile>(board.GetComponentsInChildren<ForestTile>(true));
            foreach (var tile in found)
            {
                var tileSo = new SerializedObject(tile);
                tileSo.FindProperty("board").objectReferenceValue = board;
                tileSo.ApplyModifiedProperties();
            }
            AssignTileList(board, found);
            EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
        }

        static void AssignTileList(ForestBoard board, List<ForestTile> tiles)
        {
            var so = new SerializedObject(board);
            var list = so.FindProperty("tiles");
            list.arraySize = tiles.Count;
            for (int i = 0; i < tiles.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
            so.ApplyModifiedProperties();
        }
    }
}
