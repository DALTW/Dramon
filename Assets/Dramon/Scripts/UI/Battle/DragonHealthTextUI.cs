using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>드래곤 체력 숫자 ("40 / 40") — 체력 막대(DragonHealthUI) 옆</summary>
    public class DragonHealthTextUI : MonoBehaviour
    {
        [SerializeField] DragonHealth health;
        [SerializeField] Text label;
        [Tooltip("{0} = 현재, {1} = 최대")]
        [SerializeField] string format = "{0} / {1}";

        void OnEnable()
        {
            if (health != null) health.Changed += Refresh;
            if (health != null) Refresh(health.Current, health.Max);
        }

        void OnDisable()
        {
            if (health != null) health.Changed -= Refresh;
        }

        void Refresh(int current, int max)
        {
            if (label != null) label.text = string.Format(format, current, max);
        }
    }
}
