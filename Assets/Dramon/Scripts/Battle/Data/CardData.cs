using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 카드 한 종류: 이름·종류·코스트·그림·설명·태그·효과 목록·소멸·검사 동작·상점 가격.
    /// 설명의 {0}, {1} … 은 효과 목록의 그 번호 수치, {0x}는 횟수로 바뀌어요 (강화가 반영된 최종 값).
    /// </summary>
    [CreateAssetMenu(fileName = "CardData", menuName = "Dramon/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("표시")]
        [SerializeField] string displayName = "카드";
        [Tooltip("카드 종류 (틀 색이 정해져요)")]
        [SerializeField] CardType type = CardType.Attack;
        [Tooltip("사용에 드는 레몬 에너지")]
        [Min(0)]
        [SerializeField] int cost = 1;
        [Tooltip("틀 + 그림이 합쳐진 완성 카드 (있으면 이것을 써요)")]
        [SerializeField] Sprite composedImage;
        [Tooltip("카드 그림 (완성 카드가 없을 때 틀 안에 넣어요)")]
        [SerializeField] Sprite art;
        [Tooltip("설명. {0}·{1} = 효과 목록 번호의 수치, {0x} = 그 효과의 횟수")]
        [TextArea(2, 4)]
        [SerializeField] string description = "피해 {0}";

        [Header("효과")]
        [Tooltip("검·레몬 태그 (검 단련 강화는 검 태그 카드에만)")]
        [SerializeField] CardTag tags = CardTag.None;
        [Tooltip("위에서부터 차례로 적용돼요")]
        [SerializeField] List<CardEffect> effects = new List<CardEffect>();
        [Tooltip("켜면 사용 후 이번 결투에서 제외 (소멸)")]
        [SerializeField] bool exhaust;

        [Header("검사 동작")]
        [SerializeField] HeroCardAction heroAction = HeroCardAction.Slash;
        [Tooltip("동작 반복 횟수 (연속 베기 = 2)")]
        [Min(1)]
        [SerializeField] int heroActionRepeat = 1;

        [Header("카드 강화 (카드 종류 단위 1단계 \"+\")")]
        [Tooltip("켜면 상점에서 이 카드를 강화할 수 있어요")]
        [SerializeField] bool upgradable = true;
        [Tooltip("강화 가격")]
        [Min(0)]
        [SerializeField] int upgradePrice = 100;
        [Tooltip("강화 후 코스트 (-1 = 그대로)")]
        [Min(-1)]
        [SerializeField] int upgradedCost = -1;
        [Tooltip("강화 후 효과 수치. 위 효과 목록과 같은 순서로 수치·횟수만 바꿔 넣어요 (비우면 그대로)")]
        [SerializeField] List<CardEffect> upgradedEffects = new List<CardEffect>();

        [Header("상점")]
        [Tooltip("상점 가격 (0 = 상점에서 팔지 않음)")]
        [Min(0)]
        [SerializeField] int shopPrice;
        [Tooltip("덱에 가질 수 있는 최대 장수 (상점 구매 기준, 0 = 제한 없음)")]
        [Min(0)]
        [SerializeField] int maxOwned = 3;

        [Header("임시 에셋 표시")]
        [Tooltip("그림을 임시로 다른 카드 그림으로 채웠으면 여기에 적어 둬요 (비어 있으면 정식 그림)")]
        [SerializeField] string tempAssetNote = "";

        public string DisplayName => displayName;
        public CardType Type => type;
        public int Cost => cost;
        public Sprite ComposedImage => composedImage;
        public Sprite Art => art;
        public string Description => description;
        public CardTag Tags => tags;
        public IReadOnlyList<CardEffect> Effects => effects;
        public bool Exhaust => exhaust;
        public HeroCardAction HeroAction => heroAction;
        public int HeroActionRepeat => heroActionRepeat;
        public int ShopPrice => shopPrice;
        public int MaxOwned => maxOwned;
        public bool HasTag(CardTag tag) => (tags & tag) != 0;
        public bool Upgradable => upgradable;
        public int UpgradePrice => upgradePrice;
        public string TempAssetNote => tempAssetNote;

        /// <summary>강화 여부에 따른 코스트</summary>
        public int CostFor(bool upgraded) => upgraded && upgradedCost >= 0 ? upgradedCost : cost;

        /// <summary>강화 여부에 따른 효과 i (강화 수치가 없으면 기본)</summary>
        public CardEffect EffectFor(int i, bool upgraded)
        {
            var e = effects[i];
            if (!upgraded || upgradedEffects == null || i >= upgradedEffects.Count || upgradedEffects[i] == null) return e;
            var u = upgradedEffects[i];
            return new CardEffect(e.type, u.value, Mathf.Max(1, u.count));
        }
    }
}
