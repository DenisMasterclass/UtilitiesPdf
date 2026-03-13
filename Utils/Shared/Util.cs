using System.Globalization;
using System.Text.Json;

namespace Utils.Shared
{
    public static class Util
    {
        public static string ToStringFromDictionary(this string value,string DictionaryName,Dictionary<string, Dictionary<string, string>> dicDictionaries)
        {
            string DictionaryValue = "";
            if (dicDictionaries.TryGetValue(DictionaryName, out var dicDictionary))
            {
                dicDictionary.TryGetValue(value, out DictionaryValue);
            }
            return DictionaryValue;
        }

        public static string ToStringFromDictionary(this int value, string DictionaryName, Dictionary<string, Dictionary<string, string>> dicDictionaries)
        {
            string DictionaryValue = "";
            if (dicDictionaries.TryGetValue(DictionaryName, out var dicDictionary))
            {
                dicDictionary.TryGetValue(value.ToString(), out DictionaryValue);
            }
            return DictionaryValue;
        }

        public static string ToStringFromDictionaryOrDefault(this string value, string DictionaryName, Dictionary<string, Dictionary<string, string>> dicDictionaries)
        {
            string DictionaryValue = "";
            if (dicDictionaries.TryGetValue(DictionaryName, out var dicDictionary))
            {
                if (!dicDictionary.TryGetValue(value, out DictionaryValue))
                {
                    DictionaryValue = value;

                }
            }
            return DictionaryValue;
        }
        public static string ToBrString(this decimal value)
        {
            return value.ToString("0.####################", CultureInfo.GetCultureInfo("pt-BR"));
        }
        public static string ToGbString(this decimal value)
        {
            return value.ToString("0.####################", CultureInfo.GetCultureInfo("en-GB"));
        }
        public static string ToJson(this object obj, bool indentado = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indentado
            };

            return JsonSerializer.Serialize(obj, options);
        }

        public static void ToJsonFile(this object obj, string caminhoArquivo, bool indentado = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indentado
            };

            string json = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(caminhoArquivo, json);
        }


    }
}
