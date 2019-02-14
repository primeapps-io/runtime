using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace PrimeApps.Studio.Helpers
{
    public class ConvertHelper
    {
        public const string mark = "\\u001b\\[.*?m";
        public static JObject Style = new JObject
        {
            ["\u001b[1m"] = new JObject
            {
                ["html_begin"] = "<b>",
                ["end_tag"] = "\u001b[22m",
                ["html_end"] = "</b>"
            },
            ["\u001b[4m"] = new JObject
            {
                ["html_begin"] = "<u>",
                ["end_tag"] = "\u001b[24m",
                ["html_end"] = "</u>"
            },
            ["\u001b[3m"] = new JObject
            {
                ["html_begin"] = "<i>",
                ["end_tag"] = "\u001b[23m",
                ["html_end"] = "</i>"
            },
            ["\u001b[4m"] = new JObject
            {
                ["html_begin"] = "<u>",
                ["end_tag"] = "\u001b[24m",
                ["html_end"] = "</u>"
            },
            ["\u001b[5m"] = new JObject
            {
                ["html_begin"] = "<blink>",
                ["end_tag"] = "\u001b[25m",
                ["html_end"] = "</blink>"
            },
            ["\u001b[8m"] = new JObject
            {
                ["html_begin"] = "<div style=\"display:none;display: inline-block;\"",
                ["end_tag"] = "\u001b[28m",
                ["html_end"] = "</div>"
            },
            ["\u001b[9m"] = new JObject
            {
                ["html_begin"] = "<strike>",
                ["end_tag"] = "\u001b[29m",
                ["html_end"] = "</strike>"
            },
            ["\u001b[30m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:black;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[90m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#e0dede;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[40m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:black;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[100m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#e0dede;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[31m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:red;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[91m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#e08f8f;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[41m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:red;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[101m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#e08f8f;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[32m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:green;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[92m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#94e08f;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[42m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:green;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[102m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#94e08f;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[33m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:yellow;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[93m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#efef99;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[43m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:yellow;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[103m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#efef99;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[34m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:blue;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[94m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#9db6f2;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[44m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:blue;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[104m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#9db6f2;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[36m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#00FFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[96m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#E0FFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[46m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#00FFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[106m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#E0FFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[37m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#FFFFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[97m"] = new JObject
            {
                ["html_begin"] = "<div style=\"color:#FFFFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[39m",
                ["html_end"] = "</div>"
            },
            ["\u001b[47m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#FFFFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            },
            ["\u001b[107m"] = new JObject
            {
                ["html_begin"] = "<div style=\"background-color:#FFFFFF;display: inline-block;\">",
                ["end_tag"] = "\u001b[49m",
                ["html_end"] = "</div>"
            }
        };

        public static string ASCIIToHTML(string value)
        {
            var convert = true;
            while (convert)
            {
                var code = Regex.Match(value, mark);

                if (code.Success)
                {
                    var codeValue = code.Value;
                    value = value.Replace(codeValue.ToString(), Style[codeValue]["html_begin"].ToString());
                    value = value.Replace(Style[codeValue]["end_tag"].ToString(), Style[codeValue]["html_end"].ToString());
                }
                else
                {
                    convert = false;
                }
            }

            return value;
        }
    }
}
