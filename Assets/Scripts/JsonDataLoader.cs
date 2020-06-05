using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class JsonDataLoader<T>
{
    public List<T> data;

    public static List<T> ParseJson(string json) {
        var allPoints = JsonUtility.FromJson<JsonDataLoader<T>>(json);
        return allPoints.data;
    }
}
