using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>레몬 표시에 걸린 시간 → 보상금 규칙. 구간을 추가·삭제할 수 있다.</summary>
    [CreateAssetMenu(fileName = "MarkingRewardSettings", menuName = "Dramon/Marking Reward Settings")]
    public class MarkingRewardSettings : ScriptableObject
    {
        [Serializable]
        public class RewardTier
        {
            [Tooltip("이 시간(초) 이하로 끝내면")]
            [Min(0f)] public float maxSeconds = 60f;
            [Tooltip("받는 보상금")]
            [Min(0)] public int reward = 100;
        }

        [Header("시간 구간 (짧은 시간 순으로 확인)")]
        [Tooltip("걸린 시간이 '최대 시간' 이하인 첫 구간의 보상금을 받아요. 순서와 상관없이 짧은 시간부터 확인해요")]
        [SerializeField] List<RewardTier> tiers = new List<RewardTier>
        {
            new RewardTier { maxSeconds = 60f, reward = 100 },
            new RewardTier { maxSeconds = 120f, reward = 70 },
            new RewardTier { maxSeconds = 180f, reward = 40 },
        };

        [Header("그 이상")]
        [Tooltip("어느 구간에도 들지 않을 때(가장 느린 경우) 받는 보상금")]
        [SerializeField, Min(0)] int fallbackReward = 20;

        /// <summary>
        /// 시간 구간을 limits로 바꿔서 계산 (스테이지 숲 크기에 맞춘 시간). 구간은 짧은 시간 순으로 limits[0], limits[1] …과 짝지어요.
        /// limits가 모자라는 구간은 Inspector의 최대 시간을 그대로 써요.
        /// </summary>
        public int GetReward(float seconds, IReadOnlyList<float> limits)
        {
            if (limits == null || limits.Count == 0) return GetReward(seconds);
            var sorted = new List<RewardTier>();
            foreach (var t in tiers) if (t != null) sorted.Add(t);
            sorted.Sort((a, b) => a.maxSeconds.CompareTo(b.maxSeconds));
            for (int i = 0; i < sorted.Count; i++)
            {
                float limit = i < limits.Count ? limits[i] : sorted[i].maxSeconds;
                if (seconds <= limit) return sorted[i].reward;
            }
            return fallbackReward;
        }

        public int GetReward(float seconds)
        {
            RewardTier best = null;
            foreach (var tier in tiers)
            {
                if (tier == null || seconds > tier.maxSeconds) continue;
                if (best == null || tier.maxSeconds < best.maxSeconds) best = tier;
            }
            return best != null ? best.reward : fallbackReward;
        }
    }
}
