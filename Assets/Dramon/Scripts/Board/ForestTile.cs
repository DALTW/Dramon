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
        [Tooltip("바닥 프레임 애니메이션(결투 중 둥지)을 재생하는 컴포넌트 — 자식 Ground")]
        [SerializeField] SpriteFrameAnimator groundAnimator;
        [Tooltip("상태별 스프라이트 묶음. Prefab에서 바꾸면 모든 칸에 적용돼요.")]
        [SerializeField] ForestTileSpriteSet spriteSet;

        bool showingHiddenForest = true;

        public Vector2Int GridPosition => gridPosition;
        public ForestBoard Board => board;

        public void ShowHidden()
        {
            showingHiddenForest = true;
            SetGround(spriteSet ? spriteSet.Hidden : null);
            SetNumber(null);
        }

        public void ShowWarningMarker()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.WarningMarker : null);
            SetNumber(null);
        }

        public void ShowRevealed(int nearbyNests)
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.GrassPlain : null);
            SetNumber(spriteSet ? spriteSet.GetNumber(nearbyNests) : null);
        }

        public void ShowNest()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.NestRevealed : null);
            SetNumber(null);
        }

        /// <summary>결투 직전·진행 중인 둥지 (Nest_Active 프레임 반복).</summary>
        public void ShowNestActive()
        {
            showingHiddenForest = false;
            SetNumber(null);
            if (groundAnimator != null && spriteSet != null) groundAnimator.Play(spriteSet.NestActiveFrames, true);
            else SetGround(spriteSet ? spriteSet.NestRevealed : null);
        }

        /// <summary>봉인되었거나 결투에서 이겨 정화된 둥지.</summary>
        public void ShowNestPurified()
        {
            showingHiddenForest = false;
            SetGround(spriteSet ? spriteSet.NestPurified : null);
            SetNumber(null);
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

        void SetNumber(Sprite sprite)
        {
            if (numberRenderer == null) return;
            numberRenderer.sprite = sprite;
            numberRenderer.enabled = sprite != null;
        }
    }
}
