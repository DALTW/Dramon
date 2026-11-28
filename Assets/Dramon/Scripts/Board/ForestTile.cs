using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 숲 칸 하나의 외형 담당. 바닥(Ground)과 숫자(Number) 레이어의 스프라이트만 바꾼다.
    /// 게임 규칙은 ForestBoard가 처리한다.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ForestTile : MonoBehaviour
    {
        [Header("칸 정보")]
        [Tooltip("보드 안 좌표 (왼쪽 아래가 0,0). '칸 다시 만들기' 때 자동으로 정해져요. 직접 바꿀 땐 다른 칸과 겹치지 않게 하세요.")]
        [SerializeField] Vector2Int gridPosition;

        [Header("연결")]
        [Tooltip("이 칸이 속한 숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("바닥(숲·풀밭·둥지)을 그리는 SpriteRenderer — 자식 Ground")]
        [SerializeField] SpriteRenderer groundRenderer;
        [Tooltip("숫자(드래곤의 기척)를 그리는 SpriteRenderer — 자식 Number")]
        [SerializeField] SpriteRenderer numberRenderer;
        [Tooltip("칸 위에 놓인 레몬을 그리는 SpriteRenderer — 자식 Lemon")]
        [SerializeField] SpriteRenderer lemonRenderer;
        [Tooltip("바닥 프레임 애니메이션(결투 중 둥지)을 재생하는 컴포넌트 — 자식 Ground")]
        [SerializeField] SpriteFrameAnimator groundAnimator;
        [Tooltip("상태별 스프라이트 묶음. Prefab에서 바꾸면 모든 칸에 적용돼요.")]
        [SerializeField] ForestTileSpriteSet spriteSet;

        bool showingHiddenForest = true;

        public Vector2Int GridPosition => gridPosition;
        public ForestBoard Board => board;

        /// <summary>스테이지 크기로 실행 중에 만든 칸: 보드와 좌표를 정해 준다 (ForestBoard가 호출)</summary>
        public void AssignRuntime(ForestBoard owner, Vector2Int position)
        {
            board = owner;
            gridPosition = position;
        }

        public void ShowHidden()
        {
            showingHiddenForest = true;
            SetGround(spriteSet ? spriteSet.Hidden : null);
            SetNumber(null);
            SetLemon(false);
        }

        public void ShowWarningMarker()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.WarningMarker : null);
            SetNumber(null);
            SetLemon(false);
        }

        public void ShowRevealed(int nearbyNests)
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.GrassPlain : null);
            SetNumber(spriteSet ? spriteSet.GetNumber(nearbyNests) : null);
            SetLemon(false);
        }

        public void ShowNest()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.NestRevealed : null);
            SetNumber(null);
            SetLemon(false);
        }

        /// <summary>
        /// 결투 직전·진행 중인 둥지 (프레임 반복). framesOverride에 드래곤 종류별 둥지 그림을 주면 그것을 쓰고,
        /// 비어 있으면 ForestTileSpriteSet의 기본 Nest_Active 그림을 쓴다.
        /// </summary>
        public void ShowNestActive(Sprite[] framesOverride = null)
        {
            showingHiddenForest = false;
            SetNumber(null);
            SetLemon(false);
            var frames = IsComplete(framesOverride) ? framesOverride : (spriteSet ? spriteSet.NestActiveFrames : null);
            if (groundAnimator != null && frames != null) groundAnimator.Play(frames, true);
            else SetGround(spriteSet ? spriteSet.NestRevealed : null);
        }

        /// <summary>
        /// 봉인되었거나 결투에서 이겨 정화된 둥지. purifiedOverride에 드래곤 종류별 그림을 주면 그것을 쓰고,
        /// 비어 있으면 기본 Nest_Purified 그림을 쓴다.
        /// </summary>
        public void ShowNestPurified(Sprite purifiedOverride = null)
        {
            showingHiddenForest = false;
            SetGround(purifiedOverride != null ? purifiedOverride : (spriteSet ? spriteSet.NestPurified : null));
            SetNumber(null);
            SetLemon(false);
        }

        /// <summary>깨어나지 않은 둥지 (레몬 봉인으로 끝, 결투 없음). 그림이 비어 있으면 정화된 둥지 그림.</summary>
        public void ShowNestDormant()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.NestDormant : null);
            SetNumber(null);
            SetLemon(false);
        }

        static bool IsComplete(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0) return false;
            foreach (var f in frames)
                if (f == null) return false;
            return true;
        }

        /// <summary>레몬 표시 단계: 가려진 숲 위에 레몬을 놓아 둔 모습 (굴 표시).</summary>
        public void ShowLemonOnForest()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.Hidden : null);
            SetNumber(null);
            SetLemon(true);
        }

        /// <summary>봉인된 굴: 드러난 둥지(Nest_Revealed) 위에 레몬이 겹친 모습. 검 사냥 단계의 대상.</summary>
        public void ShowSealedNest()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.NestRevealed : null);
            SetNumber(null);
            SetLemon(true);
        }

        /// <summary>굴이 아닌 칸에 레몬을 놓은 실수: 풀밭 위에 레몬.</summary>
        public void ShowWrongLemon()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.GrassPlain : null);
            SetNumber(null);
            SetLemon(true);
        }

        /// <summary>마우스 올림 표시. 가려진 숲 상태(표식 없음)일 때만 바뀐다.</summary>
        public void SetHovered(bool hovered)
        {
            if (!showingHiddenForest || spriteSet == null) return;
            SetGround(hovered ? spriteSet.Hover : spriteSet.Hidden);
        }

        void SetGround(Sprite sprite)
        {
            if (groundAnimator != null) groundAnimator.Stop();
            if (groundRenderer == null) return;
            if (sprite == null)
            {
                Debug.LogWarning($"[ForestTile] {name}: ForestTileSpriteSet에 필요한 스프라이트가 비어 있어요.", this);
                return;
            }
            groundRenderer.sprite = sprite;
        }

        void SetLemon(bool visible)
        {
            if (lemonRenderer == null) return;
            if (visible && spriteSet != null) lemonRenderer.sprite = spriteSet.LemonGround;
            lemonRenderer.enabled = visible && lemonRenderer.sprite != null;
        }

        void SetNumber(Sprite sprite)
        {
            if (numberRenderer == null) return;
            numberRenderer.sprite = sprite;
            numberRenderer.enabled = sprite != null;
        }
    }
}
