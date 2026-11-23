using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>결투 중인 드래곤의 체력. 지금은 등장할 때 최대 체력으로 시작만 한다 (깎는 기능은 카드 결투 단계에서).</summary>
    public class DragonHealth : MonoBehaviour
    {
        /// <summary>체력이 바뀌었을 때 (현재, 최대)</summary>
        public event Action<int, int> Changed;

        public int Max { get; private set; } = 1;
        public int Current { get; private set; } = 1;
        public float Ratio => Max > 0 ? (float)Current / Max : 0f;

        /// <summary>최대 체력으로 가득 채워서 시작한다.</summary>
        public void ResetTo(int maxHealth)
        {
            Max = Mathf.Max(1, maxHealth);
            Current = Max;
            Changed?.Invoke(Current, Max);
        }

        /// <summary>체력을 정해진 값으로 바꾼다 (0 ~ 최대).</summary>
        public void SetCurrent(int value)
        {
            Current = Mathf.Clamp(value, 0, Max);
            Changed?.Invoke(Current, Max);
        }
    }
}
