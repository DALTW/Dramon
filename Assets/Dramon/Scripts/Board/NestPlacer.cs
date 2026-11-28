using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>첫 레몬을 둔 뒤 드래곤 둥지를 어디에 숨길지 정한다. ForestBoard와 같은 오브젝트에 붙는다.</summary>
    public class NestPlacer : MonoBehaviour
    {
        [Header("드래곤 둥지")]
        [Tooltip("숲에 숨길 둥지 수 (칸 수 - 1을 넘으면 자동으로 줄어요)")]
        [SerializeField, Min(1)] int nestCount = 10;
        [Tooltip("켜면 처음 밝히는 칸과 주변 8칸에는 둥지가 나오지 않아요")]
        [SerializeField] bool safeFirstReveal = true;

        [Header("테스트")]
        [Tooltip("0이면 매번 무작위. 0이 아닌 값을 넣으면 같은 배치가 반복돼요 (디버그용)")]
        [SerializeField] int fixedSeed = 0;

        int stageNestCount;

        /// <summary>둥지 수: 스테이지 값이 정해져 있으면 그 값, 아니면 Inspector 값</summary>
        public int NestCount => stageNestCount > 0 ? stageNestCount : nestCount;

        /// <summary>스테이지 진행이 정한 둥지 수 (0이면 Inspector 값으로 돌아감). Inspector 값은 바꾸지 않는다</summary>
        public void SetStageNestCount(int count) => stageNestCount = Mathf.Max(0, count);

        public HashSet<Vector2Int> Place(IReadOnlyCollection<Vector2Int> tilePositions, Vector2Int firstReveal)
        {
            int count = Mathf.Clamp(NestCount, 1, Mathf.Max(1, tilePositions.Count - 1));

            var candidates = BuildCandidates(tilePositions, firstReveal, safeFirstReveal);
            if (candidates.Count < count) candidates = BuildCandidates(tilePositions, firstReveal, false);
            count = Mathf.Min(count, candidates.Count);

            var rng = fixedSeed != 0 ? new System.Random(fixedSeed) : new System.Random();
            var nests = new HashSet<Vector2Int>();
            for (int i = 0; i < count; i++)
            {
                int j = rng.Next(i, candidates.Count);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
                nests.Add(candidates[i]);
            }
            return nests;
        }

        static List<Vector2Int> BuildCandidates(IReadOnlyCollection<Vector2Int> tiles, Vector2Int first, bool protectNeighbors)
        {
            var list = new List<Vector2Int>(tiles.Count);
            foreach (var p in tiles)
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
