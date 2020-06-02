using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EarthUtility
{
    public static RaycastHit? CheckRayAgainstEarth(Vector3 origin, Vector3 line) {
        //Need to reverse the ray direction because collisions don't work from the inside of a collider
        //So take a point some distance along the line as origin, then reverse the direction
        var ray = new Ray(origin, line * 200.0f);
        ray.origin = ray.GetPoint(200.0f);
        ray.direction = -ray.direction;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.gameObject.tag == "Earth") {
                return hit;
            }
        }
        else {
            Debug.Log("Raycast missed Earth");
            Debug.DrawRay(ray.origin, ray.direction * 200.0f, Color.yellow, 1000.0f);
        }

        return null;
    }
}
