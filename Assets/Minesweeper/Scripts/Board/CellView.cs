using UnityEngine;

namespace Dramon.Minesweeper
{
    /// <summary>칸 하나의 외형 표시 담당. 게임 규칙은 MinesweeperBoard가 처리하고, 이 스크립트는 보여주기만 한다.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CellView : MonoBehaviour
    {
        [Header("칸 정보")]
        [Tooltip("보드 안에서의 좌표 (왼쪽 아래가 0,0). '칸 다시 만들기' 때 자동으로 정해져요. 직접 바꿀 때는 다른 칸과 겹치지 않게 하세요.")]
        [SerializeField] Vector2Int gridPosition;

        [Header("연결")]
        [Tooltip("이 칸이 속한 보드")]
        [SerializeField] MinesweeperBoard board;
        [Tooltip("스프라이트를 그릴 SpriteRenderer")]
        [SerializeField] SpriteRenderer spriteRenderer;
        [Tooltip("상태별 스프라이트 묶음. Prefab에서 바꾸면 모든 칸에 적용돼요.")]
        [SerializeField] CellSpriteSet spriteSet;

        public Vector2Int GridPosition => gridPosition;
        public MinesweeperBoard Board => board;

        void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void ShowHidden() => SetSprite(spriteSet ? spriteSet.Hidden : null);
        public void ShowFlag() => SetSprite(spriteSet ? spriteSet.Flag : null);
        public void ShowNumber(int adjacentMines) => SetSprite(spriteSet ? spriteSet.GetNumber(adjacentMines) : null);
        public void ShowMine() => SetSprite(spriteSet ? spriteSet.Mine : null);
        public void ShowExploded() => SetSprite(spriteSet ? spriteSet.MineExploded : null);
        public void ShowWrongFlag() => SetSprite(spriteSet ? spriteSet.WrongFlag : null);

        void SetSprite(Sprite sprite)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                Debug.LogWarning($"[CellView] {name}: CellSpriteSet이 비어 있거나 스프라이트가 없어요.", this);
                return;
            }
            spriteRenderer.sprite = sprite;
        }
    }
}
