using UnityEngine;

namespace Dramon
{
    /// <summary>판이 끝났을 때 보여 줄 화면</summary>
    public enum ModeEndScreen
    {
        [InspectorName("엔딩 (보스 처치)")] Ending,
        [InspectorName("기록 (최고 스테이지)")] Record,
    }

    /// <summary>모드가 열리는 조건</summary>
    public enum ModeUnlock
    {
        [InspectorName("처음부터")] Always,
        [InspectorName("일반 모드 클리어 후")] NormalCleared,
    }

    /// <summary>게임 모드 (일반·무한). 두 모드는 같은 시스템을 쓰고 이 데이터만 다르다.</summary>
    [CreateAssetMenu(fileName = "GameMode", menuName = "Dramon/Game Mode")]
    public class GameModeData : ScriptableObject
    {
        [Header("표시")]
        [SerializeField] string displayName = "일반 모드";
        [Tooltip("모드 선택 화면 설명")]
        [TextArea(1, 3)] [SerializeField] string description = "5 스테이지를 지나 숲의 주인을 쓰러뜨려요";
        [Tooltip("잠겨 있을 때 보여 줄 문구")]
        [SerializeField] string lockedMessage = "";

        [Header("규칙")]
        [Tooltip("스테이지 구성 공식")]
        [SerializeField] StageRules stageRules;
        [Tooltip("마지막 스테이지 (0 = 끝없이)")]
        [Min(0)] [SerializeField] int lastStage = 5;
        [Tooltip("켜면 마지막 스테이지 끝에 보스전")]
        [SerializeField] bool useBoss = true;
        [Tooltip("보스 규칙 (보스를 쓸 때)")]
        [SerializeField] BossRules bossRules;
        [Tooltip("체력 증가 강화 최대 단계 (0 = 강화 데이터 값 그대로, 무한 모드는 크게)")]
        [Min(0)] [SerializeField] int maxHealthUpgradeLevel;

        [Header("끝·열림")]
        [Tooltip("판이 끝나면 보여 줄 화면")]
        [SerializeField] ModeEndScreen endScreen = ModeEndScreen.Ending;
        [Tooltip("이 모드가 열리는 조건")]
        [SerializeField] ModeUnlock unlock = ModeUnlock.Always;

        public string DisplayName => displayName;
        public string Description => description;
        public string LockedMessage => lockedMessage;
        public StageRules StageRules => stageRules;
        public int LastStage => lastStage;
        public bool IsEndless => lastStage <= 0;
        public bool UseBoss => useBoss && !IsEndless;
        public BossRules BossRules => bossRules;
        public int MaxHealthUpgradeLevel => maxHealthUpgradeLevel;
        public ModeEndScreen EndScreen => endScreen;
        public ModeUnlock Unlock => unlock;
    }
}
