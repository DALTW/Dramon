using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>카드 더미 장수 표시 (뽑을 더미·버린 더미)</summary>
    public class PileCountUI : MonoBehaviour
    {
        [Tooltip("장수를 보여 줄 더미")]
        [SerializeField] CardPile pile;
        [SerializeField] Text label;

        void OnEnable()
        {
            if (pile != null) pile.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (pile != null) pile.Changed -= Refresh;
        }

        void Refresh()
        {
            if (label != null && pile != null) label.text = pile.Count.ToString();
        }
    }
}
