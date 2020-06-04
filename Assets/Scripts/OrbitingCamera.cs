using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform lookAt;

    //Pan speed is the degrees we would rotate around the object if we swiped across the entire screen
    [SerializeField] private float minPanSpeed = 20f;
    [SerializeField] private float maxPanSpeed = 180f;

    [SerializeField] private float minCameraDistance = 0.1f;
    [SerializeField] private float maxCameraDistance = 4f;

    [SerializeField] private float zoomSpeed = 1f;

    private Vector3 previousPos;
    private float cameraDistance;
    private float currentPanSpeed;

    // Start is called before the first frame update
    void Start()
    {
        //Jump camera to inital location
        //cam.transform.position = new Vector3(lookAt.transform.position.x, lookAt.transform.position.y, lookAt.transform.position.z - lookAt.transform.localScale.z / 2 - maxCameraDistance);
        StoreCameraDistance();
        CalculatePanSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Adjust keys to zoom
        if (Input.GetButton("Jump")) { ZoomCamera(true); }
        else if (Input.GetButton("Fire2")) { ZoomCamera(false); }    

        if(Input.GetMouseButtonDown(0)) {
            previousPos = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if(Input.GetMouseButton(0)) { PanCamera(); }
    }

    private void StoreCameraDistance() {
        cameraDistance = (lookAt.transform.position - cam.transform.position).magnitude;
    }

    private void CalculatePanSpeed() {
        var dist = CheckCameraDistance();

        var rangePercent = (dist - minCameraDistance) / (maxCameraDistance - minCameraDistance);
        currentPanSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, rangePercent);
    }

    private void ZoomCamera(bool zoomingIn) {
        var dist = CheckCameraDistance();
        Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * (zoomingIn ? minCameraDistance : maxCameraDistance), Color.red);

        if ((zoomingIn && dist <= minCameraDistance) || (!zoomingIn && dist >= maxCameraDistance)) { return; }

        CalculatePanSpeed();

        var dir = (lookAt.position - cam.transform.position);
        cam.transform.position += (dir * (zoomingIn ? 1f : -1f)) * zoomSpeed * Time.deltaTime;

        StoreCameraDistance();
    }

    private float CheckCameraDistance() {
        var distance = PlanetUtility.LineToSurface(lookAt.transform, cam.transform, maxCameraDistance);

        if(distance.HasValue) { return (distance.Value.point - cam.transform.position).magnitude; }
        return maxCameraDistance;
    }

    private void PanCamera() {
        var vp = cam.ScreenToViewportPoint(Input.mousePosition);
        Vector3 dir = previousPos - vp;

        //Jump camera to the object we are rotating around and do rotation
        cam.transform.position = lookAt.position; 
        cam.transform.Rotate(Vector3.right, dir.y * currentPanSpeed);
        cam.transform.Rotate(Vector3.up, -dir.x * currentPanSpeed, Space.World);

        //Snap camera back to proper distance
        var distAngle = cam.transform.position + -cam.transform.forward * cameraDistance;

        cam.transform.position = distAngle;

        previousPos = vp;
    }
}
