using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateMapper {
    public enum DataKeyFormat {
        //JsonDefaultModel, //JSON matches format outlined by my models
        JsonLatAndLngKeys, //JSON has seperate Latitude and Longitude keys in objects for each location
        JsonSingleLatLngArray, //JSON has a single array with alternating Latitude and Longitude numbers
        JsonLatLngArrays, //JSON has two arrays, one for Latitude and one for Longitude
        Csv //CSV parsing
    };

    public class DefaultVisualizer : MonoBehaviour, IDataLoader {

        [SerializeField] private TextAsset _dataFile;
        public TextAsset dataFile { get { return _dataFile; } set { _dataFile = value; } }

        [SerializeField] private GameObject pointPrefab = null; //TODO: Keep this?

        [SerializeField] private DataKeyFormat keyFormat = DataKeyFormat.JsonLatAndLngKeys;

        [SerializeField] private string latitudeKey = null;
        [SerializeField] private string longitudeKey = null;
        [SerializeField] private string magnitudeKey = null;

        [SerializeField] private DataLoadedEvent _loadComplete;
        public DataLoadedEvent loadComplete { get { return _loadComplete; } set { _loadComplete = value; } }

        // Start is called before the first frame update
        void Start() {
            ParseFile(dataFile.text);
        }

        public async void ParseFile(string fileText) {
            var parser = new DefaultParser(fileText, keyFormat, latitudeKey, longitudeKey, magnitudeKey);
            var infos = await parser.HandleDefaultParsing();

            var pointsContainer = new GameObject("Points Container");
            pointsContainer.transform.SetParent(transform, false);

            for(int i = 0; i < infos.Count; i++) {
                var info = infos[i];
                info.pointPrefab = pointPrefab;
                var plotted = info.Plot(transform, pointsContainer.transform, 0);
                plotted.name = "Default Point " + i;
            }

            if (loadComplete != null) { loadComplete.Invoke(infos); }
        }
    }
}
