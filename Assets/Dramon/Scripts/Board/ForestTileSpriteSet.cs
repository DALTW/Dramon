using UnityEngine;

namespace Dramon
{
    /// <summary>숲 칸이 상태별로 보여줄 스프라이트 묶음. 이 에셋만 바꾸면 모든 칸의 외형이 바뀐다.</summary>
    [CreateAssetMenu(fileName = "ForestTileSpriteSet", menuName = "Dramon/Forest Tile Sprite Set")]
    public class ForestTileSpriteSet : ScriptableObject
    {
        [Header("가려진 숲")]
        [Tooltip("아직 밝혀지지 않은 칸 (Tile_Forest_Hidden)")]
        [SerializeField] Sprite hidden;
        [Tooltip("마우스를 올린 가려진 칸 (Tile_Forest_Hover)")]
        [SerializeField] Sprite hover;
        [Tooltip("둥지로 의심해 경고 표식을 세운 칸 (Tile_Forest_WarningMarker)")]
        [SerializeField] Sprite warningMarker;

        [Header("밝혀진 풀밭")]
        [Tooltip("밝혀진 칸의 바닥 (Tile_Grass_Plain)")]
        [SerializeField] Sprite grassPlain;

        [Header("드래곤의 기척 (풀밭 위에 겹쳐 표시)")]
        [Tooltip("주변 둥지 수 숫자. 순서대로 1~8 (Number_1 ~ Number_8)")]
        [SerializeField] Sprite[] numbers = new Sprite[8];

        [Header("드래곤 둥지")]
        [Tooltip("드러난 둥지 (Nest_Revealed) — 게임 오버 때 남은 둥지 표시")]
        [SerializeField] Sprite nestRevealed;
        [Tooltip("결투 직전·진행 중인 둥지 프레임 (Nest_Active_0, Nest_Active_1 반복)")]
        [SerializeField] Sprite[] nestActiveFrames = new Sprite[2];
        [Tooltip("레몬으로 봉인했거나 결투에서 이겨 정화된 둥지 (Nest_Purified)")]
        [SerializeField] Sprite nestPurified;
        [Tooltip("깨어나지 않고 잠든 둥지 (Nest_Dormant) — 레몬 봉인으로 끝난 둥지. 비우면 정화된 둥지 그림")]
        [SerializeField] Sprite nestDormant;

        [Header("레몬")]
        [Tooltip("굴로 표시하려고 칸 위에 놓은 레몬 (Lemon_Ground) — 바닥 위에 겹쳐 표시")]
        [SerializeField] Sprite lemonGround;

        public Sprite Hidden => hidden;
        public Sprite LemonGround => lemonGround;
        public Sprite Hover => hover;
        public Sprite WarningMarker => warningMarker;
        public Sprite GrassPlain => grassPlain;
        public Sprite NestRevealed => nestRevealed;
        public Sprite[] NestActiveFrames => nestActiveFrames;
        public Sprite NestPurified => nestPurified;
        public Sprite NestDormant => nestDormant != null ? nestDormant : nestPurified;

        /// <summary>주변 둥지 수(1~8)에 맞는 숫자 스프라이트. 0이면 null.</summary>
        public Sprite GetNumber(int nearbyNests)
        {
            if (numbers == null || nearbyNests < 1 || nearbyNests > numbers.Length) return null;
            return numbers[nearbyNests - 1];
        }
    }
}
