using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤의 다음 행동: DragonSpecies의 행동 목록을 순서대로 반복한다</summary>
    public class DragonIntentPicker : MonoBehaviour
    {
        DragonSpecies species;
        int index;

        public DragonSpecies Species => species;

        /// <summary>지금 예고한 행동 (목록이 비어 있으면 null)</summary>
        public DragonActionData Current
        {
            get
            {
                var list = species != null ? species.BattleActions : null;
                if (list == null || list.Count == 0) return null;
                return list[index % list.Count];
            }
        }

        public void ResetFor(DragonSpecies newSpecies)
        {
            species = newSpecies;
            index = 0;
        }

        /// <summary>다음 행동으로</summary>
        public void Advance() => index++;
    }
}
