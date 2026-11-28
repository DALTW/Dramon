using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>체력 창 옆 상태 아이콘 줄: 상태가 있는 것만 정해진 순서대로 아이콘 + 수치</summary>
    public class StatusIconsUI : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public StatusType type;
            [Tooltip("Art/Battle/Cards/Icons/Icon_Status_*")]
            public Sprite icon;
        }

        [Header("연결")]
        [Tooltip("보여 줄 상태 효과 (플레이어 또는 드래곤)")]
        [SerializeField] StatusEffects source;
        [Tooltip("아이콘 한 칸 Prefab")]
        [SerializeField] StatusIconView iconPrefab;
        [Tooltip("아이콘이 놓일 곳 (가로 정렬)")]
        [SerializeField] RectTransform container;

        [Header("상태별 아이콘 (이 순서로 보여요)")]
        [SerializeField] List<Entry> icons = new List<Entry>();

        readonly List<StatusIconView> views = new List<StatusIconView>();

        void OnEnable()
        {
            if (source != null) source.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (source != null) source.Changed -= Refresh;
        }

        void Refresh()
        {
            if (iconPrefab == null || container == null) return;
            int used = 0;
            foreach (var e in icons)
            {
                int n = source != null ? source.Get(e.type) : 0;
                if (n <= 0) continue;
                if (used >= views.Count) views.Add(Instantiate(iconPrefab, container));
                var v = views[used++];
                v.gameObject.SetActive(true);
                v.Show(e.type, e.icon, n);
            }
            for (int i = used; i < views.Count; i++) views[i].gameObject.SetActive(false);
        }
    }
}
