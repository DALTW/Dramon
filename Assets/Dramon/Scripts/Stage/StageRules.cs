using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 스테이지 구성 공식. 무한 모드를 위해 모든 값은 공식으로 계산하고, 앞쪽 스테이지는 덮어쓰기 목록으로 손볼 수 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "StageRules", menuName = "Dramon/Stage Rules")]
    public class StageRules : ScriptableObject
    {
        /// <summary>한 스테이지에 적용할 값</summary>
        public struct StageValues
        {
            public int stage, width, height, nests, awakened, maxEnergy, draw;
            public int Cells => width * height;
        }

        /// <summary>앞쪽 스테이지 덮어쓰기 (0이면 공식 값)</summary>
        [Serializable]
        public class StageOverride
        {
            [Tooltip("덮어쓸 스테이지 번호")]
            [Min(1)] public int stage = 1;
            [Tooltip("숲 가로 칸 수 (0 = 공식)")]
            [Min(0)] public int width;
            [Tooltip("숲 세로 칸 수 (0 = 공식)")]
            [Min(0)] public int height;
            [Tooltip("둥지 수 (0 = 공식)")]
            [Min(0)] public int nests;
            [Tooltip("깨어난 둥지 수 = 결투 수 (0 = 공식)")]
            [Min(0)] public int awakened;
            [Tooltip("최대 에너지 (0 = 공식)")]
            [Min(0)] public int maxEnergy;
        }

        [Header("둥지")]
        [Tooltip("1 스테이지 둥지 수")]
        [Min(1)] [SerializeField] int baseNests = 4;
        [Tooltip("스테이지마다 늘어나는 둥지 수")]
        [Min(0)] [SerializeField] int nestsPerStage = 2;

        [Header("숲 크기")]
        [Tooltip("10 스테이지부터: 칸 수 = 둥지 수 × 이 값 (밀도 20%)")]
        [Min(1f)] [SerializeField] float cellsPerNest = 5f;
        [Tooltip("숲 최대 가로 칸 수")]
        [Min(1)] [SerializeField] int maxWidth = 24;
        [Tooltip("숲 최대 세로 칸 수")]
        [Min(1)] [SerializeField] int maxHeight = 16;

        [Header("깨어난 둥지 (결투 수)")]
        [Tooltip("1 스테이지 깨어난 둥지 수")]
        [Min(1)] [SerializeField] int baseAwakened = 2;
        [Tooltip("이 스테이지 수마다 1개씩 늘어나요 (2 = 2스테이지마다 +1)")]
        [Min(1)] [SerializeField] int stagesPerExtraAwakened = 2;
        [Tooltip("깨어난 둥지 최대 수")]
        [Min(1)] [SerializeField] int maxAwakened = 6;

        [Header("최대 에너지")]
        [Tooltip("1 스테이지 최대 에너지")]
        [Min(1)] [SerializeField] int baseEnergy = 3;
        [Tooltip("이 스테이지까지는 스테이지마다 +1 (4 = 1~4 스테이지에서 3 → 6)")]
        [Min(1)] [SerializeField] int fastEnergyUntilStage = 4;
        [Tooltip("그 뒤에는 이 스테이지 수마다 +1")]
        [Min(1)] [SerializeField] int stagesPerExtraEnergy = 4;
        [Tooltip("최대 에너지 상한")]
        [Min(1)] [SerializeField] int energyCap = 10;

        [Header("턴당 뽑기")]
        [Tooltip("에너지 3에서 뽑는 장수")]
        [Min(1)] [SerializeField] int baseDraw = 5;
        [Tooltip("에너지가 이만큼 오를 때마다 뽑기 +1 (2 = 3~4: 5장, 5~6: 6장 …)")]
        [Min(1)] [SerializeField] int energyPerExtraDraw = 2;

        [Header("레몬 표시 보상 시간 (칸 수 × 초)")]
        [Tooltip("보상 구간의 시간 = 칸 수 × 이 값 (초). MarkingRewardSettings 구간의 보상금과 순서대로 짝지어요")]
        [SerializeField] float[] rewardSecondsPerCell = { 0.8f, 1.6f, 2.4f };

        [Header("드래곤 스테이지 배율 (체력·할퀴기·연타·숨결 피해에만)")]
        [Tooltip("1·2·3… 스테이지의 체력 배율")]
        [SerializeField] float[] healthMultipliers = { 1f, 3f, 3.5f, 4f, 4.5f };
        [Tooltip("목록 다음 스테이지부터 한 스테이지마다 더하는 체력 배율")]
        [SerializeField] float healthMultiplierPerStageAfter = 0.5f;
        [Tooltip("1·2·3… 스테이지의 공격 배율")]
        [SerializeField] float[] attackMultipliers = { 1f, 1.2f, 1.5f, 2f, 2.2f };
        [Tooltip("목록 다음 스테이지부터 한 스테이지마다 더하는 공격 배율")]
        [SerializeField] float attackMultiplierPerStageAfter = 0.2f;
        [Tooltip("같은 스테이지 안에서 결투를 할 때마다 더해지는 비율 (0.05 = 결투마다 +5%)")]
        [Min(0f)] [SerializeField] float perDuelInStage = 0.05f;

        [Header("스테이지 클리어 보상")]
        [Tooltip("스테이지 클리어 골드 = 이 값 × 스테이지 번호")]
        [Min(0)] [SerializeField] int clearGoldPerStage = 30;

        [Header("앞쪽 스테이지 덮어쓰기")]
        [Tooltip("공식 대신 쓸 값 (0인 칸은 공식 그대로)")]
        [SerializeField] List<StageOverride> overrides = new List<StageOverride>();

        public IReadOnlyList<float> RewardSecondsPerCell => rewardSecondsPerCell;
        public float PerDuelInStage => perDuelInStage;
        public int ClearGold(int stage) => clearGoldPerStage * Mathf.Max(1, stage);

        public StageValues Get(int stage)
        {
            stage = Mathf.Max(1, stage);
            var v = new StageValues { stage = stage };
            v.nests = baseNests + nestsPerStage * (stage - 1);
            (v.width, v.height) = BoardSize(v.nests);
            v.awakened = Mathf.Min(maxAwakened, baseAwakened + (stage - 1) / stagesPerExtraAwakened);
            v.maxEnergy = Energy(stage);

            var o = overrides.Find(x => x != null && x.stage == stage);
            if (o != null)
            {
                if (o.width > 0) v.width = o.width;
                if (o.height > 0) v.height = o.height;
                if (o.nests > 0) v.nests = o.nests;
                if (o.awakened > 0) v.awakened = o.awakened;
                if (o.maxEnergy > 0) v.maxEnergy = o.maxEnergy;
            }
            // 숲 최대 크기에서는 둥지도 그만큼까지만 (밀도 20%)
            int maxNests = Mathf.FloorToInt(maxWidth * maxHeight / cellsPerNest);
            v.nests = Mathf.Clamp(v.nests, 1, Mathf.Min(maxNests, v.width * v.height - 9));
            v.awakened = Mathf.Clamp(v.awakened, 1, v.nests);
            v.draw = baseDraw + Mathf.Max(0, v.maxEnergy - 3) / energyPerExtraDraw;
            return v;
        }

        int Energy(int stage)
        {
            int e = stage <= fastEnergyUntilStage
                ? baseEnergy + (stage - 1)
                : baseEnergy + (fastEnergyUntilStage - 1) + (stage - fastEnergyUntilStage) / stagesPerExtraEnergy;
            return Mathf.Min(energyCap, e);
        }

        /// <summary>둥지 수로 숲 크기: 칸 수 = 둥지 × 5, 가로 = 세로 또는 세로+1, 세로가 최대가 되면 가로만 늘림</summary>
        (int, int) BoardSize(int nests)
        {
            int cells = Mathf.CeilToInt(nests * cellsPerNest);
            for (int h = 1; h <= maxHeight; h++)
            {
                if (h * h >= cells) return (h, h);
                if ((h + 1) * h >= cells && h + 1 <= maxWidth) return (h + 1, h);
            }
            int w = Mathf.Clamp(Mathf.CeilToInt(cells / (float)maxHeight), maxHeight, maxWidth);
            return (w, maxHeight);
        }

        /// <summary>그 스테이지의 드래곤 체력 배율 (같은 스테이지의 몇 번째 결투인지 포함, 0부터)</summary>
        public float HealthMultiplier(int stage, int duelIndexInStage) =>
            Pick(healthMultipliers, healthMultiplierPerStageAfter, stage) * (1f + perDuelInStage * Mathf.Max(0, duelIndexInStage));

        /// <summary>그 스테이지의 드래곤 공격 배율 (같은 스테이지의 몇 번째 결투인지 포함, 0부터)</summary>
        public float AttackMultiplier(int stage, int duelIndexInStage) =>
            Pick(attackMultipliers, attackMultiplierPerStageAfter, stage) * (1f + perDuelInStage * Mathf.Max(0, duelIndexInStage));

        static float Pick(float[] list, float perStageAfter, int stage)
        {
            stage = Mathf.Max(1, stage);
            if (list == null || list.Length == 0) return 1f;
            if (stage <= list.Length) return list[stage - 1];
            return list[list.Length - 1] + perStageAfter * (stage - list.Length);
        }
    }
}
