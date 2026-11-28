using UnityEngine;

namespace Dramon
{
    /// <summary>카드 결투 기본 규칙 숫자. 모두 Inspector에서 조절한다.</summary>
    [CreateAssetMenu(fileName = "BattleRules", menuName = "Dramon/Battle Rules")]
    public class BattleRules : ScriptableObject
    {
        [Header("플레이어")]
        [Tooltip("결투 시작 체력 (체력 증가 강화가 더해져요). 결투마다 가득 찬 상태로 시작해요")]
        [Min(1)]
        [SerializeField] int startHealth = 50;
        [Tooltip("매 턴 다시 채우는 레몬 에너지 (남은 에너지는 이월되지 않아요)")]
        [Min(0)]
        [SerializeField] int energyPerTurn = 3;
        [Tooltip("내 턴 시작마다 뽑는 카드 수")]
        [Min(0)]
        [SerializeField] int drawPerTurn = 5;
        [Tooltip("손패 최대 장수 (넘으면 뽑은 카드는 버린 더미로)")]
        [Min(1)]
        [SerializeField] int handLimit = 10;
        [Tooltip("한 턴에 쓸 수 있는 아이템 수 (3단계에서 사용)")]
        [Min(0)]
        [SerializeField] int itemsPerTurn = 1;

        [Header("예전 결투 순서 강화 (v3에서는 꺼 둠)")]
        [Tooltip("켜면 v3 스테이지 배율에 더해, 결투를 거듭할수록 드래곤 체력·공격 피해가 이 비율만큼 더 늘어나요. v3 규칙은 StageRules에 있어요")]
        [SerializeField] bool duelScalingEnabled = false;
        [Tooltip("결투 한 번마다 늘어나는 비율 (0.06 = 6%)")]
        [Min(0f)]
        [SerializeField] float scalingPerDuel = 0.06f;

        public int StartHealth => startHealth;
        public int EnergyPerTurn => energyPerTurn;
        public int DrawPerTurn => drawPerTurn;
        public int HandLimit => handLimit;
        public int ItemsPerTurn => itemsPerTurn;
        public bool DuelScalingEnabled => duelScalingEnabled;
        public float ScalingPerDuel => scalingPerDuel;
    }
}
