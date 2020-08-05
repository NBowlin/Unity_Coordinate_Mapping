using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaneSelector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayInfo;
    private GameObject selected;

    private void Start() {
        DisplayFlightInfo(null);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Jump")) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction, Color.green, 10f);
            if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("Location"))) {
                if (hit.collider != null) {
                    UpdateSelectedPoint(hit.collider.gameObject);
                    var airplane = hit.collider.gameObject.GetComponent<Airplane_Realtime>();
                    DisplayFlightInfo(airplane.info);
                }
            }
            else {
                UpdateSelectedPoint(null);
                DisplayFlightInfo(null);
            }
        }
    }

    private void UpdateSelectedPoint(GameObject point) {
        //UpdateFresnelPower(selected, 0.4f); //Set old point back
        //UpdateFresnelPower(point, 8.0f); //Update new point
        selected = point; //Track new point
    }

    private void UpdateFresnelPower(GameObject point, float power) {
        if (point == null) { return; }
        var mat = point.GetComponentInChildren<MeshRenderer>().material;
        mat.SetFloat("Fresnel_Power", power);
    }

    private void DisplayFlightInfo(FlightInfo info) {
        displayInfo.text = info == null ? "" : info.DisplayInfo();
        displayInfo.transform.parent.gameObject.SetActive(info != null);
    }
}
