using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 상점에서 산 능력 강화 단계를 보관하고, 능력 종류별 최종 보너스를 알려 준다.
    /// (카드 결투 단계에서 GetBonus로 가져다 쓸 예정. 지금은 보관·표시만)
    /// </summary>
    public class PlayerUpgrades : MonoBehaviour
    {
        /// <summary>강화 하나의 단계 (Inspector 확인용 목록에도 씀)</summary>
        [Serializable]
        public class Entry
        {
            public ShopUpgradeData upgrade;
            public int level;
        }

        [Header("연결")]
        [Tooltip("다시 시작을 알려 주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        [Header("초기화")]
        [Tooltip("켜면 다시 시작(Restart)할 때 강화 단계를 모두 0으로 되돌려요")]
        [SerializeField] bool clearOnRestart = true;

        [Header("확인용 (실행 중 강화 단계)")]
        [Tooltip("실행 중 강화 단계가 여기에 보여요. 확인용이라 여기서 고쳐도 게임에는 반영되지 않아요")]
        [SerializeField] List<Entry> currentLevels = new List<Entry>();

        /// <summary>강화 단계가 바뀔 때</summary>
        public event Action Changed;

        readonly Dictionary<ShopUpgradeData, int> levels = new Dictionary<ShopUpgradeData, int>();
        readonly Dictionary<UpgradeStat, int> statCaps = new Dictionary<UpgradeStat, int>();

        void Awake() => currentLevels.Clear();

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += HandleRestarted;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= HandleRestarted;
        }

        public int GetLevel(ShopUpgradeData upgrade) =>
            upgrade != null && levels.TryGetValue(upgrade, out int n) ? n : 0;

        public bool IsMaxed(ShopUpgradeData upgrade) => upgrade != null && GetLevel(upgrade) >= MaxLevelOf(upgrade);

        /// <summary>이 강화의 최대 단계: 모드가 정한 상한이 있으면 그 값, 없으면 강화 데이터의 Max Level</summary>
        public int MaxLevelOf(ShopUpgradeData upgrade)
        {
            if (upgrade == null) return 0;
            return statCaps.TryGetValue(upgrade.Stat, out int cap) && cap > 0 ? cap : upgrade.MaxLevel;
        }

        /// <summary>모드별 상한 (예: 일반 모드 체력 증가 5단계, 무한 모드 50단계). 0이면 강화 데이터 값</summary>
        public void SetStatLevelCap(UpgradeStat stat, int cap)
        {
            statCaps[stat] = Mathf.Max(0, cap);
            Changed?.Invoke();
        }

        /// <summary>한 단계 올린다 (돈 계산은 UpgradePurchaser가 함). 최대 단계면 false.</summary>
        public bool LevelUp(ShopUpgradeData upgrade)
        {
            if (upgrade == null || IsMaxed(upgrade)) return false;
            levels[upgrade] = GetLevel(upgrade) + 1;
            Mirror();
            Changed?.Invoke();
            return true;
        }

        /// <summary>능력 종류별 최종 보너스 (모든 강화의 합)</summary>
        public int GetBonus(UpgradeStat stat)
        {
            int sum = 0;
            foreach (var pair in levels)
                if (pair.Key != null && pair.Key.Stat == stat) sum += pair.Key.BonusAt(pair.Value);
            return sum;
        }

        /// <summary>모든 강화를 0단계로</summary>
        public void ResetAll()
        {
            if (levels.Count == 0) return;
            levels.Clear();
            Mirror();
            Changed?.Invoke();
        }

        void HandleRestarted()
        {
            if (clearOnRestart) ResetAll();
        }

        void Mirror()
        {
            currentLevels.Clear();
            foreach (var pair in levels) currentLevels.Add(new Entry { upgrade = pair.Key, level = pair.Value });
        }
    }
}
