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

        [Header("결투 순서 강화 (3단계에서 사용)")]
        [Tooltip("켜면 이번 판에서 결투를 거듭할수록 드래곤 체력·할퀴기·숨결 피해가 늘어나요")]
        [SerializeField] bool duelScalingEnabled = true;
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
