using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dramon
{
    /// <summary>
    /// 손패를 화면 아래 가운데에 부채꼴로 펼쳐 보여 준다. 카드는 CardView Prefab으로 만들고,
    /// 마우스를 올리면 커지며 올라오고, 클릭하거나 위로 끌어 올리면 사용한다. 에너지가 모자라면 흐리게.
    /// </summary>
    public class HandView : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] Hand hand;
        [SerializeField] PlayerTurn playerTurn;
        [Tooltip("에너지가 바뀌면 쓸 수 있는 카드를 다시 계산해요")]
        [SerializeField] PlayerBattleStats playerStats;
        [Tooltip("카드 한 장 Prefab")]
        [SerializeField] CardView cardPrefab;
        [Tooltip("카드가 놓일 곳 (이 오브젝트의 아래 가운데 기준)")]
        [SerializeField] RectTransform container;

        [Header("부채꼴 배치")]
        [Tooltip("손패 카드 크기 배율 (카드 원본 260×408)")]
        [Min(0.05f)]
        [SerializeField] float cardScale = 0.25f;
        [Tooltip("카드 사이 간격 (px)")]
        [SerializeField] float spacing = 52f;
        [Tooltip("손패 전체 최대 너비 (px). 넘으면 간격을 줄여요")]
        [SerializeField] float maxWidth = 330f;
        [Tooltip("카드마다 기울어지는 각도")]
        [SerializeField] float anglePerCard = 4f;
        [Tooltip("가장자리 카드가 내려가는 정도 (px)")]
        [SerializeField] float arcDrop = 3f;

        [Header("마우스 올림")]
        [SerializeField] float hoverScale = 0.42f;
        [Tooltip("마우스를 올린 카드의 아래쪽 높이 (px)")]
        [SerializeField] float hoverY = 6f;

        [Header("사용")]
        [Tooltip("켜면 클릭 한 번으로 사용")]
        [SerializeField] bool clickToPlay = true;
        [Tooltip("켜면 위로 끌어 올려서 사용")]
        [SerializeField] bool dragToPlay = true;
        [Tooltip("이만큼(px) 위로 끌어 올린 뒤 놓으면 사용")]
        [SerializeField] float dragPlayHeight = 70f;
        [Tooltip("카드가 자리로 움직이는 빠르기")]
        [Min(1f)]
        [SerializeField] float moveSpeed = 14f;

        readonly List<CardView> views = new List<CardView>();
        CardView hovered;
        CardView dragging;
        Vector2 dragOffset;
        Vector2 dragStart;

        void OnEnable()
        {
            if (hand != null) hand.Changed += Rebuild;
            if (playerStats != null) playerStats.Changed += RefreshPlayable;
            Rebuild();
        }

        void OnDisable()
        {
            if (hand != null) hand.Changed -= Rebuild;
            if (playerStats != null) playerStats.Changed -= RefreshPlayable;
            hovered = null;
            dragging = null;
        }

        void Rebuild()
        {
            if (hand == null || cardPrefab == null || container == null) return;
            // 손패에서 빠진 카드 지우기
            for (int i = views.Count - 1; i >= 0; i--)
            {
                bool keep = false;
                foreach (var c in hand.Cards) if (views[i] != null && views[i].Card == c) { keep = true; break; }
                if (keep) continue;
                if (views[i] != null) Destroy(views[i].gameObject);
                if (hovered == views[i]) hovered = null;
                if (dragging == views[i]) dragging = null;
                views.RemoveAt(i);
            }
            // 새로 들어온 카드 만들기 (손패 순서대로)
            var ordered = new List<CardView>();
            foreach (var c in hand.Cards)
            {
                var v = views.Find(x => x.Card == c);
                if (v == null)
                {
                    v = Instantiate(cardPrefab, container);
                    v.Setup(c, CardText.Format(c.Data), this);
                    var rt = v.Rect;
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    rt.anchoredPosition = new Vector2(0f, -200f); // 아래에서 올라옴
                    rt.localScale = Vector3.one * cardScale;
                }
                ordered.Add(v);
            }
            views.Clear();
            views.AddRange(ordered);
            for (int i = 0; i < views.Count; i++) views[i].transform.SetSiblingIndex(i);
            RefreshPlayable();
        }

        void RefreshPlayable()
        {
            foreach (var v in views) if (v != null) v.SetPlayable(playerTurn == null || playerTurn.CanPlay(v.Card));
        }

        void Update()
        {
            int n = views.Count;
            if (n == 0) return;
            float step = n > 1 ? Mathf.Min(spacing, maxWidth / (n - 1)) : 0f;
            float k = 1f - Mathf.Exp(-moveSpeed * Time.deltaTime);
            for (int i = 0; i < n; i++)
            {
                var v = views[i];
                if (v == null || v == dragging) continue;
                float offset = i - (n - 1) / 2f;
                Vector2 pos = new Vector2(offset * step, -arcDrop * offset * offset);
                float angle = -offset * anglePerCard;
                float scale = cardScale;
                if (v == hovered)
                {
                    pos.y = hoverY;
                    angle = 0f;
                    scale = hoverScale;
                }
                var rt = v.Rect;
                rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, pos, k);
                rt.localRotation = Quaternion.Slerp(rt.localRotation, Quaternion.Euler(0, 0, angle), k);
                rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * scale, k);
            }
        }

        // ── CardView가 부름 ─────────────────────
        public void OnCardHover(CardView v, bool enter)
        {
            if (dragging != null) return;
            if (enter)
            {
                hovered = v;
                v.transform.SetAsLastSibling(); // 다른 카드 위로
                if (playerTurn != null && playerTurn.IsActive) v.SetPlayable(playerTurn.CanPlay(v.Card));
            }
            else if (hovered == v)
            {
                hovered = null;
                v.transform.SetSiblingIndex(views.IndexOf(v));
            }
        }

        public void OnCardClick(CardView v)
        {
            if (!clickToPlay || dragging != null) return;
            Play(v);
        }

        public void OnCardBeginDrag(CardView v, PointerEventData e)
        {
            if (!dragToPlay) return;
            dragging = v;
            hovered = v;
            v.transform.SetAsLastSibling();
            ToLocal(e, out var local);
            dragStart = local;
            dragOffset = v.Rect.anchoredPosition - local;
        }

        public void OnCardDrag(CardView v, PointerEventData e)
        {
            if (dragging != v) return;
            ToLocal(e, out var local);
            v.Rect.anchoredPosition = local + dragOffset;
            v.Rect.localRotation = Quaternion.identity;
        }

        public void OnCardEndDrag(CardView v, PointerEventData e)
        {
            if (dragging != v) return;
            dragging = null;
            ToLocal(e, out var local);
            if (local.y - dragStart.y >= dragPlayHeight) Play(v);
        }

        void Play(CardView v)
        {
            if (v == null || playerTurn == null) return;
            playerTurn.TryPlay(v.Card); // 성공하면 Hand.Changed → Rebuild
        }

        void ToLocal(PointerEventData e, out Vector2 local)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, e.position, e.pressEventCamera, out local);
            // container 아래 가운데 기준으로
            var r = container.rect;
            local -= new Vector2(r.center.x, r.yMin);
        }
    }
}
