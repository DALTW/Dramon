using UnityEngine;

namespace Dramon
{
    /// <summary>재료 희귀도</summary>
    public enum MaterialRarity
    {
        [InspectorName("흔함")] Common,
        [InspectorName("보통")] Uncommon,
        [InspectorName("희귀")] Rare,
    }

    /// <summary>
    /// 재료 하나의 정보: 이름, 아이콘, 희귀도, 판매가.
    /// 드래곤이 떨어뜨릴 확률·개수는 DragonSpecies의 재료 드랍표에서 정한다. 상점에서도 이 데이터를 그대로 쓴다.
    /// </summary>
    [CreateAssetMenu(fileName = "MaterialData", menuName = "Dramon/Material Data")]
    public class MaterialData : ScriptableObject
    {
        [Header("재료 정보")]
        [Tooltip("화면에 보일 재료 이름")]
        [SerializeField] string displayName = "재료";
        [Tooltip("재료 아이콘 (DragonMaterials_Sheet_Transparent에서 잘라 낸 그림)")]
        [SerializeField] Sprite icon;
        [Tooltip("희귀도 (흔함 / 보통 / 희귀)")]
        [SerializeField] MaterialRarity rarity = MaterialRarity.Common;

        [Header("상점")]
        [Tooltip("상점에 팔 때 받는 돈")]
        [Min(0)]
        [SerializeField] int sellPrice;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public MaterialRarity Rarity => rarity;
        public int SellPrice => sellPrice;
    }
}
