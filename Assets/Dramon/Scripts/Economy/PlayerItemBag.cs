using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 상점에서 산 결투용 아이템을 아이템별 개수로 보관한다. (효과는 카드 결투 단계에서 사용)
    /// </summary>
    public class PlayerItemBag : MonoBehaviour
    {
        /// <summary>아이템과 개수 (Inspector 확인용 목록에도 씀)</summary>
        [Serializable]
        public class Entry
        {
            public ShopItemData item;
            public int count;
        }

        [Header("연결")]
        [Tooltip("다시 시작을 알려 주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        [Header("초기화")]
        [Tooltip("켜면 다시 시작(Restart)할 때 가진 아이템을 비워요")]
        [SerializeField] bool clearOnRestart = true;

        [Header("확인용 (실행 중 보유 아이템)")]
        [Tooltip("실행 중 가진 아이템이 여기에 보여요. 확인용이라 여기서 고쳐도 게임에는 반영되지 않아요")]
        [SerializeField] List<Entry> currentItems = new List<Entry>();

        /// <summary>아이템 개수가 바뀔 때</summary>
        public event Action Changed;

        readonly Dictionary<ShopItemData, int> counts = new Dictionary<ShopItemData, int>();

        void Awake() => currentItems.Clear();

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += HandleRestarted;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= HandleRestarted;
        }

        public int GetCount(ShopItemData item) =>
            item != null && counts.TryGetValue(item, out int n) ? n : 0;

        /// <summary>더 가질 수 있는 개수 (최대 보유 개수 기준, 제한 없으면 int.MaxValue)</summary>
        public int RoomFor(ShopItemData item)
        {
            if (item == null) return 0;
            return item.MaxOwned <= 0 ? int.MaxValue : Mathf.Max(0, item.MaxOwned - GetCount(item));
        }

        public void Add(ShopItemData item, int count)
        {
            if (item == null || count <= 0) return;
            counts[item] = GetCount(item) + count;
            Mirror();
            Changed?.Invoke();
        }

        /// <summary>아이템을 쓴다/뺀다. 모자라면 false. (카드 결투 단계용)</summary>
        public bool TryRemove(ShopItemData item, int count)
        {
            if (item == null || count <= 0 || GetCount(item) < count) return false;
            int left = GetCount(item) - count;
            if (left > 0) counts[item] = left;
            else counts.Remove(item);
            Mirror();
            Changed?.Invoke();
            return true;
        }

        public void Clear()
        {
            if (counts.Count == 0) return;
            counts.Clear();
            Mirror();
            Changed?.Invoke();
        }

        void HandleRestarted()
        {
            if (clearOnRestart) Clear();
        }

        void Mirror()
        {
            currentItems.Clear();
            foreach (var pair in counts) currentItems.Add(new Entry { item = pair.Key, count = pair.Value });
        }
    }
}
