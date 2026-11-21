using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon.Minesweeper
{
    /// <summary>
    /// 난이도별로 Scene에 미리 놓아둔 보드 중 하나만 켜고, 필요하면 카메라를 그 보드에 맞춘다.
    /// 시작할 때는 Scene에서 켜져 있는 보드를 그대로 쓰고 카메라도 건드리지 않는다.
    /// </summary>
    public class DifficultySelector : MonoBehaviour
    {
        [Serializable]
        public class DifficultyEntry
        {
            [Tooltip("난이도 이름 (구분용)")]
            public string displayName = "Beginner";
            [Tooltip("이 난이도에서 사용할 보드 오브젝트 (Hierarchy의 Boards 아래)")]
            public MinesweeperBoard board;
            [Tooltip("켜면 이 난이도로 바꿀 때 카메라를 아래 위치·크기로 옮겨요")]
            public bool moveCamera = true;
            [Tooltip("이 난이도에서 카메라 위치")]
            public Vector3 cameraPosition = new Vector3(0f, 0f, -10f);
            [Tooltip("이 난이도에서 카메라 Size (Orthographic). 클수록 넓게 보여요")]
            [Min(0.1f)] public float cameraSize = 6f;
        }

        [Header("난이도 목록 (버튼 1, 2, 3 순서)")]
        [SerializeField] List<DifficultyEntry> difficulties = new List<DifficultyEntry>();

        [Header("연결")]
        [Tooltip("보드가 바뀌었음을 알릴 게임 상태 관리자")]
        [SerializeField] GameStateManager gameState;
        [Tooltip("난이도 전환 때 옮길 카메라")]
        [SerializeField] Camera targetCamera;

        public IReadOnlyList<DifficultyEntry> Difficulties => difficulties;

        void Start()
        {
            foreach (var entry in difficulties)
            {
                if (entry.board != null && entry.board.gameObject.activeInHierarchy)
                {
                    gameState.SetBoard(entry.board);
                    return;
                }
            }
        }

        /// <summary>index번째 난이도로 바꾼다. (난이도 버튼·숫자 키에 연결됨)</summary>
        public void Select(int index)
        {
            if (index < 0 || index >= difficulties.Count) return;
            var chosen = difficulties[index];
            if (chosen.board == null) return;

            foreach (var entry in difficulties)
                if (entry.board != null) entry.board.gameObject.SetActive(entry == chosen);

            if (chosen.moveCamera && targetCamera != null)
            {
                targetCamera.transform.position = chosen.cameraPosition;
                targetCamera.orthographicSize = chosen.cameraSize;
            }

            gameState.SetBoard(chosen.board);
            gameState.Restart();
        }
    }
}
