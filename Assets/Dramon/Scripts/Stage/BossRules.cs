using UnityEngine;

namespace Dramon
{
    /// <summary>일반 모드 보스 규칙 (그 종의 마지막 스테이지 값 기준 배율, 분노, 이름, 힌트)</summary>
    [CreateAssetMenu(fileName = "BossRules", menuName = "Dramon/Boss Rules")]
    public class BossRules : ScriptableObject
    {
        [Header("능력 (마지막 스테이지 값 기준)")]
        [Tooltip("체력 배율 (5 스테이지 값 × 1.8)")]
        [Min(0.1f)] [SerializeField] float healthMultiplier = 1.8f;
        [Tooltip("할퀴기·연타·숨결 피해 배율 (5 스테이지 값 × 1.2)")]
        [Min(0.1f)] [SerializeField] float attackMultiplier = 1.2f;

        [Header("분노 (2단계)")]
        [Tooltip("체력이 이 비율 아래로 떨어지면 분노 (0.5 = 절반)")]
        [Range(0f, 1f)] [SerializeField] float enrageHealthRatio = 0.5f;
        [Tooltip("분노하면 행동 순서에 추가되는 필살 숨결의 이름")]
        [SerializeField] string finisherName = "필살 숨결";
        [Tooltip("필살 숨결 피해 = 그 종의 숨결 피해 × 이 값")]
        [Min(0.1f)] [SerializeField] float finisherDamageMultiplier = 1.5f;
        [Tooltip("필살 숨결 상태 수치 = 그 종의 숨결 상태 × 이 값")]
        [Min(0.1f)] [SerializeField] float finisherStatusMultiplier = 2f;
        [Tooltip("분노할 때 메시지. {0} = 보스 이름")]
        [SerializeField] string enrageMessage = "{0}이(가) 분노했다!";

        [Header("모습")]
        [Tooltip("보스 드래곤 크기 배율 (결투 화면)")]
        [Min(0.1f)] [SerializeField] float sizeMultiplier = 1.25f;
        [Tooltip("보스 이름. {0} = 드래곤 이름")]
        [SerializeField] string nameFormat = "숲의 주인 – {0}";

        [Header("힌트")]
        [Tooltip("이 스테이지부터 보스 힌트 메시지를 보여 줘요 (0 = 안 보임)")]
        [Min(0)] [SerializeField] int hintFromStage = 3;

        public float HealthMultiplier => healthMultiplier;
        public float AttackMultiplier => attackMultiplier;
        public float EnrageHealthRatio => enrageHealthRatio;
        public string FinisherName => finisherName;
        public float FinisherDamageMultiplier => finisherDamageMultiplier;
        public float FinisherStatusMultiplier => finisherStatusMultiplier;
        public string EnrageMessage => enrageMessage;
        public float SizeMultiplier => sizeMultiplier;
        public string NameFormat => nameFormat;
        public int HintFromStage => hintFromStage;
    }
}
