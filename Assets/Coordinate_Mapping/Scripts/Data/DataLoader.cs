using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CoordinateMapper {
    public enum JsonParseStyle {
        DefaultModel, //JSON matches format outlined by my models
        LatAndLngKeys, //JSON has seperate Latitude and Longitude keys in objects for each location
        SingleLatLngArray, //JSON has a single array with alternating Latitude and Longitude numbers
        LatLngArrays, //JSON has two arrays, one for Latitude and one for Longitude
        CSV //CSV parsing
    };

    [System.Serializable] public class JsonLoadedEvent : UnityEvent<IEnumerable<CoordinatePoint>> { }

    //TODO: Better way to do this without needing a subclass for each coordinate point type?
    public abstract class DataLoader : MonoBehaviour {

        //[SerializeField] private GameObject pointPrefab;
        [SerializeField] protected TextAsset jsonFile;
        //[SerializeField] private string containerName;

        //TODO: Can I write a property drawer that hides the lat/lng keys depending on which parseStyle chosen?
        [SerializeField] protected JsonParseStyle parseStyle;
        [SerializeField] protected string latitudeKey;
        [SerializeField] protected string longitudeKey;

        [SerializeField] private JsonLoadedEvent loadComplete;

        // Start is called before the first frame update
        void Start() {
            IEnumerable<CoordinatePoint> points;
            if (parseStyle == JsonParseStyle.CSV) {
                /*var csvData = CSVParser.Read(jsonFile.text);
                var csvPoints = new List<CoordinatePoint>();
                foreach (Dictionary<string, object> info in csvData) {
                    var point = new CoordinatePoint_Basic();
                    var lat = info[latitudeKey];
                    var lng = info[longitudeKey];
                    if (!(lat is float) || !(lng is float)) { continue; }
                    var loc = new Location((float)lat, (float)lng);
                    point.location = loc;
                    csvPoints.Add(point);
                }
                points = csvPoints;*/
                points = null;
            } else {
                points = LoadJson();
            }

            if (loadComplete != null) { loadComplete.Invoke(points); }
        }

        //We have to return IEnumerable so we get Covariance and can return subclasses of CoordinatePoint
        //Explanation: https://stackoverflow.com/questions/4652858/why-cant-i-assign-a-listderived-to-a-listbase/4653199
        protected abstract IEnumerable<CoordinatePoint> LoadJson();

        /*private void LoadData(IEnumerable<CoordinatePoint> points) {
            var container = new GameObject(containerName);
            container.transform.SetParent(transform, false);

            foreach (CoordinatePoint point in points) {
                point.pointPrefab = pointPrefab; //TODO: Rework this
                point.Plot(transform, container.transform);
            }
        }*/
    }
}
