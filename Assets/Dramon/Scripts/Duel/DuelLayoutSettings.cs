using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 결투 화면 구도 값 (포켓몬 팬게임 방식, 640×360 HUD 기준).
    /// 실행 중에는 쓰지 않고, DuelLayoutSettings Inspector의 [포켓몬 구도 적용] 버튼을 눌렀을 때만 Scene에 써 넣는다.
    /// 그 뒤에 Scene에서 직접 옮긴 위치는 그대로 유지된다.
    /// </summary>
    [CreateAssetMenu(fileName = "DuelLayoutSettings", menuName = "Dramon/Duel Layout Settings")]
    public class DuelLayoutSettings : ScriptableObject
    {
        [Header("검사 (PlayerSide)")]
        [Tooltip("켜면 검사가 예전 플레이어처럼 드래곤별 둥지 배경의 작은 발판(DragonSpecies의 플레이어 발판 위치)에 서요. 끄면 아래 발밑 위치(포켓몬 구도)에 서요")]
        [SerializeField] bool standOnNestPlatform = true;
        [Tooltip("검사 발밑 위치 (결투 화면 왼쪽 아래 = 0,0 / 오른쪽 위 = 1,1). 작은 발판에 설 때는 발판 위치가 없는 드래곤에서만 써요")]
        [SerializeField] Vector2 heroFootPoint = new Vector2(0.25f, 0.19f);
        [Tooltip("검사 그림 칸 크기(px). 시트 한 칸(256×256)이 이 크기로 그려지고, 검사 키는 이 값의 약 0.87배예요 (74이면 키 약 64px = 예전 뒷모습 플레이어와 같은 크기)")]
        [Min(16f)]
        [SerializeField] float heroSize = 74f;

        [Header("플레이어 체력 창 (오른쪽 아래, 메시지 창 바로 위)")]
        [Tooltip("기준점 (0,0 = 왼쪽 아래, 1,0 = 오른쪽 아래)")]
        [SerializeField] Vector2 playerHealthAnchor = new Vector2(1f, 0f);
        [Tooltip("창의 기준점 위치 (Pivot)")]
        [SerializeField] Vector2 playerHealthPivot = new Vector2(1f, 0f);
        [Tooltip("기준점에서의 위치 (px)")]
        [SerializeField] Vector2 playerHealthPosition = new Vector2(-16f, 76f);
        [Tooltip("창 크기 (px)")]
        [SerializeField] Vector2 playerHealthSize = new Vector2(160f, 40f);

        public bool StandOnNestPlatform => standOnNestPlatform;
        public Vector2 HeroFootPoint => heroFootPoint;
        public float HeroSize => heroSize;
        public Vector2 PlayerHealthAnchor => playerHealthAnchor;
        public Vector2 PlayerHealthPivot => playerHealthPivot;
        public Vector2 PlayerHealthPosition => playerHealthPosition;
        public Vector2 PlayerHealthSize => playerHealthSize;
    }
}
