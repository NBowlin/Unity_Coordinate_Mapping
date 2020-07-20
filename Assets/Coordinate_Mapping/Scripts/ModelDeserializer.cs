using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CoordinateMapper {
    //Note: Deserializing straight to model only works with JSON and the JSON format must match your model properly
    public class ModelDeserializer : MonoBehaviour {
        [SerializeField] private TextAsset jsonFile;
        [SerializeField] private string model; //Interfaces and abstract classes don't show in inspector

        void Start() {
            //Type t = Type.GetType(model);
            //JsonUtility.FromJson(jsonFile.text, t);
        }

        /*public List<T> ParseJson(string json) {
            var properJson = CheckRootObject(json);
            return null;
            //var allPoints = JsonUtility.FromJson<JsonDataLoader<T>>(properJson);
            //return allPoints.data;
        }*/

        private string CheckRootObject(string json) {
            //JsonUtility can't parse json with arrays as the root object
            //So if we find an array, wrap it in an object 'data'
            return json[0] == '[' ? "{ \"data\": " + json + "}" : json;
        }
    }
}
