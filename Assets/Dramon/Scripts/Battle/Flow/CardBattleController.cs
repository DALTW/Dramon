using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 카드 결투 진행: 결투 시작(체력·덱 준비) → 내 턴 ↔ 드래곤 턴 반복 → 드래곤 체력 0이면 승리, 내 체력 0이면 패배.
    /// 등장 연출이 끝나면 DuelSession이 Begin을 부르고, 끝나면 DuelSession.Win / Lose를 부른다.
    /// </summary>
    public class CardBattleController : MonoBehaviour
    {
        [Header("연결 — 진행")]
        [SerializeField] BattleRules rules;
        [SerializeField] DuelSession session;
        [Tooltip("게임 오버·다시 시작이면 결투를 멈춰요")]
        [SerializeField] GamePhaseController phaseController;
        [SerializeField] PlayerTurn playerTurn;
        [SerializeField] DragonTurn dragonTurn;
        [SerializeField] DragonIntentPicker intent;
        [SerializeField] DragonActionExecutor dragonActions;

        [Header("연결 — 덱·수치")]
        [SerializeField] PlayerDeck playerDeck;
        [SerializeField] DrawPile drawPile;
        [SerializeField] DiscardPile discardPile;
        [SerializeField] ExhaustPile exhaustPile;
        [SerializeField] Hand hand;
        [SerializeField] PlayerBattleStats playerStats;
        [SerializeField] DragonBattleStats dragonStats;

        [Header("연결 — 화면")]
        [Tooltip("카드 결투 화면 묶음 (손패·에너지·더미·턴 종료, 평소에는 꺼짐)")]
        [SerializeField] GameObject battleRoot;
        [SerializeField] PlayerHealthUI playerHealthUI;
        [SerializeField] BattleLogMessage log;
        [Tooltip("임시 승패 버튼을 테스트로 보일 때 씀")]
        [SerializeField] DuelScreenUI duelScreen;

        [Header("진행")]
        [Tooltip("켜면 등장 연출이 끝난 뒤 카드 결투를 시작해요. 끄면 예전처럼 임시 승패 버튼만")]
        [SerializeField] bool useCardBattle = true;
        [Tooltip("내 턴이 시작되기 전 잠깐 기다리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float turnStartDelay = 0.2f;
        [Tooltip("드래곤이 쓰러진 뒤 승리 처리까지(초)")]
        [Min(0f)]
        [SerializeField] float endDelay = 0.6f;

        [Header("문구")]
        [Tooltip("내 턴 시작. {0} = 드래곤 이름, {1} = 다음 행동, {2} = 수치")]
        [SerializeField] string intentFormat = "{0}이(가) {1} {2}을(를) 준비한다";

        [Header("테스트 (에디터에서만, Play 중 Inspector 버튼)")]
        [Tooltip("[손패에 넣기] 버튼으로 넣을 카드")]
        [SerializeField] CardData testCard;

        bool running;
        bool endTurnRequested;
        Coroutine loop;

        public bool UseCardBattle => useCardBattle;
        public bool IsRunning => running;
        public bool IsPlayerTurn => running && playerTurn != null && playerTurn.IsActive;
        public int Turn { get; private set; }

        /// <summary>결투 상태가 바뀔 때 (시작·끝·턴 변경)</summary>
        public event Action StateChanged;

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
            if (playerStats != null) playerStats.Died += HandlePlayerDied;
            if (dragonStats != null) dragonStats.Died += HandleDragonDied;
            HideUI();
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
            if (playerStats != null) playerStats.Died -= HandlePlayerDied;
            if (dragonStats != null) dragonStats.Died -= HandleDragonDied;
        }

        /// <summary>카드 결투 시작 (등장 연출이 끝난 뒤 DuelSession이 호출)</summary>
        public void Begin(DragonSpecies species)
        {
            Abort();
            running = true;
            Turn = 0;

            playerStats.ResetForDuel(rules != null ? rules.StartHealth : 50);
            dragonStats.ResetForDuel(species != null ? species.MaxHealth : 1);
            intent.ResetFor(species);

            drawPile.Clear();
            discardPile.Clear();
            exhaustPile.Clear();
            hand.Clear();
            var cards = new List<CardInstance>();
            foreach (var data in playerDeck.BuildCardList()) cards.Add(new CardInstance(data));
            drawPile.AddAndShuffle(cards);

            if (battleRoot != null) battleRoot.SetActive(true);
            if (playerHealthUI != null) playerHealthUI.Show();
            loop = StartCoroutine(Loop());
            StateChanged?.Invoke();
        }

        /// <summary>[턴 종료] 버튼</summary>
        public void RequestEndTurn()
        {
            if (IsPlayerTurn) endTurnRequested = true;
        }

        IEnumerator Loop()
        {
            while (running)
            {
                Turn++;
                if (turnStartDelay > 0f) yield return new WaitForSeconds(turnStartDelay);
                ShowIntent();
                endTurnRequested = false;
                playerTurn.BeginTurn(rules != null ? rules.EnergyPerTurn : 3, rules != null ? rules.DrawPerTurn : 5);
                StateChanged?.Invoke();
                while (running && !endTurnRequested) yield return null;
                if (!running) yield break;

                playerTurn.EndTurn();
                StateChanged?.Invoke();
                yield return dragonTurn.Run();
            }
        }

        void ShowIntent()
        {
            var action = intent.Current;
            var species = intent.Species;
            if (log == null || action == null || species == null) return;
            int value = action.IsAttack ? dragonActions.DamageOf(action) : action.Value;
            string amount = action.Type == DragonActionType.MultiHit ? $"{value}×{action.Hits}" : value.ToString();
            log.Show(string.Format(intentFormat, species.DisplayName, action.DisplayName, amount));
        }

        void HandleDragonDied()
        {
            if (!running) return;
            StartCoroutine(Finish(true));
        }

        void HandlePlayerDied()
        {
            if (!running) return;
            StartCoroutine(Finish(false));
        }

        IEnumerator Finish(bool won)
        {
            running = false;
            if (loop != null) StopCoroutine(loop);
            loop = null;
            playerTurn.Stop();
            StateChanged?.Invoke();
            if (endDelay > 0f) yield return new WaitForSeconds(endDelay);
            HideUI();
            ClearPiles();
            if (session == null) yield break;
            if (won) session.Win();
            else session.Lose();
        }

        /// <summary>결투를 바로 멈춘다 (게임 오버·다시 시작·숲으로)</summary>
        public void Abort()
        {
            StopAllCoroutines();
            loop = null;
            bool was = running;
            running = false;
            if (playerTurn != null) playerTurn.Stop();
            HideUI();
            ClearPiles();
            if (was) StateChanged?.Invoke();
        }

        void HideUI()
        {
            if (battleRoot != null) battleRoot.SetActive(false);
            if (playerHealthUI != null) playerHealthUI.Hide();
        }

        void ClearPiles()
        {
            if (hand != null) hand.Clear();
            if (drawPile != null) drawPile.Clear();
            if (discardPile != null) discardPile.Clear();
            if (exhaustPile != null) exhaustPile.Clear();
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.GameOver || phase == GamePhase.LemonMarking || phase == GamePhase.SwordHunt) Abort();
        }

        // ── 테스트 (에디터에서만) ─────────────
        public void TestDragonHealthToOne()
        {
            if (Application.isEditor && running) dragonStats.SetHealth(1);
        }

        public void TestToggleInvincible()
        {
            if (Application.isEditor) playerStats.TestInvincible = !playerStats.TestInvincible;
        }

        public void TestEnergy99()
        {
            if (Application.isEditor && running) playerStats.AddEnergy(99 - playerStats.Energy);
        }

        public void TestAddCardToHand()
        {
            if (!Application.isEditor || !running || testCard == null || hand.IsFull) return;
            hand.Add(new CardInstance(testCard));
        }

        public void TestToggleTempButtons()
        {
            if (!Application.isEditor || duelScreen == null) return;
            duelScreen.TestShowTempButtons = !duelScreen.TestShowTempButtons;
            duelScreen.SetButtonsVisible(running || (session != null && session.InDuel));
        }
    }
}
