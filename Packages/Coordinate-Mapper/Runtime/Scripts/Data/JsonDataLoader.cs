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
            var latProperties = tokens.Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == latitudeKey).Select(p => (JProperty)p).ToList();
            var lngProperties = tokens.Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == longitudeKey).Select(p => (JProperty)p).ToList();

            switch (parseStyle) {
                case JsonParseStyle.LatAndLngKeys:
                    return ParseSeparateLatLngKeys(latProperties, lngProperties);
                case JsonParseStyle.LatLngArrays:
                    return ParseSeparateLatLngArrays(latProperties, lngProperties);
                case JsonParseStyle.SingleLatLngArray:
                    return ParseAlternatingLatLngArray(latProperties);
                default:
                    return new List<T>();
            }
        }

        private static List<T> ParseSeparateLatLngKeys(List<JProperty> latProperties, List<JProperty> lngProperties) {

            var latVals = latProperties.Select(v => v.Value).ToList();
            var lngVals = lngProperties.Select(v => v.Value).ToList();

            //TODO: Just return null if empty?
            return CreatePointsFromLatLngs(latVals, lngVals) ?? new List<T>();
        }

        private static List<T> ParseSeparateLatLngArrays(List<JProperty> latArrayProperties, List<JProperty> lngArrayProperties) {
            if (latArrayProperties.Count != lngArrayProperties.Count) {
                Debug.Log("Different number of latitude and longitude arrays");
                return null;
            }

            var allPoints = new List<T>();

            for (int i = 0; i < latArrayProperties.Count; i++) {
                var latArr = (JArray)latArrayProperties[i].Value;
                var lngArr = (JArray)lngArrayProperties[i].Value;

                var latList = latArr.Children().ToList();
                var lngList = lngArr.Children().ToList();

                var setPoints = CreatePointsFromLatLngs(latList, lngList);
                if(setPoints?.Count > 0) { allPoints.AddRange(setPoints); }
            }

            return allPoints;
        }

        private static List<T> ParseAlternatingLatLngArray(List<JProperty> coordsArrayProperties) {
            var allPoints = new List<T>();

            for (int i = 0; i < coordsArrayProperties.Count; i++) {

                var coordsArr = (JArray)coordsArrayProperties[i].Value;

                if (coordsArr.Count % 2 != 0) {
                    Debug.Log("Different number of latitude and longitude in alternating array -- Skipping set");
                    continue;
                }

                var latVals = coordsArr.Children().Where((v, index) => index % 2 == 0).ToList();
                var lngVals = coordsArr.Children().Where((v, index) => index % 2 == 1).ToList();

                var setPoints = CreatePointsFromLatLngs(latVals, lngVals);
                if (setPoints?.Count > 0) { allPoints.AddRange(setPoints); }
            }

            return allPoints;
        }

        private static List<T> CreatePointsFromLatLngs(List<JToken> lats, List<JToken> lngs) {

            var allPoints = new List<T>();

            if (lats.Count != lngs.Count) {
                Debug.Log("Different amount of latitudes and longitudes?");
                return null;
            }

            for (int j = 0; j < lats.Count; j++) {
                var latVal = lats[j];
                var lngVal = lngs[j];

                float lat = 0f;
                float lng = 0f;

                //TODO: Should we assume that lat and lng use the same type?
                switch (latVal.Type) {
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
