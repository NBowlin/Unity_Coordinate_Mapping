using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private TextAsset jsonFile;

    // Start is called before the first frame update
    void Start()
    {
        var dataPoints = JsonDataLoader<CoordinatePoint_Magnitude>.ParseJson(jsonFile.text);

        var magLocationsContainer = new GameObject("Magnitude Locations");
        magLocationsContainer.transform.SetParent(transform, false);

        foreach (CoordinatePoint_Magnitude point in dataPoints) {
            point.pointPrefab = pointPrefab; //TODO: Rework this
            point.Plot(transform, magLocationsContainer.transform);
        }
    }
}
