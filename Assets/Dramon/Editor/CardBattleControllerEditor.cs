using UnityEditor;
using UnityEngine;

namespace Dramon.EditorTools
{
    /// <summary>CardBattleController Inspector 아래에 Play 중 테스트 버튼</summary>
    [CustomEditor(typeof(CardBattleController))]
    public class CardBattleControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("테스트 버튼", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play 중 결투 화면에서 눌러요. DuelSession의 테스트용 고정 드래곤과 같이 쓰면 드래곤을 하나씩 확인할 수 있어요.", MessageType.Info);
                return;
            }
            var c = (CardBattleController)target;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("드래곤 체력 1")) c.TestDragonHealthToOne();
                if (GUILayout.Button("플레이어 무적 켜기·끄기")) c.TestToggleInvincible();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("에너지 99")) c.TestEnergy99();
                if (GUILayout.Button("테스트 카드 손패에 넣기")) c.TestAddCardToHand();
                if (GUILayout.Button("임시 승패 버튼 보이기·숨기기")) c.TestToggleTempButtons();
            }
            EditorGUILayout.LabelField("상태 효과 주기", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("화상 3")) c.TestAddPlayerStatus(StatusType.Burn, 3);
                if (GUILayout.Button("중독 3")) c.TestAddPlayerStatus(StatusType.Poison, 3);
                if (GUILayout.Button("감전")) c.TestAddPlayerStatus(StatusType.Shock, 1);
                if (GUILayout.Button("빙결")) c.TestAddPlayerStatus(StatusType.Frozen, 1);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("젖음")) c.TestAddPlayerStatus(StatusType.Drenched, 1);
                if (GUILayout.Button("공포 2")) c.TestAddPlayerStatus(StatusType.Fear, 2);
                if (GUILayout.Button("출혈 3")) c.TestAddPlayerStatus(StatusType.Bleed, 3);
                if (GUILayout.Button("드래곤 산미 5")) c.TestAddDragonSour(5);
            }
        }
    }
}
