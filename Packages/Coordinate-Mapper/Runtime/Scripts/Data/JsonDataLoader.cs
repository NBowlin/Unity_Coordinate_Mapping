using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;

namespace CoordinateMapper.Data {
    [System.Serializable] public class JsonDataLoader<T> where T : CoordinatePoint {
        public List<T> data;

        public static List<T> ParseJson(TextAsset file) {
            return ParseJson(file.text);
        }

        public static List<T> ParseJson(string json) {
            var properJson = json;
            //JsonUtility can't parse json with arrays as the root object
            //So if we find an array, wrap it in an object 'data'
            if(json[0] == '[') { properJson = "{ \"data\": " + json + "}"; }

            var allPoints = JsonUtility.FromJson<JsonDataLoader<T>>(properJson);
            return allPoints.data;
        }
    }
}
