using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>한쪽(플레이어 또는 드래곤)의 상태 효과 보관: 종류별 수치. 같은 상태를 또 받으면 더한다.</summary>
    public class StatusEffects : MonoBehaviour
    {
        [Header("확인용 (실행 중)")]
        [SerializeField] List<string> current = new List<string>();

        readonly Dictionary<StatusType, int> values = new Dictionary<StatusType, int>();

        /// <summary>상태가 바뀔 때</summary>
        public event Action Changed;

        public IReadOnlyDictionary<StatusType, int> Values => values;

        public int Get(StatusType type) => values.TryGetValue(type, out var v) ? v : 0;
        public bool Has(StatusType type) => Get(type) > 0;

        public void Add(StatusType type, int amount)
        {
            if (type == StatusType.None || amount <= 0) return;
            values[type] = Get(type) + amount;
            Notify();
        }

        /// <summary>amount만큼 줄인다 (0 아래로는 안 내려감)</summary>
        public void Reduce(StatusType type, int amount = 1)
        {
            int v = Get(type);
            if (v <= 0) return;
            v = Mathf.Max(0, v - amount);
            if (v == 0) values.Remove(type);
            else values[type] = v;
            Notify();
        }

        public void Clear(StatusType type)
        {
            if (!values.Remove(type)) return;
            Notify();
        }

        public void ClearAll()
        {
            if (values.Count == 0) return;
            values.Clear();
            Notify();
        }

        void Notify()
        {
            current.Clear();
            foreach (var p in values) current.Add($"{p.Key} {p.Value}");
            Changed?.Invoke();
        }
    }
}
