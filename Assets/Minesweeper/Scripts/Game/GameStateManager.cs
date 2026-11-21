using System;
using UnityEngine;

namespace Dramon.Minesweeper
{
    public enum GameState { Ready, Playing, Won, Lost }

    /// <summary>현재 보드의 게임 진행 상태(대기·진행·승리·패배)와 경과 시간을 관리한다.</summary>
    public class GameStateManager : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("시작할 때 플레이할 보드. 난이도를 바꾸면 DifficultySelector가 자동으로 교체해요")]
        [SerializeField] MinesweeperBoard currentBoard;

        [Header("타이머")]
        [Tooltip("타이머가 셀 수 있는 최대 시간(초)")]
        [SerializeField, Min(1f)] float maxTime = 999f;

        public event Action<GameState> StateChanged;
        public event Action<MinesweeperBoard> BoardChanged;

        public GameState State { get; private set; } = GameState.Ready;
        public MinesweeperBoard CurrentBoard => currentBoard;

        float startTime;
        float finishedTime;

        public float ElapsedTime
        {
            get
            {
                float t = State switch
                {
                    GameState.Playing => Time.time - startTime,
                    GameState.Ready => 0f,
                    _ => finishedTime,
                };
                return Mathf.Min(t, maxTime);
            }
        }

        void OnEnable() => Subscribe(currentBoard);
        void OnDisable() => Unsubscribe(currentBoard);

        public void SetBoard(MinesweeperBoard board)
        {
            if (board == currentBoard) return;
            Unsubscribe(currentBoard);
            currentBoard = board;
            Subscribe(currentBoard);
            BoardChanged?.Invoke(currentBoard);
            SetState(GameState.Ready);
        }

        /// <summary>현재 보드를 처음 상태로 되돌린다. (Restart 버튼·R 키에 연결됨)</summary>
        public void Restart()
        {
            if (currentBoard != null) currentBoard.ResetBoard();
            SetState(GameState.Ready);
        }

        void Subscribe(MinesweeperBoard board)
        {
            if (board == null) return;
            board.FirstRevealed += HandleFirstRevealed;
            board.Exploded += HandleExploded;
            board.Cleared += HandleCleared;
        }

        void Unsubscribe(MinesweeperBoard board)
        {
            if (board == null) return;
            board.FirstRevealed -= HandleFirstRevealed;
            board.Exploded -= HandleExploded;
            board.Cleared -= HandleCleared;
        }

        void HandleFirstRevealed()
        {
            startTime = Time.time;
            SetState(GameState.Playing);
        }

        void HandleExploded() => Finish(GameState.Lost);
        void HandleCleared() => Finish(GameState.Won);

        void Finish(GameState result)
        {
            finishedTime = Time.time - startTime;
            SetState(result);
        }

        void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
