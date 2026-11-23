using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤 종류별 둥지 그림 묶음. 비어 있는 칸은 ForestTileSpriteSet의 기본 둥지 그림을 쓴다.</summary>
    [CreateAssetMenu(fileName = "NestSpriteSet", menuName = "Dramon/Nest Sprite Set")]
    public class NestSpriteSet : ScriptableObject
    {
        [Tooltip("드러난 둥지 (Nest_Revealed 크기, 32×32)")]
        [SerializeField] Sprite revealed;
        [Tooltip("결투 중 둥지 — 반복 재생할 프레임들 (기본 2프레임)")]
        [SerializeField] Sprite[] activeFrames = new Sprite[2];
        [Tooltip("결투에서 이겨 정화된 둥지")]
        [SerializeField] Sprite purified;

        public Sprite Revealed => revealed;
        public Sprite[] ActiveFrames => activeFrames;
        public Sprite Purified => purified;
    }
}
