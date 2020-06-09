using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Utility;

namespace CoordinateMapper.Coordinates {
    [Serializable] public abstract class CoordinatePoint {
        public GameObject pointPrefab;
        public Location location;

        [SerializeField] private float placingAdjustment = 0f; //Used to adjust point inward / outward from surface

        public virtual GameObject Plot(Transform planet, Transform container) {
            return PlacePoint(planet, container);
        }

        protected GameObject PlacePoint(Transform planet, Transform container) {
            var point = PlanetUtility.VectorFromLatLng(location.latitude, location.longitude, Vector3.right);

            var hitInfo = PlanetUtility.LineFromOriginToSurface(planet, point);
            if (hitInfo.HasValue) {
                var go = UnityEngine.Object.Instantiate(pointPrefab, hitInfo.Value.point + point * placingAdjustment, Quaternion.identity, container);
                go.name = location.name;
                return go;
            }

            return null;
        }
    }

    [Serializable] public class CoordinatePoint_Basic : CoordinatePoint {
        public override GameObject Plot(Transform planet, Transform container) {
            var go = PlacePoint(planet, container);
            if (go != null) {
                var point = (go.transform.position - planet.transform.position).normalized;
                go.transform.rotation = Quaternion.LookRotation(-point);
            }

            return go;
        }
    }

    [Serializable] public class CoordinatePoint_Magnitude : CoordinatePoint {
        public float magnitude;

        public override GameObject Plot(Transform planet, Transform container) {
            var go = PlacePoint(planet, container);
            if (go != null) {
                var point = (go.transform.position - planet.transform.position).normalized;
                go.transform.localScale = new Vector3(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z * magnitude);
                go.transform.rotation = Quaternion.LookRotation(point);
                //go.transform.Rotate(180f, 0f, 0f, Space.Self);
            }

            return go;
        }
    }
}
