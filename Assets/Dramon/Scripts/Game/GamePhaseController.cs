using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>게임 진행 단계</summary>
    public enum GamePhase
    {
        /// <summary>칸을 열고 굴에 레몬을 놓는 단계</summary>
        LemonMarking,
        /// <summary>굴을 모두 표시함 — 완료 안내 창이 떠 있는 동안</summary>
        MarkingComplete,
        /// <summary>검으로 봉인된 굴을 골라 결투하는 단계 (다음 단계에서 구현)</summary>
        SwordHunt,
        GameOver,
        /// <summary>검으로 봉인된 굴에 들어가 드래곤과 결투하는 중 (임시 승패 버튼이 보이는 상태)</summary>
        Duel,
        /// <summary>봉인된 굴에 들어가 드래곤 등장 연출이 진행되는 중</summary>
        Encounter,
        /// <summary>검 사냥 중 상점 창을 연 상태 (보드·둥지 클릭이 막힘)</summary>
        Shop,
        /// <summary>깨어난 둥지의 드래곤을 모두 쓰러뜨림 — 스테이지 클리어 창이 떠 있는 동안</summary>
        StageClear,
        /// <summary>일반 모드 보스를 쓰러뜨림 — 엔딩 화면이 떠 있는 동안</summary>
        Ending,
    }

    /// <summary>게임 오버 이유</summary>
    public enum GameOverReason { None, OpenedNest, WrongLemon, LostDuel }

    /// <summary>
    /// 현재 단계(레몬 표시 → 완료 안내 → 검 사냥 ⇄ 상점·등장·결투 …)를 관리하고, 단계가 바뀌면 PhaseChanged로 알린다.
    /// 각 단계에서 무엇을 할지는 이 이벤트를 받는 스크립트들이 정한다.
    /// </summary>
    public class GamePhaseController : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("숲 보드 (단계가 끝나거나 게임 오버면 입력을 잠가요)")]
        [SerializeField] ForestBoard board;
        [Tooltip("굴이 모두 표시됐는지 알려주는 집계기")]
        [SerializeField] NestMarkCounter nestMarkCounter;

        [Header("게임 오버 연출")]
        [Tooltip("켜면 게임 오버 때 아직 드러나지 않은 굴을 모두 보여줘요")]
        [SerializeField] bool revealAllNestsOnGameOver = true;

        public event Action<GamePhase> PhaseChanged;
        /// <summary>다시 시작(Restart)을 눌렀을 때. 돈·재료처럼 다시 시작할 때만 비울 것들이 듣는다.</summary>
        public event Action Restarted;

        public GamePhase Phase { get; private set; } = GamePhase.LemonMarking;
        GamePhase shopReturnPhase = GamePhase.SwordHunt;
        public GameOverReason LastGameOverReason { get; private set; }

        void OnEnable()
        {
            if (nestMarkCounter != null) nestMarkCounter.MarkingCompleted += CompleteMarking;
        }

        void OnDisable()
        {
            if (nestMarkCounter != null) nestMarkCounter.MarkingCompleted -= CompleteMarking;
        }

        /// <summary>레몬 표시 단계 완료 → 완료 안내 단계로.</summary>
        public void CompleteMarking()
        {
            if (Phase != GamePhase.LemonMarking) return;
            if (board != null) board.SetLocked(true); // 이후 칸 열기·레몬 놓기 막기
            SetPhase(GamePhase.MarkingComplete);
        }

        /// <summary>완료 안내를 닫고 검 사냥 단계로. (완료 창의 확인 버튼에 연결됨)</summary>
        public void BeginSwordHunt()
        {
            if (Phase != GamePhase.MarkingComplete) return;
            SetPhase(GamePhase.SwordHunt);
        }

        /// <summary>검 사냥 중 봉인된 굴에 들어가 드래곤 등장 연출 시작. (DuelSession이 호출)</summary>
        public void EnterEncounter()
        {
            if (Phase != GamePhase.SwordHunt) return;
            if (board != null) board.SetLocked(true); // 연출·결투 중에는 보드 입력 잠금
            SetPhase(GamePhase.Encounter);
        }

        /// <summary>등장 연출이 끝나고 결투 시작. (DuelSession이 호출)</summary>
        public void EnterDuel()
        {
            if (Phase != GamePhase.Encounter) return;
            SetPhase(GamePhase.Duel);
        }

        /// <summary>결투에서 이기고 검 사냥으로 돌아가기. (DuelSession이 호출)</summary>
        public void ReturnToSwordHunt()
        {
            if (Phase != GamePhase.Duel) return;
            SetPhase(GamePhase.SwordHunt);
        }

        /// <summary>검 사냥 중 또는 스테이지 클리어 창에서 상점 열기. (ShopWindowUI가 호출) 열었으면 true.</summary>
        public bool EnterShop()
        {
            if (Phase != GamePhase.SwordHunt && Phase != GamePhase.StageClear) return false;
            shopReturnPhase = Phase;
            SetPhase(GamePhase.Shop);
            return true;
        }

        /// <summary>상점을 닫고 열기 전 단계(검 사냥 또는 스테이지 클리어)로 돌아가기. (ShopWindowUI가 호출)</summary>
        public void LeaveShop()
        {
            if (Phase != GamePhase.Shop) return;
            SetPhase(shopReturnPhase);
        }

        /// <summary>깨어난 둥지를 모두 정화함 → 스테이지 클리어 단계로. (StageProgress가 호출)</summary>
        public bool CompleteStage()
        {
            if (Phase != GamePhase.SwordHunt) return false;
            if (board != null) board.SetLocked(true);
            SetPhase(GamePhase.StageClear);
            return true;
        }

        /// <summary>보스를 쓰러뜨리고 엔딩으로. (StageProgress가 호출)</summary>
        public bool EnterEnding()
        {
            if (Phase != GamePhase.SwordHunt) return false;
            if (board != null) board.SetLocked(true);
            SetPhase(GamePhase.Ending);
            return true;
        }

        /// <summary>
        /// 스테이지 클리어 창의 [다음 스테이지] 뒤: 숲을 새로 덮고 레몬 표시 단계로. (StageProgress가 숲 크기를 바꾼 뒤 호출)
        /// 돈·재료·덱은 그대로 (Restarted를 부르지 않음).
        /// </summary>
        public void BeginNextStage()
        {
            if (Phase != GamePhase.StageClear) return;
            if (board != null) board.ResetBoard();
            SetPhase(GamePhase.LemonMarking);
        }

        /// <summary>게임 오버 (굴을 직접 열었거나, 굴이 아닌 칸에 레몬을 놓았거나, 결투에서 짐).</summary>
        public void TriggerGameOver(GameOverReason reason)
        {
            if (Phase != GamePhase.LemonMarking && Phase != GamePhase.Duel) return;
            LastGameOverReason = reason;
            if (board != null)
            {
                board.SetLocked(true);
                if (revealAllNestsOnGameOver) board.RevealAllNests();
            }
            SetPhase(GamePhase.GameOver);
        }

        /// <summary>처음부터 다시 시작. (Restart 버튼·게임 오버 창의 다시 시작 버튼에 연결됨)</summary>
        public void Restart()
        {
            LastGameOverReason = GameOverReason.None;
            if (board != null) board.ResetBoard();
            Restarted?.Invoke();
            SetPhase(GamePhase.LemonMarking);
        }

        void SetPhase(GamePhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }
    }
}
