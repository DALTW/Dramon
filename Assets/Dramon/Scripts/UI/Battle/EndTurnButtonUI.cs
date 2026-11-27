using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>[턴 종료] 버튼: 내 턴에만 누를 수 있다 (OnClick → CardBattleController.RequestEndTurn)</summary>
    public class EndTurnButtonUI : MonoBehaviour
    {
        [SerializeField] CardBattleController controller;
        [SerializeField] Button button;
        [Tooltip("누를 수 없을 때 버튼 색")]
        [SerializeField] Color disabledColor = new Color(0.55f, 0.52f, 0.48f, 1f);

        void Update()
        {
            if (button == null || controller == null) return;
            bool on = controller.IsPlayerTurn;
            if (button.interactable == on) return;
            button.interactable = on;
            if (button.image != null) button.image.color = on ? Color.white : disabledColor;
        }
    }
}
