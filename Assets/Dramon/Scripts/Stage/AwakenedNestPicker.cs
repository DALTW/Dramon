using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 레몬 표시가 끝나면 이번 스테이지의 '깨어난 둥지'(결투할 둥지)를 무작위로 고른다.
    /// 나머지 둥지는 레몬 봉인으로 끝 (정화된 둥지 모습, 검을 대도 반응 없음).
    /// </summary>
    public class AwakenedNestPicker : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] GamePhaseController phaseController;
        [SerializeField] ForestBoard board;
        [Tooltip("이번 스테이지의 깨어난 둥지 수")]
        [SerializeField] StageProgress stageProgress;

        [Header("설정")]
        [Tooltip("켜면 깨어난 둥지만 결투해요. 끄면 레몬을 놓은 둥지 모두와 결투 (예전 방식)")]
        [SerializeField] bool useAwakenedCount = true;

        [Header("테스트 (에디터에서만)")]
        [Tooltip("0보다 크면 깨어난 둥지 수를 이 값으로 (에디터에서만)")]
        [Min(0)]
        [SerializeField] int testAwakenedCount;

        public int LastAwakened { get; private set; }

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.MarkingComplete || board == null) return;
            var nests = board.GetSealedNests();
            int count = AwakenedCount(nests.Count);
            LastAwakened = count;
            if (!useAwakenedCount) return;
            // 섞은 뒤 앞의 count개만 남기고 나머지는 잠든 둥지로
            for (int i = 0; i < nests.Count; i++)
            {
                int j = Random.Range(i, nests.Count);
                (nests[i], nests[j]) = (nests[j], nests[i]);
            }
            for (int i = count; i < nests.Count; i++) board.SetNestDormant(nests[i]);
        }

        int AwakenedCount(int total)
        {
            if (!useAwakenedCount) return total;
            int n = Application.isEditor && testAwakenedCount > 0 ? testAwakenedCount
                : stageProgress != null ? stageProgress.Current.awakened : total;
            return Mathf.Clamp(n, 1, Mathf.Max(1, total));
        }
    }
}
