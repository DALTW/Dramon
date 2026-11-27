using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>레몬 에너지 "현재/최대" 표시</summary>
    public class EnergyUI : MonoBehaviour
    {
        [SerializeField] PlayerBattleStats stats;
        [SerializeField] Text label;
        [Tooltip("{0} = 현재, {1} = 최대")]
        [SerializeField] string format = "{0}/{1}";

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
            if (label != null && stats != null) label.text = string.Format(format, stats.Energy, stats.MaxEnergy);
        }
    }
}
