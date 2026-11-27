using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>드래곤 방어 표시 (Icon_Block + 숫자, 0이면 숨김)</summary>
    public class DragonBlockUI : MonoBehaviour
    {
        [SerializeField] DragonBattleStats stats;
        [Tooltip("방어 표시 묶음")]
        [SerializeField] GameObject root;
        [SerializeField] Text label;

        void OnEnable()
        {
            if (stats != null) stats.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (stats != null) stats.Changed -= Refresh;
        }

        void Refresh()
        {
            if (stats == null) return;
            if (root != null) root.SetActive(stats.Block > 0);
            if (label != null) label.text = stats.Block.ToString();
        }
    }
}
