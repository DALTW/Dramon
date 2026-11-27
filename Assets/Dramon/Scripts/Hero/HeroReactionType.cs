using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤 브레스에 맞았을 때 검사가 보이는 속성 반응 종류</summary>
    public enum HeroReactionType
    {
        [InspectorName("없음 (기본 피격)")] None,
        [InspectorName("화상")] Burned,
        [InspectorName("감전")] Shocked,
        [InspectorName("빙결")] Frozen,
        [InspectorName("중독")] Poisoned,
        [InspectorName("젖음")] Drenched,
        [InspectorName("돌 맞음")] StoneHit,
        [InspectorName("공포")] Fear,
        [InspectorName("결정 박힘")] CrystalShards,
    }
}
