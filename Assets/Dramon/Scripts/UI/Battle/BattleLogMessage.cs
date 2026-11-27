using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>결투 진행 문구를 메시지 창에 바로 보여 준다 ("검 베기! 드래곤에게 6 피해")</summary>
    public class BattleLogMessage : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("메시지 창 (DuelScreen/Panel/MessageWindow)")]
        [SerializeField] GameObject window;
        [Tooltip("문구를 쓸 Text")]
        [SerializeField] Text label;

        public void Show(string message)
        {
            if (window != null && !window.activeSelf) window.SetActive(true);
            if (label != null) label.text = message;
        }
    }
}
