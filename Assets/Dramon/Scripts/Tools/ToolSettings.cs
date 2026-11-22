using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>도구별 커서·아이콘 모양.</summary>
    [Serializable]
    public class ToolAppearance
    {
        [Tooltip("마우스 커서로 쓸 스프라이트")]
        public Sprite cursorSprite;
        [Tooltip("커서 크기 (HUD 기준 픽셀, 640×360 화면 기준)")]
        public Vector2 cursorSize = new Vector2(24, 24);
        [Tooltip("클릭 기준점. (0,0)=왼쪽 아래, (0.5,0.5)=가운데, (1,1)=오른쪽 위. 이 점이 실제 마우스 위치에 놓여요")]
        public Vector2 hotspot = new Vector2(0.5f, 0.5f);
        [Tooltip("HUD 도구 표시에 쓸 아이콘. 비워 두면 커서 스프라이트를 써요")]
        public Sprite indicatorIcon;

        public Sprite IndicatorIcon => indicatorIcon != null ? indicatorIcon : cursorSprite;
    }

    /// <summary>레몬·검 도구 설정 묶음. 커서 모양, 기준점, 크기, 레몬 소모량을 Inspector에서 고친다.</summary>
    [CreateAssetMenu(fileName = "ToolSettings", menuName = "Dramon/Tool Settings")]
    public class ToolSettings : ScriptableObject
    {
        [Header("레몬")]
        [Tooltip("레몬을 들었을 때 커서·아이콘")]
        [SerializeField] ToolAppearance lemon = new ToolAppearance();
        [Tooltip("안전한 칸에 레몬을 둘 때 쓰는 레몬 수 (점수 감점에 반영)")]
        [SerializeField, Min(0)] int lemonsPerReveal = 1;
        [Tooltip("레몬으로 둥지를 봉인할 때 쓰는 레몬 수 (점수 감점에 반영)")]
        [SerializeField, Min(0)] int lemonsPerSeal = 1;

        [Header("검")]
        [Tooltip("검을 들었을 때 커서·아이콘 (전용 에셋이 생기면 여기서 교체)")]
        [SerializeField] ToolAppearance sword = new ToolAppearance();

        public int LemonsPerReveal => lemonsPerReveal;
        public int LemonsPerSeal => lemonsPerSeal;

        public ToolAppearance Get(PlayerTool tool) => tool == PlayerTool.Sword ? sword : lemon;
    }
}
