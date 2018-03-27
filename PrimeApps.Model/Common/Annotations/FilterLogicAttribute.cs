using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PrimeApps.Model.Common.Annotations
{
    /// <summary>
    /// Checks filter logic is valid
    /// </summary>
    public class FilterLogicAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (!(value is string))
                return true;

            var input = value.ToString().Trim();

            if (string.IsNullOrEmpty(input))
                return true;

            if (!input.Contains("(") || !input.Contains(")") || !(input.Contains("and") || input.Contains("or")))
                return false;

            var parts = input.Split(' ');

            foreach (var part in parts)
            {
                if (!(part.Contains("(") || part.Contains(")")) && !(part == "and" || part == "or"))
                    return false;
            }

            var digitChars = new List<char>();

            for (var i = 0; i < input.Length; i++)
            {
                var chr = input[i];

                if (char.IsDigit(chr))
                {
                    var charByte = byte.Parse(chr.ToString());

                    if (charByte < 1 || charByte > 9 || digitChars.Contains(chr))
                        return false;

                    digitChars.Add(chr);
                }

                switch (chr)
                {
                    case '(':
                        var nextChr = input[i + 1];

                        if (!char.IsDigit(nextChr) && nextChr != '(')
                            return false;
                        break;
                    case ')':
                        var preChr = input[i - 1];

                        if (!char.IsDigit(preChr) && preChr != ')')
                            return false;
                        break;
                    case 'a':
                        var preOneChrA = input[i - 1];

                        if (preOneChrA != ' ')
                            return false;

                        var nextThreeChr = input.Substring(input.IndexOf(input[i + 1]), 3);

                        if (nextThreeChr != "nd ")//should be "and "
                            return false;
                        break;
                    case 'o':
                        var preOneChrO = input[i - 1];

                        if (preOneChrO != ' ')
                            return false;

                        var nextTwoChr = input.Substring(input.IndexOf(input[i + 1]), 2);

                        if (nextTwoChr != "r ")//should be "or "
                            return false;
                        break;
                }
            }

            var filterClearChars = input.Replace("(", "").Replace(")", "").Replace("and", "").Replace("or", "");
            filterClearChars = Regex.Replace(filterClearChars, @"\s+", "");
            int filterNumbers;

            if (!int.TryParse(filterClearChars, out filterNumbers))
                return false;

            return true;
        }
    }
}