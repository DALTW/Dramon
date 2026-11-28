using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤의 다음 행동: DragonSpecies의 행동 목록을 순서대로 반복한다</summary>
    public class DragonIntentPicker : MonoBehaviour
    {
        DragonSpecies species;
        int index;
        readonly System.Collections.Generic.List<DragonActionData> extras = new System.Collections.Generic.List<DragonActionData>();

        public DragonSpecies Species => species;

        /// <summary>예고한 행동이 바뀔 때</summary>
        public event Action Changed;

        /// <summary>지금 예고한 행동 (목록이 비어 있으면 null)</summary>
        public DragonActionData Current
        {
            get
            {
                var list = species != null ? species.BattleActions : null;
                int baseCount = list != null ? list.Count : 0;
                int total = baseCount + extras.Count;
                if (total == 0) return null;
                int i = index % total;
                return i < baseCount ? list[i] : extras[i - baseCount];
            }
        }

        public void ResetFor(DragonSpecies newSpecies)
        {
            species = newSpecies;
            index = 0;
            extras.Clear();
            Changed?.Invoke();
        }

        /// <summary>행동 순서 끝에 행동을 더한다 (보스 분노의 필살 숨결)</summary>
        public void AddExtraAction(DragonActionData action)
        {
            if (action == null) return;
            extras.Add(action);
            Changed?.Invoke();
        }

        /// <summary>다음 예고를 이 행동으로 바로 바꾼다 (분노 직후 필살 숨결 예고)</summary>
        public void JumpTo(DragonActionData action)
        {
            var list = species != null ? species.BattleActions : null;
            int baseCount = list != null ? list.Count : 0;
            int total = baseCount + extras.Count;
            for (int k = 0; k < total; k++)
            {
                var a = k < baseCount ? list[k] : extras[k - baseCount];
                if (a != action) continue;
                index = k;
                Changed?.Invoke();
                return;
            }
        }

        /// <summary>다음 행동으로</summary>
        public void Advance()
        {
            index++;
            Changed?.Invoke();
        }
    }
}
