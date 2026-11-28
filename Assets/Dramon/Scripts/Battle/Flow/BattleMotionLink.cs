using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>카드 결투에서 검사·드래곤 모션 호출: 카드 동작, 드래곤 공격·피격, 검사 피격·속성 반응</summary>
    public class BattleMotionLink : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] HeroBattlerView hero;
        [SerializeField] DragonBattlerView dragon;

        [Header("설정")]
        [Tooltip("켜면 카드·행동마다 모션을 보여 줘요")]
        [SerializeField] bool playMotions = true;
        [Tooltip("같은 동작을 여러 번 할 때 (연속 베기) 사이 간격(초)")]
        [Min(0f)] [SerializeField] float repeatInterval = 0.4f;

        public void PlayCard(CardData card)
        {
            if (!playMotions || hero == null || card == null) return;
            StopAllCoroutines();
            if (card.HeroActionRepeat > 1 && isActiveAndEnabled) StartCoroutine(Repeat(card.HeroAction, card.HeroActionRepeat));
            else Play(card.HeroAction);
        }

        IEnumerator Repeat(HeroCardAction action, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Play(action);
                if (i < times - 1) yield return new WaitForSeconds(repeatInterval);
            }
        }

        void Play(HeroCardAction action)
        {
            switch (action)
            {
                case HeroCardAction.Slash: hero.PlaySlash(); break;
                case HeroCardAction.Block: hero.PlayBlock(); break;
                case HeroCardAction.Roll: hero.PlayRoll(); break;
                case HeroCardAction.LemonThrow: hero.PlayLemonThrow(); break;
                case HeroCardAction.LemonSqueeze: hero.PlayLemonSqueeze(); break;
                case HeroCardAction.Herb: hero.PlayHerb(); break;
                case HeroCardAction.DrawCard: hero.PlayDrawCard(); break;
            }
        }

        public void DragonHurt()
        {
            if (playMotions && dragon != null) dragon.ShowHurt();
        }

        public void DragonAttack()
        {
            if (playMotions && dragon != null) dragon.ShowAttack();
        }

        /// <summary>검사 피격: 다 막았으면 방패, 아니면 피격</summary>
        public void HeroHit(bool blockedAll)
        {
            if (!playMotions || hero == null) return;
            if (blockedAll) hero.PlayBlock();
            else hero.PlayHit();
        }

        public void HeroBreath(DragonSpecies species)
        {
            if (playMotions && hero != null) hero.PlayDragonBreathReaction(species);
        }

        public void HeroDodge()
        {
            if (playMotions && hero != null) hero.PlayRoll();
        }
    }
}
