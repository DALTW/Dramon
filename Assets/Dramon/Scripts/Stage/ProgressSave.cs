using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 기록 저장 (PlayerPrefs): 일반 모드 클리어 여부, 최고 클리어 시간, 마지막으로 쓰러뜨린 보스, 무한 모드 최고 스테이지.
    /// </summary>
    public class ProgressSave : MonoBehaviour
    {
        [Header("저장 키 (PlayerPrefs)")]
        [SerializeField] string keyPrefix = "Dramon.";

        [Header("테스트 (에디터에서만)")]
        [Tooltip("켜면 저장과 상관없이 무한 모드가 열려요 (에디터에서만)")]
        [SerializeField] bool testUnlockEndless;

        string K(string name) => keyPrefix + name;

        public bool NormalCleared => PlayerPrefs.GetInt(K("NormalCleared"), 0) == 1 || (Application.isEditor && testUnlockEndless);
        /// <summary>최고 클리어 시간(초). 기록이 없으면 0</summary>
        public float BestClearSeconds => PlayerPrefs.GetFloat(K("BestClearSeconds"), 0f);
        public string LastBoss => PlayerPrefs.GetString(K("LastBoss"), "");
        public int EndlessBestStage => PlayerPrefs.GetInt(K("EndlessBestStage"), 0);

        public bool IsUnlocked(GameModeData mode) => mode == null || mode.Unlock == ModeUnlock.Always || NormalCleared;

        /// <summary>일반 모드 클리어 기록. (최고 기록인지, 이번에 무한 모드가 처음 열렸는지)</summary>
        public (bool best, bool unlocked) RecordNormalClear(float seconds, string bossName)
        {
            bool unlocked = PlayerPrefs.GetInt(K("NormalCleared"), 0) != 1;
            float best = BestClearSeconds;
            bool isBest = best <= 0f || seconds < best;
            PlayerPrefs.SetInt(K("NormalCleared"), 1);
            if (isBest) PlayerPrefs.SetFloat(K("BestClearSeconds"), seconds);
            PlayerPrefs.SetString(K("LastBoss"), bossName ?? "");
            PlayerPrefs.Save();
            return (isBest, unlocked);
        }

        /// <summary>무한 모드에서 도달한 스테이지 기록. 최고 기록이면 true</summary>
        public bool RecordEndlessStage(int stage)
        {
            if (stage <= EndlessBestStage) return false;
            PlayerPrefs.SetInt(K("EndlessBestStage"), stage);
            PlayerPrefs.Save();
            return true;
        }

        // ── 테스트 (에디터에서만) ─────────────
        public void TestUnlockEndless()
        {
            if (Application.isEditor) testUnlockEndless = true;
        }

        public void TestClearAll()
        {
            if (!Application.isEditor) return;
            foreach (var n in new[] { "NormalCleared", "BestClearSeconds", "LastBoss", "EndlessBestStage" }) PlayerPrefs.DeleteKey(K(n));
            PlayerPrefs.Save();
            testUnlockEndless = false;
        }
    }
}
