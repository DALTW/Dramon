using UnityEngine;

namespace Dramon
{
    /// <summary>손패 (최대 장수는 BattleRules)</summary>
    public class Hand : CardPile
    {
        [Header("연결")]
        [Tooltip("손패 최대 장수를 가져올 규칙")]
        [SerializeField] BattleRules rules;

        public int Limit => rules != null ? rules.HandLimit : 10;
        public bool IsFull => Count >= Limit;
    }
}
