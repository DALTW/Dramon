using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Dramon.Minesweeper
{
    /// <summary>키보드 단축키: 재시작, 난이도 전환.</summary>
    public class MinesweeperHotkeys : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("재시작을 실행할 게임 상태 관리자")]
        [SerializeField] GameStateManager gameState;
        [Tooltip("난이도 전환을 실행할 선택기")]
        [SerializeField] DifficultySelector difficultySelector;

#if ENABLE_INPUT_SYSTEM
        [Header("키")]
        [Tooltip("재시작 키")]
        [SerializeField] Key restartKey = Key.R;
        [Tooltip("난이도 키. 순서대로 DifficultySelector 목록의 0, 1, 2…번 난이도로 바뀌어요")]
        [SerializeField] Key[] difficultyKeys = { Key.Digit1, Key.Digit2, Key.Digit3 };

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (restartKey != Key.None && kb[restartKey].wasPressedThisFrame && gameState != null)
                gameState.Restart();

            if (difficultySelector == null) return;
            for (int i = 0; i < difficultyKeys.Length; i++)
                if (difficultyKeys[i] != Key.None && kb[difficultyKeys[i]].wasPressedThisFrame)
                    difficultySelector.Select(i);
        }
#endif
    }
}
