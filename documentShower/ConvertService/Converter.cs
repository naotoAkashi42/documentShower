using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertService
{
    public class Converter
    {
        public static void Convert(string jsonText, List<ReplaceString> replaceStrings, out string result)
        {
            var convertedJson = format_json(jsonText);

            result = Replace(jsonText, replaceStrings);
        }

        private static string format_json(string jsonText)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(jsonText);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private static string Replace(string jsonText, List<ReplaceString> replaceStrings)
        {
            jsonText = jsonText.Replace("\"", "\"\"");

            replaceStrings.ForEach(str =>
            {
                jsonText = jsonText.Replace(str.From, str.To);
            });
            return jsonText;
        }
    }


    public  class ReplaceString
    {
        private string _from;
        private string _to;
        public ReplaceString(string from, string to)
        {
            _from = from;
            _to = to;
        }

        public string From { get { return _from; } }
        public string To { get { return _to; } }
        public int FromLen => _from.Length;

        public string DispString()
        {
            return From + " -> " + To;
        }

        public override string ToString()
        {
            return From + "," + To;
        }
    }
}
