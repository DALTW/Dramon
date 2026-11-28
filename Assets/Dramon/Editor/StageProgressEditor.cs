using UnityEditor;
using UnityEngine;

namespace Dramon.EditorTools
{
    /// <summary>StageProgress Inspector 아래에 Play 중 테스트 버튼과 지금 스테이지 값</summary>
    [CustomEditor(typeof(StageProgress))]
    public class StageProgressEditor : Editor
    {
        int jumpStage = 2;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var p = (StageProgress)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("지금 스테이지 값", EditorStyles.boldLabel);
            if (p.Rules != null)
            {
                var v = Application.isPlaying ? p.Current : p.Rules.Get(1);
                EditorGUILayout.HelpBox(
                    $"{v.stage} 스테이지: 숲 {v.width}×{v.height} ({v.Cells}칸) · 둥지 {v.nests} · 깨어난 둥지 {v.awakened} · 최대 에너지 {v.maxEnergy} · 뽑기 {v.draw}" +
                    (Application.isPlaying ? $"\n이번 스테이지 결투 {p.DuelsInStage}번 · 이번 판 결투 {p.TotalDuels}번" : ""),
                    MessageType.None);
            }
            EditorGUILayout.LabelField("테스트 버튼", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play 중에 눌러요. (기록 지우기는 지금도 돼요)", MessageType.Info);
                if (GUILayout.Button("기록 지우기 (PlayerPrefs)") && p.Save != null) p.Save.TestClearAll();
                return;
            }
            if (GUILayout.Button("스테이지 클리어 (검 사냥 중일 때)")) p.TestClearStage();
            using (new EditorGUILayout.HorizontalScope())
            {
                jumpStage = Mathf.Max(1, EditorGUILayout.IntField("시작 스테이지", jumpStage));
                if (GUILayout.Button("이 스테이지부터 다시 시작")) p.TestRestartAt(jumpStage);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("보스전 바로 시작")) p.TestStartBoss();
                if (GUILayout.Button("골드 +500")) p.TestAddGold(500);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("무한 모드 잠금 해제") && p.Save != null) p.Save.TestUnlockEndless();
                if (GUILayout.Button("기록 지우기") && p.Save != null) p.Save.TestClearAll();
            }
            if (p.Boss != null && p.Boss.Boss != null)
                EditorGUILayout.HelpBox($"이번 판 보스: {p.Boss.BossName}" + (p.Boss.IsActive ? (p.Boss.Enraged ? " (보스전 중 · 분노)" : " (보스전 중)") : ""), MessageType.None);
        }
    }
}
