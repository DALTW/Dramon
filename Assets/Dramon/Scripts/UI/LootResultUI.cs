using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 결투 승리 후 "전리품" 창: 얻은 재료 아이콘·이름을 나열하고, 메시지 창에 드랍 문구를 보여 준다.
    /// [확인]을 누르면 창을 닫고 다음 단계(숲으로 돌아가기)를 진행한다. 재료 줄은 LootRow Prefab으로 만든다.
    /// </summary>
    public class LootResultUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("게임 단계 관리자 (게임 오버·다시 시작이면 창을 닫아요)")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("전리품 창 (평소에는 꺼져 있음)")]
        [SerializeField] GameObject window;
        [Tooltip("재료 줄이 들어갈 곳 (Vertical Layout Group)")]
        [SerializeField] RectTransform listRoot;
        [Tooltip("재료 한 줄 Prefab (아이콘·이름·개수)")]
        [SerializeField] LootRowView rowPrefab;
        [Tooltip("아무것도 못 얻었을 때 보여 줄 글 (비워도 됨)")]
        [SerializeField] GameObject emptyLabel;
        [Tooltip("[확인] 버튼 (누르면 Confirm 호출)")]
        [SerializeField] Button confirmButton;
        [Tooltip("드랍 문구를 보여 줄 메시지 창 (결투 화면의 메시지 창)")]
        [SerializeField] EncounterMessageUI message;

        [Header("문구")]
        [Tooltip("드랍 문구. {0} = 드래곤 이름, {1} = 얻은 재료 목록")]
        [SerializeField] string dropMessageFormat = "{0}이(가) {1}을(를) 떨어뜨렸다!";
        [Tooltip("재료가 2개 이상일 때 목록에 쓰는 형식. {0} = 재료 이름, {1} = 개수")]
        [SerializeField] string multiCountFormat = "{0} {1}개";
        [Tooltip("재료 이름 사이에 넣을 글자")]
        [SerializeField] string separator = ", ";
        [Tooltip("아무것도 안 나왔을 때 문구. {0} = 드래곤 이름")]
        [SerializeField] string emptyMessageFormat = "{0}은(는) 아무것도 떨어뜨리지 않았다...";

        [Header("확인 버튼")]
        [Tooltip("켜면 메시지가 다 나온 뒤에 [확인]을 누를 수 있어요")]
        [SerializeField] bool waitForMessage = true;

        readonly List<LootRowView> rows = new List<LootRowView>();
        Action onConfirm;

        public bool IsOpen => window != null && window.activeSelf;

        void Awake() => HideWindow();

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        /// <summary>전리품 창을 띄운다. [확인]을 누르면 onConfirmed 호출.</summary>
        public void Show(DragonSpecies species, IReadOnlyList<LootEntry> loot, Action onConfirmed)
        {
            StopAllCoroutines();
            onConfirm = onConfirmed;
            ClearRows();

            int shown = 0;
            if (loot != null && rowPrefab != null && listRoot != null)
            {
                foreach (var entry in loot)
                {
                    if (entry.material == null || entry.count <= 0) continue;
                    var row = Instantiate(rowPrefab, listRoot);
                    row.Set(entry.material, entry.count);
                    rows.Add(row);
                    shown++;
                }
            }
            if (emptyLabel != null) emptyLabel.SetActive(shown == 0);
            if (window != null) window.SetActive(true);

            string dragonName = species != null ? species.DisplayName : "드래곤";
            string text = shown > 0
                ? string.Format(dropMessageFormat, dragonName, BuildList(loot))
                : string.Format(emptyMessageFormat, dragonName);
            StartCoroutine(ShowMessage(text));
        }

        /// <summary>[확인] 버튼: 창을 닫고 다음으로.</summary>
        public void Confirm()
        {
            if (!IsOpen) return;
            var next = onConfirm;
            onConfirm = null;
            StopAllCoroutines();
            HideWindow();
            if (message != null) message.Hide();
            next?.Invoke();
        }

        IEnumerator ShowMessage(string text)
        {
            SetConfirmInteractable(!waitForMessage || message == null);
            if (message != null) yield return message.Show(text);
            SetConfirmInteractable(true);
        }

        string BuildList(IReadOnlyList<LootEntry> loot)
        {
            var sb = new StringBuilder();
            foreach (var entry in loot)
            {
                if (entry.material == null || entry.count <= 0) continue;
                if (sb.Length > 0) sb.Append(separator);
                sb.Append(entry.count > 1
                    ? string.Format(multiCountFormat, entry.material.DisplayName, entry.count)
                    : entry.material.DisplayName);
            }
            return sb.ToString();
        }

        void SetConfirmInteractable(bool on)
        {
            if (confirmButton != null) confirmButton.interactable = on;
        }

        void HideWindow()
        {
            ClearRows();
            if (window != null) window.SetActive(false);
        }

        void ClearRows()
        {
            foreach (var row in rows)
                if (row != null) Destroy(row.gameObject);
            rows.Clear();
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.GameOver && phase != GamePhase.LemonMarking) return;
            onConfirm = null;
            StopAllCoroutines();
            HideWindow();
        }
    }
}
