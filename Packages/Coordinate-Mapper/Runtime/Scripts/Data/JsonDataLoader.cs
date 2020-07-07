using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CoordinateMapper.Coordinates;
using CoordinateMapper.Extensions;

namespace CoordinateMapper.Data {
    [System.Serializable] public class JsonDataLoader<T> where T : CoordinatePoint, new() {
        public List<T> data;

        public static List<T> ParseJson(TextAsset file) {
            return ParseJson(file.text);
        }

        public static List<T> ParseJson(string json) {
            var properJson = CheckRootObject(json);

            var allPoints = JsonUtility.FromJson<JsonDataLoader<T>>(properJson);
            return allPoints.data;
        }

        public static List<T> ParseJson(TextAsset file, JsonParseStyle parseStyle, string latitudeKey, string longitudeKey) {
            return ParseJson(file.text);
        }

        //TODO: Is it confusing to have the .Net parser be part of the same file as the Unity parser?
        public static List<T> ParseJson(string json, JsonParseStyle parseStyle, string latitudeKey, string longitudeKey) {
            if(parseStyle == JsonParseStyle.DefaultModel) { return ParseJson(json); }

            var properJson = CheckRootObject(json);

            var parsed = JObject.Parse(properJson);
            var tokens = parsed.AllTokens();

            var allPoints = new List<T>();
            var latProperties = new List<JProperty>();
            var lngProperties = new List<JProperty>();

            switch (parseStyle) {
                case JsonParseStyle.LatAndLngKeys:
                    latProperties = tokens.Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == latitudeKey).Select(p => (JProperty)p).ToList();
                    lngProperties = tokens.Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == longitudeKey).Select(p => (JProperty)p).ToList();
                    break;
                case JsonParseStyle.LatLngArrays:
                    break;
                case JsonParseStyle.SingleLatLngArray:
                    break;
                default:
                    break;
            }

            if (latProperties.Count != lngProperties.Count) {
                Debug.Log("Different amount of latitudes and longitudes?");
                return null;
            }

            for (int i = 0; i < latProperties.Count; i++) {
                var latVal = latProperties[i].Value;
                var lngVal = lngProperties[i].Value;

                float lat = 0f;
                float lng = 0f;

                //TODO: Should we assume that lat and lng use the same type?
                switch(latVal.Type) {
                    case JTokenType.Float:
                    case JTokenType.Integer:
                        lat = (float)latVal;
                        lng = (float)lngVal;
                        break;
                    case JTokenType.String:
                        lat = float.Parse((string)latVal);
                        lng = float.Parse((string)lngVal);
                        break;
                    default:
                        break;
                }

                var cp = new T();
                cp.location = new Location(lat, lng);
                allPoints.Add(cp);
            }

            return allPoints;
        }

        private static string CheckRootObject(string json) {
            //JsonUtility can't parse json with arrays as the root object
            //So if we find an array, wrap it in an object 'data'
            return json[0] == '[' ? "{ \"data\": " + json + "}" : json;
        }
    }
}
