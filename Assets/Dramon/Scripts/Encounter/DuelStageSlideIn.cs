using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 결투 화면의 드래곤 쪽·플레이어 쪽을 양옆에서 밀어 넣는다 (등장 연출 ③).
    /// 도착 위치는 Scene에 배치해 둔 위치를 그대로 쓰고, 덮어쓰지 않는다.
    /// </summary>
    public class DuelStageSlideIn : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("드래곤과 드래곤 발판을 묶은 오브젝트")]
        [SerializeField] RectTransform dragonSide;
        [Tooltip("플레이어와 플레이어 발판을 묶은 오브젝트")]
        [SerializeField] RectTransform playerSide;

        [Header("들어오는 방향")]
        [Tooltip("드래곤 쪽이 시작하는 위치 (Scene 배치 위치 기준, 픽셀). 기본: 왼쪽 화면 밖")]
        [SerializeField] Vector2 dragonStartOffset = new Vector2(-640f, 0f);
        [Tooltip("플레이어 쪽이 시작하는 위치 (Scene 배치 위치 기준, 픽셀). 기본: 오른쪽 화면 밖")]
        [SerializeField] Vector2 playerStartOffset = new Vector2(640f, 0f);

        Vector2 dragonHome, playerHome;
        bool homesRecorded;

        void Awake() => RecordHomes();

        void RecordHomes()
        {
            if (homesRecorded) return;
            if (dragonSide != null) dragonHome = dragonSide.anchoredPosition;
            if (playerSide != null) playerHome = playerSide.anchoredPosition;
            homesRecorded = true;
        }

        /// <summary>양쪽을 시작 위치(화면 밖)로 옮겨 둔다.</summary>
        public void PrepareOffscreen()
        {
            RecordHomes();
            if (dragonSide != null) dragonSide.anchoredPosition = dragonHome + dragonStartOffset;
            if (playerSide != null) playerSide.anchoredPosition = playerHome + playerStartOffset;
        }

        /// <summary>양쪽을 Scene 배치 위치로 밀어 넣는다.</summary>
        public IEnumerator SlideIn(float duration)
        {
            PrepareOffscreen();
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = 1f - Mathf.Pow(1f - t / duration, 3f); // 끝에서 천천히 멈춤
                if (dragonSide != null) dragonSide.anchoredPosition = dragonHome + dragonStartOffset * (1f - k);
                if (playerSide != null) playerSide.anchoredPosition = playerHome + playerStartOffset * (1f - k);
                yield return null;
            }
            SnapHome();
        }

        /// <summary>양쪽을 Scene 배치 위치로 바로 돌려놓는다.</summary>
        public void SnapHome()
        {
            RecordHomes();
            if (dragonSide != null) dragonSide.anchoredPosition = dragonHome;
            if (playerSide != null) playerSide.anchoredPosition = playerHome;
        }
    }
}
