using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using CoordinateMapper.Coordinates;

public enum JsonParseStyle {
    DefaultModel,
    LatAndLngKeys,
    SingleLatLngArray,
    LatLngArrays
};

namespace CoordinateMapper.Data {

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
            var points = LoadJson();
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
