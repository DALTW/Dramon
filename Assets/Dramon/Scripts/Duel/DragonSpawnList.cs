using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>숲(보드) 하나에서 나올 수 있는 드래곤 목록. 목록 안에서 균등하게 무작위로 한 마리를 고른다.</summary>
    [CreateAssetMenu(fileName = "DragonSpawnList", menuName = "Dramon/Dragon Spawn List")]
    public class DragonSpawnList : ScriptableObject
    {
        [Tooltip("이 숲에서 나올 수 있는 드래곤 종류 (추가·삭제 가능, 모두 같은 확률)")]
        [SerializeField] List<DragonSpecies> species = new List<DragonSpecies>();

        [Tooltip("켜면 바로 전에 나온 드래곤은 연속으로 나오지 않아요 (나올 수 있는 드래곤이 2종 이상일 때)")]
        [SerializeField] bool avoidSameTwiceInARow = false;

        public IReadOnlyList<DragonSpecies> Species => species;

        /// <summary>목록에서 균등하게 무작위로 한 마리. 비어 있으면 null.</summary>
        public DragonSpecies PickRandom(DragonSpecies previous = null)
        {
            var pool = new List<DragonSpecies>();
            foreach (var s in species)
                if (s != null) pool.Add(s);
            if (pool.Count == 0) return null;
            if (avoidSameTwiceInARow && previous != null && pool.Count > 1)
                pool.RemoveAll(x => x == previous);
            return pool[Random.Range(0, pool.Count)];
        }
    }
}
