using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Dramon
{
    /// <summary>
    /// 상점 창 열기·닫기와 탭 전환. 검 사냥 단계에서만 열리고, 열려 있는 동안 단계는 "상점"(보드·둥지 클릭 막힘).
    /// [닫기] 버튼이나 Esc로 닫으면 검 사냥으로 돌아간다. 다시 시작 등으로 단계가 바뀌면 창을 바로 닫는다.
    /// </summary>
    public class ShopWindowUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("게임 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("상점 창 전체 (자식 Panel, 평소에는 꺼져 있음)")]
        [SerializeField] GameObject panel;
        [Tooltip("확인 창 (열려 있으면 Esc가 확인 창부터 닫아요)")]
        [SerializeField] ShopConfirmDialog confirmDialog;

        [Header("탭 (버튼과 페이지는 같은 순서)")]
        [Tooltip("탭 버튼들: 재료 판매 / 능력 강화 / 아이템 구매 (각 버튼 OnClick → SelectTab(번호))")]
        [SerializeField] Button[] tabButtons = new Button[0];
        [Tooltip("탭 페이지들 (버튼과 같은 순서)")]
        [SerializeField] GameObject[] tabPages = new GameObject[0];
        [Tooltip("상점을 열 때 처음 보여 줄 탭 번호 (0부터)")]
        [Min(0)]
        [SerializeField] int firstTab = 0;
        [Tooltip("선택된 탭 버튼 색")]
        [SerializeField] Color selectedTabColor = Color.white;
        [Tooltip("선택되지 않은 탭 버튼 색")]
        [SerializeField] Color idleTabColor = new Color(0.72f, 0.66f, 0.56f, 1f);

        [Header("키")]
        [Tooltip("켜면 Esc 키로 상점을 닫아요")]
        [SerializeField] bool closeWithEscape = true;

        [Header("테스트 (에디터에서만, Play 중 Inspector 버튼)")]
        [Tooltip("테스트로 돈을 넣을 지갑")]
        [SerializeField] PlayerWallet testWallet;
        [Tooltip("테스트로 재료를 넣을 인벤토리")]
        [SerializeField] PlayerInventory testInventory;
        [Tooltip("테스트로 초기화할 강화 단계")]
        [SerializeField] PlayerUpgrades testUpgrades;
        [Tooltip("재료 30종 목록을 가져올 상점 목록")]
        [SerializeField] ShopCatalog testCatalog;
        [Tooltip("[돈 넣기] 버튼으로 넣을 돈")]
        [SerializeField] int testMoney = 500;
        [Tooltip("[재료 넣기] 버튼으로 재료마다 넣을 개수")]
        [SerializeField] int testMaterialCount = 3;

        int currentTab;

        public bool IsOpen => panel != null && panel.activeSelf;

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        /// <summary>상점 열기 (HUD 상점 버튼의 OnClick에 연결됨). 검 사냥 단계에서만 열려요.</summary>
        public void Open()
        {
            if (IsOpen || phaseController == null) return;
            if (!phaseController.EnterShop()) return;
            if (panel != null) panel.SetActive(true);
            SelectTab(firstTab);
        }

        /// <summary>상점 닫기 ([닫기] 버튼·Esc) → 검 사냥으로</summary>
        public void Close()
        {
            if (!IsOpen) return;
            if (confirmDialog != null) confirmDialog.Cancel();
            if (panel != null) panel.SetActive(false);
            if (phaseController != null) phaseController.LeaveShop();
        }

        public void SelectTab(int index)
        {
            if (tabPages.Length == 0) return;
            currentTab = Mathf.Clamp(index, 0, tabPages.Length - 1);
            for (int i = 0; i < tabPages.Length; i++)
                if (tabPages[i] != null) tabPages[i].SetActive(i == currentTab);
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] == null) continue;
                var image = tabButtons[i].targetGraphic;
                if (image != null) image.color = i == currentTab ? selectedTabColor : idleTabColor;
            }
        }

        void Update()
        {
            if (!IsOpen || !closeWithEscape) return;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;
            if (confirmDialog != null && confirmDialog.IsOpen) confirmDialog.Cancel();
            else Close();
#endif
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            // 다시 시작 등으로 상점 단계가 아니게 되면 창만 닫음
            if (phase == GamePhase.Shop || !IsOpen) return;
            if (confirmDialog != null) confirmDialog.Cancel();
            if (panel != null) panel.SetActive(false);
        }

        // ── 테스트 (에디터에서만) ─────────────
        [ContextMenu("테스트/돈 넣기")]
        public void TestAddMoney()
        {
            if (!Application.isEditor || testWallet == null) return;
            testWallet.Add(testMoney);
        }

        [ContextMenu("테스트/재료 30종 넣기")]
        public void TestAddMaterials()
        {
            if (!Application.isEditor || testInventory == null || testCatalog == null) return;
            var loot = new System.Collections.Generic.List<LootEntry>();
            foreach (var m in testCatalog.MaterialOrder)
                if (m != null) loot.Add(new LootEntry(m, testMaterialCount));
            testInventory.AddAll(loot);
        }

        [ContextMenu("테스트/강화 초기화")]
        public void TestResetUpgrades()
        {
            if (!Application.isEditor || testUpgrades == null) return;
            testUpgrades.ResetAll();
        }
    }
}
