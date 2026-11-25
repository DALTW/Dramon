using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 결투 화면의 드래곤 쪽·플레이어 쪽을 양옆에서 밀어 넣는다 (등장 연출 ③).
    /// 도착 위치는 기본으로 Scene에 배치해 둔 위치를 쓰고, 등장마다 드래곤별 발판 위치(배경 비율)로 바꿀 수 있다.
    /// 바꾼 위치는 매번 Scene 배치 위치에서 새로 계산하므로 누적되지 않고, 크기·앵커 같은 Scene 설정은 건드리지 않는다.
    /// </summary>
    public class DuelStageSlideIn : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("드래곤과 드래곤 발판을 묶은 오브젝트 (Pivot = 발밑)")]
        [SerializeField] RectTransform dragonSide;
        [Tooltip("플레이어와 플레이어 발판을 묶은 오브젝트 (Pivot = 발밑)")]
        [SerializeField] RectTransform playerSide;
        [Tooltip("발판 위치(0~1 비율)를 잴 기준 영역. 보통 결투 배경 Image (DuelScreen/Panel)")]
        [SerializeField] RectTransform standArea;

        [Header("들어오는 방향")]
        [Tooltip("드래곤 쪽이 시작하는 위치 (도착 위치 기준, 픽셀). 기본: 왼쪽 화면 밖")]
        [SerializeField] Vector2 dragonStartOffset = new Vector2(-640f, 0f);
        [Tooltip("플레이어 쪽이 시작하는 위치 (도착 위치 기준, 픽셀). 기본: 오른쪽 화면 밖")]
        [SerializeField] Vector2 playerStartOffset = new Vector2(640f, 0f);

        [Header("발판 위치 보정")]
        [Tooltip("드래곤별 발판 위치를 쓸 때 드래곤 쪽에 더하는 값 (픽셀). 그림 아래 빈 공간 등을 맞출 때 사용")]
        [SerializeField] Vector2 dragonStandOffset = Vector2.zero;
        [Tooltip("드래곤별 발판 위치를 쓸 때 플레이어 쪽에 더하는 값 (픽셀)")]
        [SerializeField] Vector2 playerStandOffset = Vector2.zero;

        Vector2 dragonHome, playerHome;     // Scene에 배치된 위치
        Vector2 dragonTarget, playerTarget; // 이번 등장의 도착 위치
        bool homesRecorded;

        void Awake() => RecordHomes();

        void RecordHomes()
        {
            if (homesRecorded) return;
            if (dragonSide != null) dragonHome = dragonSide.anchoredPosition;
            if (playerSide != null) playerHome = playerSide.anchoredPosition;
            dragonTarget = dragonHome;
            playerTarget = playerHome;
            homesRecorded = true;
        }

        /// <summary>이번 등장의 도착 위치를 배경 비율 위치(0~1, 왼쪽 아래 기준)로 정한다.</summary>
        public void SetArrivalFromStandPoints(Vector2 dragonPoint, Vector2 playerPoint)
        {
            RecordHomes();
            dragonTarget = PointToAnchored(dragonSide, dragonPoint, dragonHome) + dragonStandOffset;
            playerTarget = PointToAnchored(playerSide, playerPoint, playerHome) + playerStandOffset;
        }

        /// <summary>도착 위치를 Scene 배치 위치로 되돌린다 (움직이지는 않음).</summary>
        public void ResetArrival()
        {
            RecordHomes();
            dragonTarget = dragonHome;
            playerTarget = playerHome;
        }

        /// <summary>양쪽을 시작 위치(화면 밖)로 옮겨 둔다.</summary>
        public void PrepareOffscreen()
        {
            RecordHomes();
            if (dragonSide != null) dragonSide.anchoredPosition = dragonTarget + dragonStartOffset;
            if (playerSide != null) playerSide.anchoredPosition = playerTarget + playerStartOffset;
        }

        /// <summary>양쪽을 도착 위치로 밀어 넣는다.</summary>
        public IEnumerator SlideIn(float duration)
        {
            PrepareOffscreen();
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = 1f - Mathf.Pow(1f - t / duration, 3f); // 끝에서 천천히 멈춤
                if (dragonSide != null) dragonSide.anchoredPosition = dragonTarget + dragonStartOffset * (1f - k);
                if (playerSide != null) playerSide.anchoredPosition = playerTarget + playerStartOffset * (1f - k);
                yield return null;
            }
            SnapHome();
        }

        /// <summary>양쪽을 이번 도착 위치로 바로 옮긴다.</summary>
        public void SnapHome()
        {
            RecordHomes();
            if (dragonSide != null) dragonSide.anchoredPosition = dragonTarget;
            if (playerSide != null) playerSide.anchoredPosition = playerTarget;
        }

        /// <summary>도착 위치를 Scene 배치 위치로 되돌리고 양쪽을 그 자리로 옮긴다 (숲으로 돌아갈 때·다시 시작할 때).</summary>
        public void RestoreScenePositions()
        {
            ResetArrival();
            SnapHome();
        }

        /// <summary>기준 영역 안의 비율 위치를 side의 anchoredPosition(Pivot 위치)으로 바꾼다. 앵커·크기는 그대로.</summary>
        Vector2 PointToAnchored(RectTransform side, Vector2 point, Vector2 fallback)
        {
            if (side == null || standArea == null || !(side.parent is RectTransform parent)) return fallback;

            var area = standArea.rect;
            var local = new Vector3(area.xMin + area.width * point.x, area.yMin + area.height * point.y, 0f);
            Vector2 inParent = parent.InverseTransformPoint(standArea.TransformPoint(local));

            // anchoredPosition = Pivot의 위치 - 앵커 기준점
            var pr = parent.rect;
            var anchorRef = new Vector2(
                Mathf.Lerp(pr.xMin, pr.xMax, Mathf.Lerp(side.anchorMin.x, side.anchorMax.x, side.pivot.x)),
                Mathf.Lerp(pr.yMin, pr.yMax, Mathf.Lerp(side.anchorMin.y, side.anchorMax.y, side.pivot.y)));
            return inParent - anchorRef;
        }
    }
}
