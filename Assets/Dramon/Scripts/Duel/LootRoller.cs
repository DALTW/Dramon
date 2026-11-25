using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>얻은 재료 한 줄 (재료, 개수)</summary>
    [Serializable]
    public struct LootEntry
    {
        public MaterialData material;
        public int count;

        public LootEntry(MaterialData material, int count)
        {
            this.material = material;
            this.count = count;
        }
    }

    /// <summary>
    /// 드래곤의 재료 드랍표로 이번 승리에서 얻을 재료를 굴린다.
    /// 줄마다 확률을 따로 굴리고, 나오면 최소~최대 개수 중 하나. 아무것도 안 나오면 흔함 재료 1개를 보장할 수 있다.
    /// </summary>
    public class LootRoller : MonoBehaviour
    {
        [Header("드랍 규칙")]
        [Tooltip("켜면 아무 재료도 안 나왔을 때 흔함 재료 1개를 보장해요")]
        [SerializeField] bool guaranteeCommonWhenEmpty = true;

        /// <summary>얻을 재료 목록을 정한다. dropEverything이면 확률을 무시하고 모든 줄이 나온다 (테스트용).</summary>
        public List<LootEntry> Roll(DragonSpecies species, bool dropEverything = false)
        {
            var result = new List<LootEntry>();
            if (species == null) return result;

            foreach (var drop in species.MaterialDrops)
            {
                if (drop == null || drop.material == null) continue;
                if (!dropEverything && UnityEngine.Random.value >= drop.chance) continue;
                int min = Mathf.Max(1, drop.minCount);
                int max = Mathf.Max(min, drop.maxCount);
                AddTo(result, drop.material, UnityEngine.Random.Range(min, max + 1));
            }

            if (result.Count == 0 && guaranteeCommonWhenEmpty)
            {
                var common = FindCommon(species);
                if (common != null) AddTo(result, common, 1);
            }
            return result;
        }

        /// <summary>드랍표에서 흔함 재료를 찾는다 (없으면 첫 재료).</summary>
        static MaterialData FindCommon(DragonSpecies species)
        {
            MaterialData first = null;
            foreach (var drop in species.MaterialDrops)
            {
                if (drop == null || drop.material == null) continue;
                if (drop.material.Rarity == MaterialRarity.Common) return drop.material;
                if (first == null) first = drop.material;
            }
            return first;
        }

        /// <summary>같은 재료가 여러 줄에 있으면 개수를 합친다.</summary>
        static void AddTo(List<LootEntry> list, MaterialData material, int count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].material != material) continue;
                list[i] = new LootEntry(material, list[i].count + count);
                return;
            }
            list.Add(new LootEntry(material, count));
        }
    }
}
