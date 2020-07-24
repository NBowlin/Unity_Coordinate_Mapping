using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class EarthquakeSelector : MonoBehaviour {
    //[SerializeField] private UnityEvent<string> earthquakeSelected;
    [SerializeField] private TextMeshProUGUI displayInfo;

    private void Start() {
        DisplayEarthquakeInfo(null);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Jump")) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction, Color.green, 10f);
            if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("Location"))) {
                if(hit.collider != null) {
                    var eqPoint = hit.collider.gameObject.GetComponent<EarthquakePoint>();
                    DisplayEarthquakeInfo(eqPoint);
                }
            } else {
                DisplayEarthquakeInfo(null);
            }
        }
    }

    private void DisplayEarthquakeInfo(EarthquakePoint info) {
        displayInfo.text = info == null ? "" : info.DisplayInfo();
        displayInfo.transform.parent.gameObject.SetActive(info != null);

        //if(earthquakeSelected != null) { earthquakeSelected.Invoke(displayInfo); }
    }
}
