using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 드래곤 행동 하나: 종류, 수치, 횟수, 부여할 상태와 수치, 방어 무시 비율, 공격 모션 여부.
    /// DragonSpecies의 행동 목록에 순서대로 넣으면 그 순서로 반복해요.
    /// </summary>
    [CreateAssetMenu(fileName = "DragonAction", menuName = "Dramon/Dragon Action")]
    public class DragonActionData : ScriptableObject
    {
        [Tooltip("메시지에 보일 행동 이름 (비우면 종류 이름)")]
        [SerializeField] string displayName;
        [SerializeField] DragonActionType type = DragonActionType.Claw;
        [Tooltip("수치 (피해·방어·회복·강화량). 피해는 1번째 결투 기준이고, 결투 순서 강화가 곱해져요")]
        [Min(0)]
        [SerializeField] int value = 10;
        [Tooltip("때리는 횟수 (연타)")]
        [Min(1)]
        [SerializeField] int hits = 1;

        [Header("상태 (숨결)")]
        [Tooltip("숨결로 플레이어에게 줄 상태")]
        [SerializeField] StatusType status = StatusType.None;
        [Tooltip("상태 수치 (감전·빙결·젖음은 1 = 한 턴)")]
        [Min(0)]
        [SerializeField] int statusAmount;
        [Tooltip("플레이어 방어를 무시하는 비율 (0.5 = 방어의 절반만 계산)")]
        [Range(0f, 1f)]
        [SerializeField] float blockIgnoreRatio;

        [Header("모션")]
        [Tooltip("켜면 드래곤 공격 모션을 보여 줘요 (숨결이면 검사 속성 반응도)")]
        [SerializeField] bool playAttackMotion = true;

        public DragonActionType Type => type;
        public int Value => value;
        public int Hits => hits;
        public StatusType Status => status;
        public int StatusAmount => statusAmount;
        public float BlockIgnoreRatio => blockIgnoreRatio;
        public bool PlayAttackMotion => playAttackMotion;
        public bool IsAttack => type == DragonActionType.Claw || type == DragonActionType.MultiHit || type == DragonActionType.Breath;

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(displayName)) return displayName;
                switch (type)
                {
                    case DragonActionType.Claw: return "할퀴기";
                    case DragonActionType.MultiHit: return "연타";
                    case DragonActionType.Breath: return "숨결";
                    case DragonActionType.Defend: return "방어";
                    case DragonActionType.Heal: return "회복";
                    default: return "강화";
                }
            }
        }
    }
}
