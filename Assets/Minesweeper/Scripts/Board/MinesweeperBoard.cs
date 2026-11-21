using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon.Minesweeper
{
    /// <summary>
    /// 보드 하나의 지뢰찾기 규칙(열기·깃발·주변 열기·승패 판정)을 담당한다.
    /// 칸은 Scene에 미리 배치된 CellView들을 그대로 사용하며, 실행 중에 칸을 만들거나 옮기지 않는다.
    /// </summary>
    [RequireComponent(typeof(MinePlacer))]
    public class MinesweeperBoard : MonoBehaviour
    {
        [Header("보드 크기 ('칸 다시 만들기' 버튼에서만 사용)")]
        [Tooltip("가로 칸 수. 바꾼 뒤 Inspector 아래 '칸 다시 만들기'를 눌러야 Scene에 반영돼요")]
        [SerializeField, Min(2)] int width = 9;
        [Tooltip("세로 칸 수. 바꾼 뒤 '칸 다시 만들기'를 눌러야 반영돼요")]
        [SerializeField, Min(2)] int height = 9;
        [Tooltip("칸 사이 간격(월드 단위). '칸 다시 만들기' 때 배치에 쓰여요")]
        [SerializeField, Min(0.1f)] float cellSpacing = 1f;

        [Header("연결")]
        [Tooltip("'칸 다시 만들기'에 쓸 칸 Prefab")]
        [SerializeField] CellView cellPrefab;
        [Tooltip("지뢰 배치를 맡는 컴포넌트 (같은 오브젝트에 붙어 있음)")]
        [SerializeField] MinePlacer minePlacer;
        [Tooltip("이 보드의 칸 목록. Scene에서 칸을 지우거나 옮겨도 목록에 남은 칸으로 게임이 진행돼요")]
        [SerializeField] List<CellView> cells = new List<CellView>();

        public event Action FirstRevealed;
        public event Action Exploded;
        public event Action Cleared;
        public event Action<int> FlagCountChanged;

        class CellState
        {
            public CellView view;
            public bool mine, revealed, flagged;
            public int adjacent;
        }

        static readonly Vector2Int[] NeighborOffsets =
        {
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1, 0),              new(1, 0),
            new(-1, 1),  new(0, 1),  new(1, 1),
        };

        readonly Dictionary<Vector2Int, CellState> states = new();
        bool minesPlaced;
        int mineTotal;
        int revealedSafeCount;

        public int Width => width;
        public int Height => height;
        public float CellSpacing => cellSpacing;
        public CellView CellPrefab => cellPrefab;
        public IReadOnlyList<CellView> Cells => cells;
        public bool IsFinished { get; private set; }
        public int FlagCount { get; private set; }
        public int MineCount => minesPlaced ? mineTotal : Mathf.Clamp(minePlacer.MineCount, 1, Mathf.Max(1, states.Count - 1));
        public int MinesLeft => MineCount - FlagCount;

        void Reset()
        {
            minePlacer = GetComponent<MinePlacer>();
        }

        void Awake()
        {
            if (minePlacer == null) minePlacer = GetComponent<MinePlacer>();
            BuildLookup();
            ResetBoard();
        }

        void BuildLookup()
        {
            states.Clear();
            foreach (var cell in cells)
            {
                if (cell == null) continue;
                if (states.ContainsKey(cell.GridPosition))
                {
                    Debug.LogWarning($"[MinesweeperBoard] 좌표 {cell.GridPosition}가 겹치는 칸이 있어요: {cell.name}", cell);
                    continue;
                }
                states[cell.GridPosition] = new CellState { view = cell };
            }
        }

        /// <summary>모든 칸을 닫힌 상태로 되돌린다. 칸의 위치·배치는 건드리지 않는다.</summary>
        public void ResetBoard()
        {
            foreach (var s in states.Values)
            {
                s.mine = s.revealed = s.flagged = false;
                s.adjacent = 0;
                s.view.ShowHidden();
            }
            minesPlaced = false;
            mineTotal = 0;
            revealedSafeCount = 0;
            IsFinished = false;
            SetFlagCount(0);
        }

        public void Reveal(Vector2Int pos)
        {
            if (IsFinished || !states.TryGetValue(pos, out var s) || s.revealed || s.flagged) return;

            if (!minesPlaced)
            {
                PlaceMines(pos);
                FirstRevealed?.Invoke();
            }

            if (s.mine)
            {
                s.revealed = true;
                s.view.ShowExploded();
                IsFinished = true;
                ShowRemainingMines();
                Exploded?.Invoke();
                return;
            }

            FloodReveal(pos);

            if (revealedSafeCount >= states.Count - mineTotal)
            {
                IsFinished = true;
                FlagAllMines();
                Cleared?.Invoke();
            }
        }

        public void ToggleFlag(Vector2Int pos)
        {
            if (IsFinished || !states.TryGetValue(pos, out var s) || s.revealed) return;
            s.flagged = !s.flagged;
            if (s.flagged) s.view.ShowFlag(); else s.view.ShowHidden();
            SetFlagCount(FlagCount + (s.flagged ? 1 : -1));
        }

        /// <summary>열린 숫자 칸 주변의 깃발 수가 숫자와 같으면, 깃발 없는 주변 칸을 한꺼번에 연다.</summary>
        public void Chord(Vector2Int pos)
        {
            if (IsFinished || !states.TryGetValue(pos, out var s) || !s.revealed || s.adjacent == 0) return;

            int flags = 0;
            foreach (var n in Neighbors(pos)) if (n.flagged) flags++;
            if (flags != s.adjacent) return;

            foreach (var offset in NeighborOffsets) Reveal(pos + offset);
        }

        public bool IsRevealed(Vector2Int pos) => states.TryGetValue(pos, out var s) && s.revealed;

        void PlaceMines(Vector2Int firstClick)
        {
            var mines = minePlacer.Place(states.Keys, firstClick);
            foreach (var p in mines) states[p].mine = true;
            mineTotal = mines.Count;

            foreach (var pair in states)
            {
                int count = 0;
                foreach (var n in Neighbors(pair.Key)) if (n.mine) count++;
                pair.Value.adjacent = count;
            }
            minesPlaced = true;
        }

        void FloodReveal(Vector2Int start)
        {
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                var s = states[p];
                if (s.revealed || s.flagged || s.mine) continue;

                s.revealed = true;
                revealedSafeCount++;
                s.view.ShowNumber(s.adjacent);

                if (s.adjacent != 0) continue;
                foreach (var offset in NeighborOffsets)
                {
                    var np = p + offset;
                    if (states.TryGetValue(np, out var n) && !n.revealed) queue.Enqueue(np);
                }
            }
        }

        IEnumerable<CellState> Neighbors(Vector2Int pos)
        {
            foreach (var offset in NeighborOffsets)
                if (states.TryGetValue(pos + offset, out var n)) yield return n;
        }

        void ShowRemainingMines()
        {
            foreach (var s in states.Values)
            {
                if (s.mine && !s.flagged && !s.revealed) s.view.ShowMine();
                else if (!s.mine && s.flagged) s.view.ShowWrongFlag();
            }
        }

        void FlagAllMines()
        {
            foreach (var s in states.Values)
            {
                if (!s.mine || s.flagged) continue;
                s.flagged = true;
                s.view.ShowFlag();
            }
            SetFlagCount(mineTotal);
        }

        void SetFlagCount(int value)
        {
            FlagCount = value;
            FlagCountChanged?.Invoke(value);
        }

        /// <summary>'칸 다시 만들기' 때 칸이 놓일 로컬 위치 (보드 중심 기준).</summary>
        public Vector3 GetDefaultCellLocalPosition(int x, int y) =>
            new Vector3((x - (width - 1) * 0.5f) * cellSpacing, (y - (height - 1) * 0.5f) * cellSpacing, 0f);
    }
}
