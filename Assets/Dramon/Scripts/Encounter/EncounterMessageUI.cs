using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>등장 메시지 창 (등장 연출 ⑥). 글자를 한 글자씩 보여 줄 수 있다.</summary>
    public class EncounterMessageUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("메시지 창 (평소에는 꺼져 있음)")]
        [SerializeField] GameObject window;
        [Tooltip("메시지를 표시할 Text")]
        [SerializeField] Text label;

        [Header("글자 표시")]
        [Tooltip("1초에 보여 줄 글자 수 (0이면 한 번에 표시)")]
        [Min(0f)]
        [SerializeField] float charactersPerSecond = 30f;

        void Awake() => Hide();

        /// <summary>메시지를 띄우고, 글자가 다 나올 때까지 기다린다.</summary>
        public IEnumerator Show(string message)
        {
            if (window != null) window.SetActive(true);
            if (label == null) yield break;
            if (charactersPerSecond <= 0f || string.IsNullOrEmpty(message))
            {
                label.text = message;
                yield break;
            }
            for (float t = 0f; ; t += Time.deltaTime)
            {
                int n = Mathf.Min(message.Length, Mathf.FloorToInt(t * charactersPerSecond));
                label.text = message.Substring(0, n);
                if (n >= message.Length) yield break;
                yield return null;
            }
        }

        public void Hide()
        {
            if (window != null) window.SetActive(false);
            if (label != null) label.text = "";
        }
    }
}
