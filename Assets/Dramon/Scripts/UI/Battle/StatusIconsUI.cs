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

        [Header("플레이어 방어 효과 (플레이어 줄에만 연결)")]
        [Tooltip("반격 자세(반사)·연막 구르기(무효)를 보여 줄 플레이어 능력치. 드래곤 줄은 비워 둬요")]
        [SerializeField] PlayerBattleStats guardSource;
        [Tooltip("반격 자세: 맞을 때마다 되돌려 주는 피해 (Icon_Status_Reflect)")]
        [SerializeField] Sprite reflectIcon;
        [Tooltip("연막 구르기: 무효로 막을 남은 공격 수 (Icon_Status_Negate)")]
        [SerializeField] Sprite negateIcon;

        readonly List<StatusIconView> views = new List<StatusIconView>();

        void OnEnable()
        {
            if (source != null) source.Changed += Refresh;
            if (guardSource != null) guardSource.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (source != null) source.Changed -= Refresh;
            if (guardSource != null) guardSource.Changed -= Refresh;
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
            if (guardSource != null)
            {
                used = ShowExtra(used, reflectIcon, guardSource.Reflect);
                used = ShowExtra(used, negateIcon, guardSource.NegateCharges);
            }
            for (int i = used; i < views.Count; i++) views[i].gameObject.SetActive(false);
        }

        /// <summary>상태 효과가 아닌 아이콘 (반사·무효) 한 칸. 보여 줬으면 다음 칸 번호를 돌려준다</summary>
        int ShowExtra(int used, Sprite sprite, int count)
        {
            if (sprite == null || count <= 0) return used;
            if (used >= views.Count) views.Add(Instantiate(iconPrefab, container));
            var v = views[used];
            v.gameObject.SetActive(true);
            v.Show(StatusType.None, sprite, count);
            return used + 1;
        }
    }
}
