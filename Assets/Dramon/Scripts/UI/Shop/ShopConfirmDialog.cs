using System;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>"정말 팔까요?" 같은 확인 창. [예]를 누르면 넘겨받은 일을 하고, [아니요]·Esc면 그냥 닫는다.</summary>
    public class ShopConfirmDialog : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("확인 창 (평소에는 꺼져 있음)")]
        [SerializeField] GameObject window;
        [Tooltip("질문을 표시할 Text")]
        [SerializeField] Text messageLabel;

        Action onYes;

        public bool IsOpen => window != null && window.activeSelf;

        void Awake()
        {
            if (window != null) window.SetActive(false);
        }

        public void Show(string message, Action yes)
        {
            onYes = yes;
            if (messageLabel != null) messageLabel.text = message;
            if (window != null)
            {
                window.SetActive(true);
                window.transform.SetAsLastSibling(); // 상점 창 위에
            }
        }

        /// <summary>[예] 버튼</summary>
        public void Confirm()
        {
            var action = onYes;
            Hide();
            action?.Invoke();
        }

        /// <summary>[아니요] 버튼 · Esc · 상점 닫기</summary>
        public void Cancel() => Hide();

        void Hide()
        {
            onYes = null;
            if (window != null) window.SetActive(false);
        }
    }
}
