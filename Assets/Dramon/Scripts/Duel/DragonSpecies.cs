using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤 한 종류의 데이터: 이름, 체력, 그림, 둥지 그림, 결투 둥지, 울음소리, 재료 드랍표. 나올 드래곤은 DragonSpawnList에 넣어서 정한다.</summary>
    [CreateAssetMenu(fileName = "DragonSpecies", menuName = "Dramon/Dragon Species")]
    public class DragonSpecies : ScriptableObject
    {
        /// <summary>결투에서 이겼을 때 떨어뜨리는 재료 한 줄. 재료 정보(이름·아이콘·판매가)는 MaterialData에, 확률·개수는 여기에.</summary>
        [Serializable]
        public class MaterialDrop
        {
            [Tooltip("떨어뜨릴 재료 (Settings/Materials의 MaterialData)")]
            public MaterialData material;
            [Tooltip("떨어뜨릴 확률 (0~1). 재료마다 따로 굴려요")]
            [Range(0f, 1f)] public float chance = 0.5f;
            [Tooltip("나올 때 최소 개수")]
            [Min(1)] public int minCount = 1;
            [Tooltip("나올 때 최대 개수 (최소와 같으면 항상 그 개수)")]
            [Min(1)] public int maxCount = 1;
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

        [Header("결투 그림 (왼쪽을 바라봄, 모션 프레임이 없을 때 사용)")]
        [Tooltip("결투 중 기본 모습 (비행 기본)")]
        [SerializeField] Sprite battleIdle;
        [Tooltip("숨결 공격")]
        [SerializeField] Sprite battleAttack;
        [Tooltip("레몬즙을 맞았을 때 (피격)")]
        [SerializeField] Sprite battleHurt;

        [Header("모션 프레임 (비어 있으면 위의 결투 그림 1장씩 사용)")]
        [Tooltip("날갯짓 비행 프레임 (반복 재생, MotionSheet 1행 Fly_0~3)")]
        [SerializeField] Sprite[] flyFrames = new Sprite[0];
        [Tooltip("공격 프레임 (준비 → 돌진 → 숨결 → 복귀, 한 번 재생 후 비행으로)")]
        [SerializeField] Sprite[] attackFrames = new Sprite[0];
        [Tooltip("피격 프레임 (피격 → 움찔, 한 번 재생 후 비행으로)")]
        [SerializeField] Sprite[] hurtFrames = new Sprite[0];
        [Tooltip("패배 프레임 (쓰러지는 중 → 쓰러짐, 마지막 프레임에서 멈춤)")]
        [SerializeField] Sprite[] defeatFrames = new Sprite[0];
        [Tooltip("비행 초당 프레임 수 (클수록 날갯짓이 빨라요)")]
        [Min(0.1f)]
        [SerializeField] float flyFps = 8f;
        [Tooltip("공격 초당 프레임 수")]
        [Min(0.1f)]
        [SerializeField] float attackFps = 10f;
        [Tooltip("피격 초당 프레임 수")]
        [Min(0.1f)]
        [SerializeField] float hurtFps = 8f;
        [Tooltip("패배 초당 프레임 수")]
        [Min(0.1f)]
        [SerializeField] float defeatFps = 6f;

        [Header("비행 흔들림")]
        [Tooltip("날갯짓에 맞춰 몸이 위아래로 움직이는 거리 (픽셀, 0이면 흔들리지 않음)")]
        [Min(0f)]
        [SerializeField] float hoverAmplitude = 4f;
        [Tooltip("한 번 오르내리는 데 걸리는 시간 (초)")]
        [Min(0.05f)]
        [SerializeField] float hoverPeriod = 1.2f;

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

        [Header("결투 둥지")]
        [Tooltip("결투 화면 배경. 비워 두면 기본 배경(BattleBG_ForestNight)을 써요")]
        [SerializeField] Sprite duelBackground;
        [Tooltip("켜면 아래 두 발판 위치에 드래곤과 플레이어를 세워요. 끄면 Scene에 배치된 위치를 그대로 써요")]
        [SerializeField] bool useStandPoints;
        [Tooltip("드래곤 발밑이 닿을 둥지 윗면 가운데 (배경 왼쪽 아래 = 0,0 / 오른쪽 위 = 1,1)")]
        [SerializeField] Vector2 dragonStandPoint = new Vector2(0.72f, 0.56f);
        [Tooltip("플레이어 발밑이 닿을 작은 발판 윗면 가운데 (배경 왼쪽 아래 = 0,0 / 오른쪽 위 = 1,1)")]
        [SerializeField] Vector2 playerStandPoint = new Vector2(0.22f, 0.22f);
        [Tooltip("이 드래곤의 결투 그림 크기 배율 (결투 화면 DragonBattlerView의 크기 배율과 곱해져요). 둥지에 맞게 드래곤마다 조절")]
        [Range(0.3f, 2f)]
        [SerializeField] float battleSizeMultiplier = 1f;

        [Header("결투 수치 (다음 단계에서 사용)")]
        [Tooltip("공격력 (카드 결투 단계에서 사용)")]
        [Min(0)]
        [SerializeField] int attackPower;

        [Header("재료 드랍표")]
        [Tooltip("결투에서 이겼을 때 떨어뜨릴 수 있는 재료들 (줄마다 확률을 따로 굴려요)")]
        [SerializeField] List<MaterialDrop> materialDrops = new List<MaterialDrop>();

        public int Number => number;
        public string DisplayName => displayName;
        public Color BreathColor => breathColor;
        public int MaxHealth => maxHealth;
        public Sprite BattleIdle => battleIdle;
        public Sprite BattleAttack => battleAttack;
        public Sprite BattleHurt => battleHurt;
        public Sprite[] FlyFrames => flyFrames;
        public Sprite[] AttackFrames => attackFrames;
        public Sprite[] HurtFrames => hurtFrames;
        public Sprite[] DefeatFrames => defeatFrames;
        public float FlyFps => flyFps;
        public float AttackFps => attackFps;
        public float HurtFps => hurtFps;
        public float DefeatFps => defeatFps;
        public float HoverAmplitude => hoverAmplitude;
        public float HoverPeriod => hoverPeriod;
        public bool HasFlyFrames => HasFrames(flyFrames);

        /// <summary>프레임 배열에 그림이 한 장이라도 있는지</summary>
        public static bool HasFrames(Sprite[] frames)
        {
            if (frames == null) return false;
            foreach (var f in frames) if (f != null) return true;
            return false;
        }
        public Sprite FieldSleeping => fieldSleeping;
        public Sprite FieldSleepingGlow => fieldSleepingGlow;
        public NestSpriteSet NestSprites => nestSprites;
        public AudioClip Cry => cry;
        public Sprite DuelBackground => duelBackground;
        public bool UseStandPoints => useStandPoints;
        public Vector2 DragonStandPoint => dragonStandPoint;
        public Vector2 PlayerStandPoint => playerStandPoint;
        public float BattleSizeMultiplier => battleSizeMultiplier;
        public int AttackPower => attackPower;
        public IReadOnlyList<MaterialDrop> MaterialDrops => materialDrops;
    }
}
