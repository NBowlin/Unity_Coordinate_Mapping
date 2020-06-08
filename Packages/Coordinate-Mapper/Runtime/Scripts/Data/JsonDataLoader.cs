using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;

namespace CoordinateMapper.Data {
    [System.Serializable] public class JsonDataLoader<T> where T : CoordinatePoint {
        public List<T> data;

        public static List<T> ParseJson(string json) {
            var allPoints = JsonUtility.FromJson<JsonDataLoader<T>>(json);
            return allPoints.data;
        }
    }
}
