using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class EarthquakeSelector : MonoBehaviour {
    //[SerializeField] private UnityEvent<string> earthquakeSelected;
    [SerializeField] private TextMeshProUGUI displayInfo = null;

    private GameObject selected;

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
                    UpdateSelectedPoint(hit.collider.gameObject);
                    var eqPoint = hit.collider.gameObject.GetComponent<EarthquakePoint>();
                    DisplayEarthquakeInfo(eqPoint);
                }
            } else {
                UpdateSelectedPoint(null);
                DisplayEarthquakeInfo(null);
            }
        }
    }

    private void UpdateSelectedPoint(GameObject point) {
        UpdateFresnelPower(selected, 0.4f); //Set old point back
        UpdateFresnelPower(point, 8.0f); //Update new point
        selected = point; //Track new point
    }

    private void UpdateFresnelPower(GameObject point, float power) {
        if(point == null) { return; }
        var mat = point.GetComponentInChildren<MeshRenderer>().material;
        mat.SetFloat("Fresnel_Power", power);
    }

    private void DisplayEarthquakeInfo(EarthquakePoint info) {
        displayInfo.text = info == null ? "" : info.DisplayInfo();
        displayInfo.transform.parent.gameObject.SetActive(info != null);

        //if(earthquakeSelected != null) { earthquakeSelected.Invoke(displayInfo); }
    }
}
