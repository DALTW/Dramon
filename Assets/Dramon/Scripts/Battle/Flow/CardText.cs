using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dramon
{
    /// <summary>카드 설명의 {0}·{0x} 자리를 실제 숫자로 바꾼다. 기본값과 달라진 숫자는 초록색.</summary>
    public static class CardText
    {
        static readonly Regex Token = new Regex(@"\{(\d+)(x?)\}");

        /// <summary>finalValues[i] = 효과 i의 최종 수치 (null이면 기본값)</summary>
        public static string Format(CardData card, IReadOnlyList<int> finalValues = null, string changedColor = "#1E7A1E")
        {
            if (card == null) return "";
            var effects = card.Effects;
            return Token.Replace(card.Description ?? "", m =>
            {
                int i = int.Parse(m.Groups[1].Value);
                if (i < 0 || i >= effects.Count) return m.Value;
                if (m.Groups[2].Value == "x") return effects[i].count.ToString();
                int baseValue = effects[i].value;
                int final = finalValues != null && i < finalValues.Count ? finalValues[i] : baseValue;
                return final != baseValue ? $"<color={changedColor}>{final}</color>" : final.ToString();
            });
        }
    }
}
