using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 플레이어가 모은 재료를 재료별 개수로 보관한다. (지금은 쌓기만, 파는 기능은 상점 단계에서 추가)
    /// 다시 시작(Restart)하면 비울 수 있고, 게임 오버만으로는 비우지 않는다.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        /// <summary>재료와 개수 (Inspector 확인용 목록에도 씀)</summary>
        [Serializable]
        public class Stack
        {
            public MaterialData material;
            public int count;
        }

        [Header("연결")]
        [Tooltip("다시 시작을 알려 주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        [Header("초기화")]
        [Tooltip("켜면 다시 시작(Restart)할 때 모은 재료를 비워요. 게임 오버만으로는 비우지 않아요")]
        [SerializeField] bool clearOnRestart = true;

        [Header("확인용 (실행 중 보유 재료)")]
        [Tooltip("실행 중에 모은 재료가 여기에 보여요. 확인용이라 여기서 고쳐도 게임에는 반영되지 않아요")]
        [SerializeField] List<Stack> currentStacks = new List<Stack>();

        /// <summary>재료 개수가 바뀔 때</summary>
        public event Action Changed;

        readonly Dictionary<MaterialData, int> counts = new Dictionary<MaterialData, int>();

        /// <summary>모든 재료 개수의 합</summary>
        public int TotalCount { get; private set; }
        public IReadOnlyDictionary<MaterialData, int> Counts => counts;

        void Awake() => currentStacks.Clear();

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += HandleRestarted;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= HandleRestarted;
        }

        public int GetCount(MaterialData material) =>
            material != null && counts.TryGetValue(material, out int n) ? n : 0;

        public void Add(MaterialData material, int count)
        {
            if (material == null || count <= 0) return;
            counts[material] = GetCount(material) + count;
            TotalCount += count;
            Mirror();
            Changed?.Invoke();
        }

        public void AddAll(IEnumerable<LootEntry> loot)
        {
            if (loot == null) return;
            bool any = false;
            foreach (var entry in loot)
            {
                if (entry.material == null || entry.count <= 0) continue;
                counts[entry.material] = GetCount(entry.material) + entry.count;
                TotalCount += entry.count;
                any = true;
            }
            if (!any) return;
            Mirror();
            Changed?.Invoke();
        }

        /// <summary>재료를 뺀다. 모자라면 false. (상점 단계용)</summary>
        public bool TryRemove(MaterialData material, int count)
        {
            if (material == null || count <= 0 || GetCount(material) < count) return false;
            int left = GetCount(material) - count;
            if (left > 0) counts[material] = left;
            else counts.Remove(material);
            TotalCount -= count;
            Mirror();
            Changed?.Invoke();
            return true;
        }

        public void Clear()
        {
            if (TotalCount == 0 && counts.Count == 0) return;
            counts.Clear();
            TotalCount = 0;
            Mirror();
            Changed?.Invoke();
        }

        void HandleRestarted()
        {
            if (clearOnRestart) Clear();
        }

        void Mirror()
        {
            currentStacks.Clear();
            foreach (var pair in counts)
                currentStacks.Add(new Stack { material = pair.Key, count = pair.Value });
        }
    }
}
