namespace Dramon
{
    /// <summary>상태 효과의 한국어 이름 (문구·떠오르는 숫자용)</summary>
    public static class StatusNames
    {
        public static string Korean(StatusType type)
        {
            switch (type)
            {
                case StatusType.Sour: return "산미";
                case StatusType.Burn: return "화상";
                case StatusType.Poison: return "중독";
                case StatusType.Shock: return "감전";
                case StatusType.Frozen: return "빙결";
                case StatusType.Drenched: return "젖음";
                case StatusType.Fear: return "공포";
                case StatusType.Bleed: return "출혈";
                default: return "";
            }
        }
    }
}
