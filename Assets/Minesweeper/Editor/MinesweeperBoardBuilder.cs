using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dramon.Minesweeper.EditorTools
{
    /// <summary>에디터에서 보드의 칸 Prefab을 Scene에 배치·등록하는 도구. (실행 중에는 쓰지 않음)</summary>
    public static class MinesweeperBoardBuilder
    {
        /// <summary>보드의 기존 칸을 지우고 가로×세로 크기로 칸 Prefab을 새로 배치한다. Ctrl+Z로 되돌릴 수 있다.</summary>
        public static void RebuildCells(MinesweeperBoard board)
        {
            if (board.CellPrefab == null)
            {
                EditorUtility.DisplayDialog("칸 다시 만들기", "보드의 'Cell Prefab' 칸이 비어 있어요.", "확인");
                return;
            }

            Undo.SetCurrentGroupName("Rebuild Minesweeper Cells");
            int group = Undo.GetCurrentGroup();

            var old = new List<GameObject>();
            foreach (Transform child in board.transform)
                if (child.GetComponent<CellView>() != null) old.Add(child.gameObject);
            foreach (var go in old) Undo.DestroyObjectImmediate(go);

            var created = new List<CellView>();
            for (int y = board.Height - 1; y >= 0; y--)
            for (int x = 0; x < board.Width; x++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(board.CellPrefab.gameObject, board.transform);
                Undo.RegisterCreatedObjectUndo(go, "Create Cell");
                go.name = $"Cell_{x}_{y}";
                go.transform.localPosition = board.GetDefaultCellLocalPosition(x, y);

                var cell = go.GetComponent<CellView>();
                var cellSo = new SerializedObject(cell);
                cellSo.FindProperty("gridPosition").vector2IntValue = new Vector2Int(x, y);
                cellSo.FindProperty("board").objectReferenceValue = board;
                cellSo.ApplyModifiedPropertiesWithoutUndo();
                created.Add(cell);
            }

            AssignCellList(board, created);
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
        }

        /// <summary>보드의 자식 중 CellView를 모두 찾아 칸 목록에 등록한다. (칸을 직접 복제·삭제한 뒤 사용)</summary>
        public static void CollectChildCells(MinesweeperBoard board)
        {
            var found = new List<CellView>(board.GetComponentsInChildren<CellView>(true));
            foreach (var cell in found)
            {
                var cellSo = new SerializedObject(cell);
                cellSo.FindProperty("board").objectReferenceValue = board;
                cellSo.ApplyModifiedProperties();
            }
            AssignCellList(board, found);
            EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
        }

        static void AssignCellList(MinesweeperBoard board, List<CellView> cells)
        {
            var so = new SerializedObject(board);
            var list = so.FindProperty("cells");
            list.arraySize = cells.Count;
            for (int i = 0; i < cells.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = cells[i];
            so.ApplyModifiedProperties();
        }
    }
}
