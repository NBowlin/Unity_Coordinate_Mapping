using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playground : MonoBehaviour
{
    public GameObject test;

    private int degBetweenLats = 10;
    private int latSegments = 180;

    private int degBetweenLngs = 10;
    private int lngSegments = 360;
    // Start is called before the first frame update
    void Start()
    {
        int latSegmentIncrement = 360 / latSegments;
        for (int i = -90; i <= 90; i += degBetweenLats) {
            for (int j = 0; j < 360; j += latSegmentIncrement) {
                var line = Quaternion.Euler(0.0f, j, i) * Vector3.right;

                //Need to reverse the ray direction because collisions don't work from the inside of a collider
                //So take a point some distance along the line as origin, then reverse the direction
                var ray = new Ray(Vector3.zero, line * 6.0f);
                ray.origin = ray.GetPoint(6.0f);
                ray.direction = -ray.direction;

                RaycastHit hit;
                if(Physics.Raycast(ray, out hit)) {
                    if (hit.collider.gameObject.tag == "Earth") {
                        Instantiate(test, hit.point, Quaternion.identity, transform);
                    }
                }
            }
        }

        int lngSegmentIncrement = 360 / lngSegments;
        for (int i = 0; i <= 180; i += degBetweenLngs) {
            for (int j = 0; j < 360; j += lngSegmentIncrement) {
                var line = Quaternion.Euler(0.0f, i, j) * Vector3.right;

                //Need to reverse the ray direction because collisions don't work from the inside of a collider
                //So take a point some distance along the line as origin, then reverse the direction
                var ray = new Ray(Vector3.zero, line * 6.0f);
                ray.origin = ray.GetPoint(6.0f);
                ray.direction = -ray.direction;

                RaycastHit hit;
                if(Physics.Raycast(ray, out hit)) {
                    if (hit.collider.gameObject.tag == "Earth") {
                        Instantiate(test, hit.point, Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}
