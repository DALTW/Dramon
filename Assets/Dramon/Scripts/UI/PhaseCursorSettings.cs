using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>커서 모양 하나 (스프라이트·크기·클릭 기준점).</summary>
    [Serializable]
    public class CursorAppearance
    {
        [Tooltip("마우스 커서로 쓸 스프라이트")]
        public Sprite sprite;
        [Tooltip("대상 위에 있을 때 바꿔 쓸 강조 스프라이트 (검 사냥: 봉인된 굴 위 → Cursor_Sword_Glow). 비워 두면 바뀌지 않아요")]
        public Sprite highlightSprite;
        [Tooltip("커서 크기 (HUD 기준 픽셀, 640×360 화면 기준)")]
        public Vector2 size = new Vector2(24, 24);
        [Tooltip("클릭 기준점. (0,0)=왼쪽 아래, (0.5,0.5)=가운데, (1,1)=오른쪽 위. 이 점이 실제 마우스 위치에 놓여요")]
        public Vector2 hotspot = new Vector2(0.5f, 0.5f);
    }

    /// <summary>게임 단계별 마우스 커서 설정. 검 전용 에셋이 생기면 여기서 교체한다.</summary>
    [CreateAssetMenu(fileName = "PhaseCursorSettings", menuName = "Dramon/Phase Cursor Settings")]
    public class PhaseCursorSettings : ScriptableObject
    {
        [Header("레몬 표시 단계")]
        [Tooltip("칸을 열고 레몬을 놓는 동안의 커서 (Lemon_Icon)")]
        [SerializeField] CursorAppearance lemonMarking = new CursorAppearance();

        [Header("검 사냥 단계")]
        [Tooltip("봉인된 굴을 고르는 동안의 커서 (Cursor_Sword, 클릭 기준점은 칼끝)")]
        [SerializeField] CursorAppearance swordHunt = new CursorAppearance();

        /// <summary>해당 단계의 커서. 일반 커서를 써야 하는 단계면 null.</summary>
        public CursorAppearance Get(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.LemonMarking: return lemonMarking;
                case GamePhase.SwordHunt: return swordHunt;
                default: return null; // 완료 안내·게임 오버 창이 떠 있는 동안은 일반 커서
            }
        }
    }
}
