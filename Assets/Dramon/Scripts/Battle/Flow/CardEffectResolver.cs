using UnityEngine;

namespace Dramon
{
    /// <summary>카드 효과 처리: 피해·여러 번 피해·방어·뽑기·에너지 (산미·회복·반사 등은 2단계에서)</summary>
    public class CardEffectResolver : MonoBehaviour
    {
        /// <summary>카드 한 장을 쓴 결과 (메시지용)</summary>
        public struct Result
        {
            public int damage;   // 드래곤 체력에 들어간 피해 합
            public int blocked;  // 드래곤 방어가 막은 양
            public int hits;     // 때린 횟수
            public int block;    // 얻은 방어
            public int drawn;    // 뽑은 장수
        }

        [Header("연결")]
        [SerializeField] PlayerBattleStats player;
        [SerializeField] DragonBattleStats dragon;
        [Tooltip("카드 뽑기 효과에 씀")]
        [SerializeField] PlayerTurn playerTurn;

        public Result Apply(CardData card)
        {
            var r = new Result();
            if (card == null) return r;
            foreach (var e in card.Effects)
            {
                if (dragon != null && dragon.IsDead) break;
                switch (e.type)
                {
                    case CardEffectType.Damage:
                        Hit(e.value, ref r);
                        break;
                    case CardEffectType.MultiDamage:
                        for (int i = 0; i < Mathf.Max(1, e.count) && !dragon.IsDead; i++) Hit(e.value, ref r);
                        break;
                    case CardEffectType.Block:
                        if (player != null) player.GainBlock(e.value);
                        r.block += e.value;
                        break;
                    case CardEffectType.Draw:
                        if (playerTurn != null) r.drawn += playerTurn.Draw(e.value);
                        break;
                    case CardEffectType.Energy:
                        if (player != null) player.AddEnergy(e.value);
                        break;
                    // 산미·회복·반사·산미 비례 피해·산미 추가 피해·지속 효과는 2단계에서
                }
            }
            return r;
        }

        void Hit(int amount, ref Result r)
        {
            if (dragon == null) return;
            var (blocked, lost) = dragon.TakeAttack(amount);
            r.damage += lost;
            r.blocked += blocked;
            r.hits++;
        }
    }
}
