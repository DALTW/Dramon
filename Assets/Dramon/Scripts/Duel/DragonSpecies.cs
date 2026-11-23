using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤 한 종류의 데이터: 이름, 체력, 그림, 둥지 그림, 울음소리. 나올 드래곤은 DragonSpawnList에 넣어서 정한다.</summary>
    [CreateAssetMenu(fileName = "DragonSpecies", menuName = "Dramon/Dragon Species")]
    public class DragonSpecies : ScriptableObject
    {
        /// <summary>결투에서 이겼을 때 떨어뜨리는 재료 한 줄 (다음 단계에서 사용)</summary>
        [Serializable]
        public class MaterialDrop
        {
            [Tooltip("재료 이름 (예: 비늘, 발톱, 뿔)")]
            public string materialName;
            [Tooltip("재료 아이콘")]
            public Sprite icon;
            [Tooltip("떨어뜨릴 확률 (0~1)")]
            [Range(0f, 1f)] public float chance = 0.5f;
            [Tooltip("상점에 팔 때 받는 돈")]
            [Min(0)] public int sellPrice;
        }

        [Header("기본 정보")]
        [Tooltip("도감 번호 (01~10)")]
        [SerializeField] int number = 1;
        [Tooltip("결투 화면에 표시할 드래곤 이름")]
        [SerializeField] string displayName = "드래곤";
        [Tooltip("숨결 색 (공격 연출 등에 사용)")]
        [SerializeField] Color breathColor = new Color(1f, 0.6f, 0.1f);
        [Tooltip("결투 시작 때의 최대 체력")]
        [Min(1)]
        [SerializeField] int maxHealth = 100;

        [Header("결투 그림 (208×144, 왼쪽을 바라봄)")]
        [Tooltip("결투 중 기본 모습 (비행 기본)")]
        [SerializeField] Sprite battleIdle;
        [Tooltip("숨결 공격")]
        [SerializeField] Sprite battleAttack;
        [Tooltip("레몬즙을 맞았을 때 (피격)")]
        [SerializeField] Sprite battleHurt;

        [Header("필드 그림 (88×64)")]
        [Tooltip("보드 위에서 잠든 모습")]
        [SerializeField] Sprite fieldSleeping;
        [Tooltip("빛나는 잠든 모습 — 결투 직전 둥지 위에서 깨어날 때 사용")]
        [SerializeField] Sprite fieldSleepingGlow;

        [Header("둥지 · 소리")]
        [Tooltip("이 드래곤의 둥지 그림 묶음 (결투 중 둥지, 정화된 둥지)")]
        [SerializeField] NestSpriteSet nestSprites;
        [Tooltip("등장할 때 울음소리 (비워 두면 생략)")]
        [SerializeField] AudioClip cry;

        [Header("결투 수치 (다음 단계에서 사용)")]
        [Tooltip("공격력 (카드 결투 단계에서 사용)")]
        [Min(0)]
        [SerializeField] int attackPower;

        [Header("재료 드랍표 (다음 단계에서 사용)")]
        [Tooltip("결투에서 이겼을 때 떨어뜨릴 수 있는 재료들")]
        [SerializeField] List<MaterialDrop> materialDrops = new List<MaterialDrop>();

        public int Number => number;
        public string DisplayName => displayName;
        public Color BreathColor => breathColor;
        public int MaxHealth => maxHealth;
        public Sprite BattleIdle => battleIdle;
        public Sprite BattleAttack => battleAttack;
        public Sprite BattleHurt => battleHurt;
        public Sprite FieldSleeping => fieldSleeping;
        public Sprite FieldSleepingGlow => fieldSleepingGlow;
        public NestSpriteSet NestSprites => nestSprites;
        public AudioClip Cry => cry;
        public int AttackPower => attackPower;
        public IReadOnlyList<MaterialDrop> MaterialDrops => materialDrops;
    }
}
