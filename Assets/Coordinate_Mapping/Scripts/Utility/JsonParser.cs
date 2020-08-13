using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using System.Linq;
using Newtonsoft.Json.Linq;

namespace CoordinateMapper {
    public class JsonParser {
        
        public static object CastToken(JToken token) {
            switch (token.Type) {
                case JTokenType.Integer:
                    return (long)token; //TODO: Think about handling ints better
                    //return (int)token; //TODO: Add option to JsonParser to specify cast ints to float? (CSV Parser also)
                case JTokenType.Float:
                    return (float)token;
                case JTokenType.String:
                    return (string)token;
                case JTokenType.Array:
                    var arr = ((JArray)token).Children().ToArray();
                    var tokenArr = new object[arr.Length];
                    for(int i = 0; i < tokenArr.Length; i++) {
                        var t = arr[i];
                        tokenArr[i] = CastToken(t);
                    }
                    return tokenArr;
                default: return new object();
            }
        }

        public async static Task<Dictionary<string, object[]>> ParseAsync(string json, string[] keys) {
            return await Task.Run(() => Parse(json, keys));
        }

        public static Dictionary<string, object[]> Parse(string json, string[] keys) {
            var properJson = CheckRootObject(json);
            var parsedJson = JObject.Parse(properJson);
            var allTokens = parsedJson.AllTokens();

            var parsedInfo = new Dictionary<string, object[]>();

            foreach (string key in keys) {
                var tokens = allTokens.Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == key).Select(p => ((JProperty)p).Value).ToArray();
                var values = new object[tokens.Length];
                for (int i = 0; i < values.Length; i++) {
                    var t = tokens[i];
                    values[i] = CastToken(t);
                }
                parsedInfo[key] = values;
            }

            return parsedInfo;
        }

        private static string CheckRootObject(string json) {
            //JsonUtility can't parse json with arrays as the root object
            //So if we find an array, wrap it in an object 'data'
            return json[0] == '[' ? "{ \"data\": " + json + "}" : json;
        }
    }
}
