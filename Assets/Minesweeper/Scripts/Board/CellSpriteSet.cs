using UnityEngine;

namespace Dramon.Minesweeper
{
    /// <summary>칸 하나가 상태별로 보여줄 스프라이트 묶음. 이 에셋의 스프라이트만 바꾸면 모든 칸의 외형이 바뀐다.</summary>
    [CreateAssetMenu(fileName = "CellSpriteSet", menuName = "Dramon/Minesweeper/Cell Sprite Set")]
    public class CellSpriteSet : ScriptableObject
    {
        [Header("닫힌 칸")]
        [Tooltip("아직 열지 않은 칸")]
        [SerializeField] Sprite hidden;
        [Tooltip("깃발을 꽂은 칸")]
        [SerializeField] Sprite flag;

        [Header("열린 칸")]
        [Tooltip("열린 칸. 순서(0~8) = 주변 지뢰 수. 0번은 빈 칸")]
        [SerializeField] Sprite[] numbers = new Sprite[9];

        [Header("지뢰")]
        [Tooltip("게임 오버 때 드러나는 지뢰")]
        [SerializeField] Sprite mine;
        [Tooltip("직접 밟아서 터진 지뢰")]
        [SerializeField] Sprite mineExploded;
        [Tooltip("지뢰가 아닌데 깃발을 꽂았던 칸 (게임 오버 때 표시)")]
        [SerializeField] Sprite wrongFlag;

        public Sprite Hidden => hidden;
        public Sprite Flag => flag;
        public Sprite Mine => mine;
        public Sprite MineExploded => mineExploded;
        public Sprite WrongFlag => wrongFlag;

        public Sprite GetNumber(int adjacentMines)
        {
            if (numbers == null || numbers.Length == 0) return null;
            return numbers[Mathf.Clamp(adjacentMines, 0, numbers.Length - 1)];
        }
    }
}
