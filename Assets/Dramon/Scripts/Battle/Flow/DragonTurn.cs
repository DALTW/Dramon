using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤 턴: 드래곤 방어 초기화 → 예고한 행동 실행 → 다음 행동 예고</summary>
    public class DragonTurn : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] DragonBattleStats dragon;
        [SerializeField] DragonIntentPicker intent;
        [SerializeField] DragonActionExecutor executor;

        [Header("연출 시간")]
        [Tooltip("드래곤 턴이 시작되고 행동하기까지(초)")]
        [Min(0f)]
        [SerializeField] float startDelay = 0.5f;
        [Tooltip("행동한 뒤 내 턴이 돌아오기까지(초)")]
        [Min(0f)]
        [SerializeField] float endDelay = 0.8f;

        public IEnumerator Run()
        {
            if (dragon != null) dragon.ClearBlock();
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);
            if (intent != null && executor != null)
            {
                yield return executor.Execute(intent.Species, intent.Current);
                intent.Advance();
            }
            if (endDelay > 0f) yield return new WaitForSeconds(endDelay);
        }
    }
}
