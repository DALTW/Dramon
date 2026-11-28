using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>보스 이름 띠 ("숲의 주인 – 화염 드래곤"). 보스전 동안 결투 화면 위쪽에 보여요.</summary>
    public class BossNameBandUI : MonoBehaviour
    {
        [Tooltip("켜고 끌 띠 (평소에는 꺼져 있음)")]
        [SerializeField] GameObject band;
        [SerializeField] Text label;

        [Header("임시 에셋 표시")]
        [TextArea(1, 2)]
        [SerializeField] string tempAssetNote = "임시: 기본 창(UI_Window_9Slice)에 붉은 색만 입힘. 보스 이름 띠 그림으로 교체";

        public string TempAssetNote => tempAssetNote;

        public void Show(string name)
        {
            if (label != null) label.text = name;
            if (band != null) band.SetActive(true);
        }

        public void Hide()
        {
            if (band != null) band.SetActive(false);
        }
    }
}
