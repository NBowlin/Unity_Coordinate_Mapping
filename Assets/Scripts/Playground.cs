using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playground : MonoBehaviour
{
    public GameObject test;

    private int degBetweenLats = 10;
    private int latSegments = 180;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Draw lats!");

        int segmentIncrement = 360 / latSegments;
        for (int i = -90; i <= 90; i += degBetweenLats) {
            for (int j = 0; j < 360; j += segmentIncrement) {
                var line = Quaternion.Euler(0.0f, j, i) * Vector3.right;
                //Debug.DrawRay(Vector3.zero, line * 5.0f, Color.red, 1000.0f);

                var ray = new Ray(Vector3.zero, line * 6.0f);
                ray.origin = ray.GetPoint(6.0f);
                ray.direction = -ray.direction;

                RaycastHit hit;
                if(Physics.Raycast(ray, out hit)) {
                    if (hit.collider.gameObject.tag == "Earth") {
                        Debug.Log("Hit the earth!");
                        Instantiate(test, hit.point, Quaternion.identity, transform);

                    } else {
                        Debug.Log("Didn't hit the earth??");
                    }
                } else {
                    Debug.Log("Didn't hit ANYTHING??");
                }
                

                //Debug.DrawRay(Vector3.zero, line * 5.0f, Color.red, 1000.0f);
            }
        }

        /*var right = Quaternion.identity * Vector3.right;
        var right45 = Quaternion.Euler(0, 45.0f, 0) * Vector3.right;
        var rightNeg10 = Quaternion.Euler(0, -10.0f, 0) * Vector3.right;

        var up10 = Quaternion.Euler(0, 0, 10.0f) * Vector3.right;
        var up10rightNeg10 = Quaternion.Euler(0, -10.0f, 10.0f) * Vector3.right;

        Debug.DrawRay(Vector3.zero, right * 5.0f, Color.red, 1000.0f);
        Debug.DrawRay(Vector3.zero, right45 * 5.0f, Color.red, 1000.0f);
        Debug.DrawRay(Vector3.zero, rightNeg10 * 5.0f, Color.red, 1000.0f);
        Debug.DrawRay(Vector3.zero, up10 * 5.0f, Color.red, 1000.0f);
        Debug.DrawRay(Vector3.zero, up10rightNeg10 * 5.0f, Color.red, 1000.0f);*/


        /*var longitude = Quaternion.AngleAxis(45.0f, Vector3.up) * Vector3.forward;
        var newLng = Quaternion.AngleAxis(45.0f, Vector3.forward) * longitude;
        var new2 = Quaternion.AngleAxis(45.0f, Vector3.back) * Vector3.up;

        Debug.DrawRay(Vector3.zero, Vector3.right * 5.0f, Color.red, 1000.0f);
        //Debug.DrawRay(Vector3.zero, longitude * 5.0f, Color.red, 1000.0f);
        //Debug.DrawRay(Vector3.zero, newLng * 5.0f, Color.red, 1000.0f);
        Debug.DrawRay(Vector3.zero, new2 * 5.0f, Color.red, 1000.0f);*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
