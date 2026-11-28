using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dramon
{
    /// <summary>카드 설명의 {0}·{0x} 자리를 실제 숫자로 바꾼다. 기본값보다 커진 숫자는 초록색, 작아진 숫자(공포·젖음)는 빨간색.</summary>
    public static class CardText
    {
        static readonly Regex Token = new Regex(@"\{(\d+)(x?)\}");

        /// <summary>finalValues[i] = 효과 i의 최종 수치, finalCounts[i] = 최종 횟수 (null이면 기본값)</summary>
        public static string Format(CardData card, IReadOnlyList<int> finalValues = null, IReadOnlyList<int> finalCounts = null,
            string upColor = "#1E7A1E", string downColor = "#B02020")
        {
            if (card == null) return "";
            var effects = card.Effects;
            return Token.Replace(card.Description ?? "", m =>
            {
                int i = int.Parse(m.Groups[1].Value);
                if (i < 0 || i >= effects.Count) return m.Value;
                bool isCount = m.Groups[2].Value == "x";
                int baseValue = isCount ? effects[i].count : effects[i].value;
                var finals = isCount ? finalCounts : finalValues;
                int final = finals != null && i < finals.Count ? finals[i] : baseValue;
                if (final > baseValue) return $"<color={upColor}>{final}</color>";
                if (final < baseValue) return $"<color={downColor}>{final}</color>";
                return final.ToString();
            });
        }
    }
}
