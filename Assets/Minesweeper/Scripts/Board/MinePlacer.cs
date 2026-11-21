using System.Collections.Generic;
using UnityEngine;

namespace Dramon.Minesweeper
{
    /// <summary>첫 클릭 때 지뢰를 어디에 놓을지 정한다. 보드 오브젝트에 MinesweeperBoard와 함께 붙는다.</summary>
    public class MinePlacer : MonoBehaviour
    {
        [Header("지뢰")]
        [Tooltip("이 보드에 놓을 지뢰 수 (칸 수 - 1을 넘으면 자동으로 줄어요)")]
        [SerializeField, Min(1)] int mineCount = 10;
        [Tooltip("켜면 첫 클릭한 칸과 주변 8칸에는 지뢰가 나오지 않아요")]
        [SerializeField] bool safeFirstClick = true;

        [Header("테스트")]
        [Tooltip("0이면 매번 무작위. 0이 아닌 값을 넣으면 같은 배치가 반복돼요 (디버그용)")]
        [SerializeField] int fixedSeed = 0;

        public int MineCount => mineCount;

        public HashSet<Vector2Int> Place(IReadOnlyCollection<Vector2Int> cellPositions, Vector2Int firstClick)
        {
            int count = Mathf.Clamp(mineCount, 1, Mathf.Max(1, cellPositions.Count - 1));

            var candidates = BuildCandidates(cellPositions, firstClick, safeFirstClick);
            if (candidates.Count < count) candidates = BuildCandidates(cellPositions, firstClick, false);
            count = Mathf.Min(count, candidates.Count);

            var rng = fixedSeed != 0 ? new System.Random(fixedSeed) : new System.Random();
            var mines = new HashSet<Vector2Int>();
            for (int i = 0; i < count; i++)
            {
                int j = rng.Next(i, candidates.Count);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
                mines.Add(candidates[i]);
            }
            return mines;
        }

        static List<Vector2Int> BuildCandidates(IReadOnlyCollection<Vector2Int> cells, Vector2Int first, bool protectNeighbors)
        {
            var list = new List<Vector2Int>(cells.Count);
            foreach (var p in cells)
            {
                bool safe = protectNeighbors
                    ? Mathf.Abs(p.x - first.x) <= 1 && Mathf.Abs(p.y - first.y) <= 1
                    : p == first;
                if (!safe) list.Add(p);
            }
            return list;
        }
    }
}
