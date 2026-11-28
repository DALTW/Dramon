using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 카드 한 장의 모습: 틀(종류별) + 그림 + 이름 + 코스트 + 설명. 완성 카드 그림이 있으면 그것을 쓴다.
    /// 손패에서는 마우스 올림·클릭·드래그를 HandView에 알린다. (Prefabs/UI/Battle/CardView)
    /// </summary>
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("연결")]
        [Tooltip("틀 (완성 카드가 있으면 완성 카드를 그려요)")]
        [SerializeField] Image frame;
        [Tooltip("틀 안의 그림 (완성 카드를 쓸 때는 숨겨요)")]
        [SerializeField] Image art;
        [SerializeField] Text costLabel;
        [SerializeField] Text nameLabel;
        [SerializeField] Text descriptionLabel;
        [Tooltip("못 쓰는 카드를 흐리게 할 때 씀")]
        [SerializeField] CanvasGroup canvasGroup;

        [Header("종류별 틀")]
        [SerializeField] Sprite attackFrame;
        [SerializeField] Sprite defenseFrame;
        [SerializeField] Sprite specialFrame;
        [SerializeField] Sprite powerFrame;

        [Header("표시")]
        [Tooltip("에너지가 모자라 못 쓸 때 투명도")]
        [Range(0f, 1f)]
        [SerializeField] float unplayableAlpha = 0.55f;
        [Tooltip("강화된 카드 이름 뒤에 붙는 글자")]
        [SerializeField] string upgradedSuffix = "+";
        [Tooltip("강화된 카드 이름 색")]
        [SerializeField] Color upgradedNameColor = new Color(0.12f, 0.48f, 0.12f);
        [Tooltip("코스트가 기본보다 낮아졌을 때 코스트 색")]
        [SerializeField] Color reducedCostColor = new Color(0.12f, 0.48f, 0.12f);

        Color baseNameColor = Color.black;
        Color baseCostColor = Color.white;
        bool colorsSaved;

        HandView owner;

        public CardInstance Card { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        /// <summary>카드 내용 채우기 (description = 숫자가 들어간 설명)</summary>
        public void Setup(CardInstance card, string description, HandView handOwner = null, int cost = -1, bool upgraded = false)
        {
            Card = card;
            owner = handOwner;
            var data = card != null ? card.Data : null;
            Show(data, description, cost, upgraded);
        }

        /// <summary>카드 데이터만으로 보여 주기 (상점 미리보기 등). cost −1 = 카드 기본 코스트</summary>
        public void Show(CardData data, string description) => Show(data, description, -1, false);

        public void Show(CardData data, string description, int cost, bool upgraded)
        {
            if (data == null) return;
            SaveColors();
            if (cost < 0) cost = data.Cost;
            bool composed = data.ComposedImage != null;
            if (frame != null) frame.sprite = composed ? data.ComposedImage : FrameFor(data.Type);
            if (art != null)
            {
                art.enabled = !composed && data.Art != null;
                art.sprite = data.Art;
            }
            if (costLabel != null)
            {
                costLabel.text = cost.ToString();
                costLabel.color = cost < data.Cost ? reducedCostColor : baseCostColor;
            }
            if (nameLabel != null)
            {
                nameLabel.text = upgraded ? data.DisplayName + upgradedSuffix : data.DisplayName;
                nameLabel.color = upgraded ? upgradedNameColor : baseNameColor;
            }
            if (descriptionLabel != null) descriptionLabel.text = description;
        }

        void SaveColors()
        {
            if (colorsSaved) return;
            colorsSaved = true;
            if (nameLabel != null) baseNameColor = nameLabel.color;
            if (costLabel != null) baseCostColor = costLabel.color;
        }

        public void SetPlayable(bool playable)
        {
            if (canvasGroup != null) canvasGroup.alpha = playable ? 1f : unplayableAlpha;
        }

        Sprite FrameFor(CardType type)
        {
            switch (type)
            {
                case CardType.Defense: return defenseFrame;
                case CardType.Special: return specialFrame;
                case CardType.Power: return powerFrame;
                default: return attackFrame;
            }
        }

        public void OnPointerEnter(PointerEventData e) { if (owner != null) owner.OnCardHover(this, true); }
        public void OnPointerExit(PointerEventData e) { if (owner != null) owner.OnCardHover(this, false); }
        public void OnPointerClick(PointerEventData e) { if (owner != null && e.button == PointerEventData.InputButton.Left) owner.OnCardClick(this); }
        public void OnBeginDrag(PointerEventData e) { if (owner != null) owner.OnCardBeginDrag(this, e); }
        public void OnDrag(PointerEventData e) { if (owner != null) owner.OnCardDrag(this, e); }
        public void OnEndDrag(PointerEventData e) { if (owner != null) owner.OnCardEndDrag(this, e); }
    }
}
