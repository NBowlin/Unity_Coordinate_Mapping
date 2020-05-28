using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOnEarth : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab;

    [SerializeField] private float latitude;
    [SerializeField] private float longitude;

    // Start is called before the first frame update
    void Start()
    {
        var point = Quaternion.Euler(0.0f, -longitude, latitude) * Vector3.right;

        var ray = new Ray(Vector3.zero, point * 6.0f);
        ray.origin = ray.GetPoint(6.0f);
        ray.direction = -ray.direction;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.gameObject.tag == "Earth") {
                Instantiate(pointPrefab, hit.point, Quaternion.identity, transform);
            }
        }
        else {
            Debug.Log("Raycast missed Earth");
            Debug.DrawRay(ray.origin, ray.direction * 6.0f, Color.yellow, 1000.0f);
        }
    }
}
