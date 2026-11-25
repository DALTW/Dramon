using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>HUD에 모은 재료 개수를 "재료 n개"로 보여 준다.</summary>
    public class MaterialCountUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("재료를 보관하는 인벤토리")]
        [SerializeField] PlayerInventory inventory;
        [Tooltip("개수를 표시할 Text")]
        [SerializeField] Text label;

        [Header("표시")]
        [Tooltip("표시 형식. {0} 자리에 모든 재료 개수의 합이 들어가요")]
        [SerializeField] string format = "재료 {0}개";

        void OnEnable()
        {
            if (inventory != null) inventory.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (inventory != null) inventory.Changed -= Refresh;
        }

        void Refresh()
        {
            if (label != null) label.text = string.Format(format, inventory != null ? inventory.TotalCount : 0);
        }
    }
}
