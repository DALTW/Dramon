using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>드래곤 머리 위 다음 행동 예고: 아이콘 + 수치 (배율·강화가 반영된 값)</summary>
    public class DragonIntentUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] DragonIntentPicker intent;
        [Tooltip("배율·강화가 반영된 피해 계산")]
        [SerializeField] DragonActionExecutor executor;
        [Tooltip("강화 보너스가 바뀌면 다시 써요")]
        [SerializeField] DragonBattleStats dragonStats;
        [Tooltip("결투 중일 때만 보여요")]
        [SerializeField] CardBattleController controller;
        [Tooltip("켜고 끌 묶음 (아이콘 + 숫자)")]
        [SerializeField] GameObject root;
        [SerializeField] Image icon;
        [SerializeField] Text valueLabel;

        [Header("행동별 아이콘 (Art/Battle/Cards/Icons/Icon_Intent_*)")]
        [SerializeField] Sprite clawIcon;
        [Tooltip("비우면 할퀴기 아이콘")]
        [SerializeField] Sprite multiHitIcon;
        [SerializeField] Sprite breathIcon;
        [SerializeField] Sprite defendIcon;
        [SerializeField] Sprite healIcon;
        [SerializeField] Sprite buffIcon;

        void OnEnable()
        {
            if (intent != null) intent.Changed += Refresh;
            if (dragonStats != null) dragonStats.Changed += Refresh;
            if (controller != null) controller.StateChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (intent != null) intent.Changed -= Refresh;
            if (dragonStats != null) dragonStats.Changed -= Refresh;
            if (controller != null) controller.StateChanged -= Refresh;
        }

        void Refresh()
        {
            var action = intent != null ? intent.Current : null;
            bool show = action != null && (controller == null || controller.IsRunning);
            if (root != null) root.SetActive(show);
            if (!show) return;
            if (icon != null) icon.sprite = IconFor(action.Type);
            if (valueLabel != null)
            {
                int v = executor != null ? executor.DamageOf(action) : action.Value;
                switch (action.Type)
                {
                    case DragonActionType.MultiHit: valueLabel.text = $"{v}×{action.Hits}"; break;
                    case DragonActionType.Defend:
                    case DragonActionType.Heal:
                    case DragonActionType.Buff: valueLabel.text = $"+{v}"; break;
                    default: valueLabel.text = v.ToString(); break;
                }
            }
        }

        Sprite IconFor(DragonActionType type)
        {
            switch (type)
            {
                case DragonActionType.MultiHit: return multiHitIcon != null ? multiHitIcon : clawIcon;
                case DragonActionType.Breath: return breathIcon;
                case DragonActionType.Defend: return defendIcon;
                case DragonActionType.Heal: return healIcon;
                case DragonActionType.Buff: return buffIcon;
                default: return clawIcon;
            }
        }
    }
}
