using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 결투 화면 배경 그림을 드래곤별 결투 둥지로 바꾸고, 숲으로 돌아갈 때 기본 배경으로 되돌린다.
    /// 그림(sprite)만 바꾸고, 배경 Image의 크기·앵커 같은 Scene 설정은 건드리지 않는다.
    /// </summary>
    public class DuelBackgroundView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("결투 화면 배경 Image (DuelScreen/Panel)")]
        [SerializeField] Image background;

        [Header("기본 배경")]
        [Tooltip("드래곤에 결투 배경이 없거나 숲으로 돌아갈 때 쓰는 그림. 비워 두면 Scene에서 배경 Image에 넣어 둔 그림을 써요")]
        [SerializeField] Sprite defaultSprite;

        Sprite sceneSprite;
        bool recorded;

        Sprite Default => defaultSprite != null ? defaultSprite : sceneSprite;

        void Awake() => Record();

        void Record()
        {
            if (recorded) return;
            if (background != null) sceneSprite = background.sprite;
            recorded = true;
        }

        /// <summary>그 드래곤의 결투 배경으로 바꾼다 (없으면 기본 배경).</summary>
        public void Apply(DragonSpecies species)
        {
            Record();
            if (background == null) return;
            var sprite = species != null ? species.DuelBackground : null;
            background.sprite = sprite != null ? sprite : Default;
        }

        /// <summary>기본 배경으로 되돌린다.</summary>
        public void RestoreDefault()
        {
            Record();
            if (background != null) background.sprite = Default;
        }
    }
}
