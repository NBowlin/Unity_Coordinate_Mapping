using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform lookAt;

    [SerializeField] private float speed;
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;

    private Vector3 previousPos;
    private float cameraDistance;

    // Start is called before the first frame update
    void Start()
    {
        StoreCameraDistance();
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

    private void ZoomCamera(bool zoomingIn) {
        if(!CheckCameraDistance(zoomingIn)) { return; }
        var dir = (lookAt.position - cam.transform.position);
        cam.transform.position += (dir * (zoomingIn ? 1f : -1f)) * speed * Time.deltaTime;
        StoreCameraDistance();
    }

    private bool CheckCameraDistance(bool zoomingIn) {
        RaycastHit hit;
        var lookAtDir = (lookAt.transform.position - cam.transform.position).normalized;

        Debug.DrawLine(cam.transform.position + cam.transform.up * 0.1f, (cam.transform.position + cam.transform.up * 0.1f) + cam.transform.forward, Color.blue, 0.1f);
        Debug.DrawLine(cam.transform.position - cam.transform.up * 0.1f, (cam.transform.position - cam.transform.up * 0.1f) + cam.transform.forward * 8f, Color.red, 0.1f);

        if (Physics.Raycast(cam.transform.position, lookAtDir, out hit, zoomingIn ? minRange : maxRange)) {
            if (hit.collider.gameObject.tag == "Earth") { return zoomingIn ? false : true; }
        } else if(!zoomingIn) { return false; }
        
        return true;
    }

    private void PanCamera() {
        var vp = cam.ScreenToViewportPoint(Input.mousePosition);
        Vector3 dir = previousPos - vp;

        //Jump camera to the object we are rotating around and do rotation
        cam.transform.position = lookAt.position; 
        cam.transform.Rotate(Vector3.right, dir.y * 180);
        cam.transform.Rotate(Vector3.up, -dir.x * 180, Space.World);

        //Snap camera back to proper distance
        var distAngle = -cam.transform.forward * cameraDistance;
        cam.transform.position = distAngle;

        previousPos = vp;
    }
}
