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

        [Header("연결 — 상태·배율 (v3)")]
        [Tooltip("드래곤 체력·공격 배율 (스테이지·결투 순서·보스)")]
        [SerializeField] DragonScaling scaling;
        [Tooltip("스테이지별 최대 에너지·뽑기 장수. 비우면 BattleRules 값")]
        [SerializeField] StageRules stageRules;
        [Tooltip("화상·중독 등 내 턴 끝 처리")]
        [SerializeField] StatusEffectProcessor statusProcessor;
        [SerializeField] StatusEffects playerStatus;
        [SerializeField] StatusEffects dragonStatus;
        [Tooltip("체력 증가 강화를 시작 체력에 더해요")]
        [SerializeField] PlayerUpgrades playerUpgrades;
        [Tooltip("검사 저체력 표시 등에 씀")]
        [SerializeField] HeroBattlerView hero;

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
        [Tooltip("내 턴 끝에 화상·중독 문구를 보여 주는 시간(초)")]
        [Min(0f)]
        [SerializeField] float statusTickDelay = 0.6f;
        [Tooltip("체력이 이 비율 아래면 검사가 지친 모습 (0이면 끔)")]
        [Range(0f, 1f)]
        [SerializeField] float lowHealthRatio = 0.3f;
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
        /// <summary>이번 결투 최대 에너지</summary>
        public int MaxEnergy => stageRules != null && scaling != null ? stageRules.Get(scaling.Stage).maxEnergy : rules != null ? rules.EnergyPerTurn : 3;
        /// <summary>이번 결투 턴마다 뽑는 장수</summary>
        public int DrawPerTurn => stageRules != null && scaling != null ? stageRules.Get(scaling.Stage).draw : rules != null ? rules.DrawPerTurn : 5;

        /// <summary>결투 상태가 바뀔 때 (시작·끝·턴 변경)</summary>
        public event Action StateChanged;

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
            if (playerStats != null) playerStats.Died += HandlePlayerDied;
            if (dragonStats != null) dragonStats.Died += HandleDragonDied;
            if (playerStats != null) playerStats.Changed += HandlePlayerChanged;
            HideUI();
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
            if (playerStats != null) playerStats.Died -= HandlePlayerDied;
            if (dragonStats != null) dragonStats.Died -= HandleDragonDied;
            if (playerStats != null) playerStats.Changed -= HandlePlayerChanged;
        }

        /// <summary>카드 결투 시작 (등장 연출이 끝난 뒤 DuelSession이 호출)</summary>
        public void Begin(DragonSpecies species)
        {
            Abort();
            running = true;
            Turn = 0;

            int bonusHealth = playerUpgrades != null ? playerUpgrades.GetBonus(UpgradeStat.MaxHealth) : 0;
            playerStats.ResetForDuel((rules != null ? rules.StartHealth : 50) + bonusHealth);
            dragonStats.ResetForDuel(scaling != null ? scaling.MaxHealth(species) : species != null ? species.MaxHealth : 1);
            if (playerStatus != null) playerStatus.ClearAll();
            if (dragonStatus != null) dragonStatus.ClearAll();
            playerTurn.ResetForDuel();
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
                playerTurn.BeginTurn(MaxEnergy, DrawPerTurn);
                StateChanged?.Invoke();
                while (running && !endTurnRequested) yield return null;
                if (!running) yield break;

                playerTurn.EndTurn();
                StateChanged?.Invoke();
                if (statusProcessor != null)
                {
                    var lines = statusProcessor.OnPlayerTurnEnd(); // 화상·중독
                    if (lines.Count > 0)
                    {
                        if (log != null) log.Show(string.Join(", ", lines));
                        if (statusTickDelay > 0f) yield return new WaitForSeconds(statusTickDelay);
                    }
                    if (!running) yield break;
                }
                yield return dragonTurn.Run();
            }
        }

        void ShowIntent()
        {
            var action = intent.Current;
            var species = intent.Species;
            if (log == null || action == null || species == null) return;
            int value = dragonActions.DamageOf(action);
            string amount = action.Type == DragonActionType.MultiHit ? $"{value}×{action.Hits}" : value.ToString();
            log.Show(string.Format(intentFormat, species.DisplayName, action.DisplayName, amount));
        }

        void HandlePlayerChanged()
        {
            if (hero == null || playerStats == null || !running) return;
            hero.SetLowHealth(lowHealthRatio > 0f && playerStats.Health < playerStats.MaxHealth * lowHealthRatio);
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

        /// <summary>테스트: 플레이어에게 상태 효과 주기</summary>
        public void TestAddPlayerStatus(StatusType type, int amount)
        {
            if (Application.isEditor && running && playerStatus != null) playerStatus.Add(type, amount);
        }

        /// <summary>테스트: 드래곤에게 산미 주기</summary>
        public void TestAddDragonSour(int amount)
        {
            if (Application.isEditor && running && dragonStatus != null) dragonStatus.Add(StatusType.Sour, amount);
        }

        public void TestToggleTempButtons()
        {
            if (!Application.isEditor || duelScreen == null) return;
            duelScreen.TestShowTempButtons = !duelScreen.TestShowTempButtons;
            duelScreen.SetButtonsVisible(running || (session != null && session.InDuel));
        }
    }
}
