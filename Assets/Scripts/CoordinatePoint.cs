using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class CoordinatePoint
{
    public GameObject pointPrefab;
    public Location location;

    [SerializeField] private float placingAdjustment = 0f; //Used to adjust point inward / outward from surface

    public virtual GameObject Plot(Transform planet, Transform container) {
        return PlacePoint(planet, container);
    }

    private GameObject PlacePoint(Transform planet, Transform container) {
        var point = Quaternion.Euler(0.0f, -location.longitude, location.latitude) * Vector3.right;

        var hitInfo = PlanetUtility.LineFromOriginToSurface(planet, point);
        if (hitInfo.HasValue) {
            var go = UnityEngine.Object.Instantiate(pointPrefab, hitInfo.Value.point + point * placingAdjustment, Quaternion.LookRotation(-point), container);
            go.name = location.name;
            return go;
        }

        return null;
    }
}

[Serializable] public class CoordinatePoint_Magnitude: CoordinatePoint {
    public float magnitude;

    public override GameObject Plot(Transform planet, Transform container) {
        var go = base.Plot(planet, container);
        if(go != null) { go.transform.localScale = new Vector3(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z * magnitude); }

        return go;
    }
}
