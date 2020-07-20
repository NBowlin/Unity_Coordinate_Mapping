using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateMapper {
    public static class PlanetUtility {
        public static Vector3 VectorFromLatLng(float latitude, float longitude, Vector3 direction) {
            return Quaternion.Euler(0.0f, -longitude, latitude) * direction;
        }

        //TODO: Better name
        public static RaycastHit? LineFromOriginToSurface(Transform planet, Vector3 line, LayerMask mask) {
            //Need to reverse the ray direction because collisions don't work from the inside of a collider
            //So take a point some distance along the line as origin, then reverse the direction
            //Also, planet can't be larger than 200 units
            var ray = new Ray(planet.position, line * 200.0f);
            ray.origin = ray.GetPoint(200.0f);
            ray.direction = -ray.direction;

            RaycastHit hit;
            
            //Physics.Raycast(ray, hit, Dist, mask)
            if (Physics.Raycast(ray, out hit, 200.0f, mask)) {
                //TODO: Since we have layermask we don't need collider check anymore?
                //if (hit.collider.gameObject == planet.gameObject) {
                    return hit;
                //}
            }
            else {
                Debug.Log("Raycast missed Planet");
                Debug.DrawRay(ray.origin, ray.direction * 200.0f, Color.yellow, 1000.0f);
            }

            return null;
        }

        public static RaycastHit? LineToSurface(Transform planet, Transform orbiter, float maxDist, LayerMask mask) {
            RaycastHit hit;
            var planetDir = (planet.position - orbiter.position).normalized;

            if (Physics.Raycast(orbiter.position, planetDir, out hit, maxDist, mask)) {
                //if (hit.collider.gameObject == planet.gameObject) {
                    return hit;
                //}
            }

            return null;
        }

        public static GameObject PlacePoint(Transform planet, Transform container, Location location, GameObject prefab) {
            var placingAdjustment = 0.0f;
            var point = PlanetUtility.VectorFromLatLng(location.latitude, location.longitude, Vector3.right);

            var hitInfo = PlanetUtility.LineFromOriginToSurface(planet, point, LayerMask.GetMask("Planet"));
            if (hitInfo.HasValue) {
                var go = UnityEngine.Object.Instantiate(prefab, hitInfo.Value.point + point * placingAdjustment, Quaternion.identity, container);
                go.name = location.name;
                return go;
            }

            return null;
        }
    }
}
