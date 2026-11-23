using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dramon
{
    /// <summary>도구 전환 입력(기본 R)을 받아 레몬 ↔ 검을 바꾸고, 바뀌었다는 이벤트를 알린다.</summary>
    public class ToolSwitcher : MonoBehaviour
    {
        [Header("사용 여부")]
        [Tooltip("끄면 R키 도구 전환이 동작하지 않아요. (레몬 표시 → 검 사냥 설계로 바뀌면서 꺼 둠)")]
        [SerializeField] bool switchingEnabled = true;

        [Header("도구")]
        [Tooltip("게임을 시작하거나 다시 시작할 때 들고 있을 도구")]
        [SerializeField] PlayerTool startingTool = PlayerTool.Lemon;

        [Header("입력")]
        [Tooltip("도구를 바꾸는 입력. 'Use Reference'를 끄면 여기서 키를 바로 바꿀 수 있고, 켜면 Input Action 에셋의 액션을 연결할 수 있어요")]
        [SerializeField] InputActionProperty switchAction = new InputActionProperty(
            new InputAction("SwitchTool", InputActionType.Button, "<Keyboard>/r"));

        [Header("연결")]
        [Tooltip("탐험 단계를 확인할 상태 관리자. 결투 중·게임 오버·클리어 때는 전환이 막혀요")]
        [SerializeField] ExplorationState explorationState;

        public event Action<PlayerTool> ToolChanged;

        public PlayerTool CurrentTool { get; private set; }

        void Awake()
        {
            CurrentTool = startingTool;
        }

        void OnEnable()
        {
            var action = switchAction.action;
            if (action != null)
            {
                action.performed += HandleSwitchPerformed;
                action.Enable();
            }
            if (explorationState != null) explorationState.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            var action = switchAction.action;
            if (action != null) action.performed -= HandleSwitchPerformed;
            if (explorationState != null) explorationState.PhaseChanged -= HandlePhaseChanged;
        }

        /// <summary>HUD 안내 문구용: 지금 연결된 키 이름 (예: "R")</summary>
        public string GetBindingDisplayString()
        {
            var action = switchAction.action;
            return action != null ? action.GetBindingDisplayString() : "-";
        }

        public bool CanSwitch =>
            switchingEnabled && (explorationState == null ||
            explorationState.Phase == ExplorationPhase.Ready ||
            explorationState.Phase == ExplorationPhase.Exploring);

        public void SetTool(PlayerTool tool)
        {
            if (tool == CurrentTool) return;
            CurrentTool = tool;
            ToolChanged?.Invoke(tool);
        }

        void HandleSwitchPerformed(InputAction.CallbackContext _)
        {
            if (!CanSwitch) return;
            SetTool(CurrentTool == PlayerTool.Lemon ? PlayerTool.Sword : PlayerTool.Lemon);
        }

        void HandlePhaseChanged(ExplorationPhase phase)
        {
            if (phase == ExplorationPhase.Ready) SetTool(startingTool);
        }
    }
}
