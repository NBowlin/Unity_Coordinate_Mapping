using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateMapper.Coordinates {
    [Serializable] public class Location {
        public string name;

        public float latitude;
        public float longitude;

        public float kmBetweenLocations(Location loc) {
            return kmBetweenLocations(latitude, longitude, loc.latitude, loc.longitude);
        }

        public float kmBetweenLocations(float lat2, float lon2) {
            return kmBetweenLocations(latitude, longitude, lat2, lon2);
        }

        //TODO: This is actually fairly expensive: https://latkin.org/blog/2014/11/09/a-simple-benchmark-of-various-math-operations/
        public static float kmBetweenLocations(float lat1, float lon1, float lat2, float lon2) {
            //Haversine Formula
            float R = 6371000f; //meters
            float φ1 = lat1 * Mathf.Deg2Rad; // φ, λ in radians
            float φ2 = lat2 * Mathf.Deg2Rad;
            float Δφ = (lat2 - lat1) * Mathf.Deg2Rad;
            float Δλ = (lon2 - lon1) * Mathf.Deg2Rad;

            float a = Mathf.Sin(Δφ / 2) * Mathf.Sin(Δφ / 2) +
                      Mathf.Cos(φ1) * Mathf.Cos(φ2) *
                      Mathf.Sin(Δλ / 2) * Mathf.Sin(Δλ / 2);
            float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

            float d = (R * c) / 1000f; //kilometers
            return d;

            //TODO: Compare this Equirectangular approximation formula for speed / accuracy
            /*Formula: 
            x = Δλ ⋅ cos φm
            y = Δφ
            d = R ⋅ √x² +y²
            JavaScript:
            const x = (λ2 - λ1) * Math.cos((φ1 + φ2) / 2);
            const y = (φ2 - φ1);
            const d = Math.sqrt(x * x + y * y) * R;*/
        }
    }
}