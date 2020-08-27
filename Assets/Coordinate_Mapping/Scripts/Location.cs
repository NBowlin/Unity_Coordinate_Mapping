using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoordinateMapper {
    [Serializable] public class Location {
        public string name;

        public float latitude;
        public float longitude;

        public Location(string place, float lat, float lng) {
            name = place;
            latitude = lat;
            longitude = lng;
        }

        public Location(float lat, float lng) {
            latitude = lat;
            longitude = lng;
        }

        public override string ToString() {
            return "Location: Name: " + name + " Coords: " + latitude + "/" + longitude;
        }

        public float kmBetweenLocations(Location loc, float radius) {
            return kmBetweenLocations(latitude, longitude, loc.latitude, loc.longitude, radius);
        }

        public float kmBetweenLocations(float lat2, float lon2, float radius) {
            return kmBetweenLocations(latitude, longitude, lat2, lon2, radius);
        }

        public static float kmBetweenLocations(float lat1, float lon1, float lat2, float lon2, float radius) {
            //radius is in meters (Earth's radius is 6371000)
            //φ is latitude, λ is longitude, R is earth’s radius(mean radius = 6,371km);
            //note that angles need to be in radians to pass to trig functions!

            //Haversine Formula
            /*float R = 6371000f; //meters
            float φ1 = lat1 * Mathf.Deg2Rad; // φ, λ in radians
            float φ2 = lat2 * Mathf.Deg2Rad;
            float Δφ = (lat2 - lat1) * Mathf.Deg2Rad;
            float Δλ = (lon2 - lon1) * Mathf.Deg2Rad;

            float a = Mathf.Sin(Δφ / 2) * Mathf.Sin(Δφ / 2) +
                      Mathf.Cos(φ1) * Mathf.Cos(φ2) *
                      Mathf.Sin(Δλ / 2) * Mathf.Sin(Δλ / 2);
            float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

            float d = (R * c) / 1000f; //kilometers
            return d;*/

            //Spherical Law of Cosines - in my testing about 30% faster than the Haversine formula
            //float R = 6371000f; //meters
            /*float φ1 = lat1 * Mathf.Deg2Rad;
            float φ2 = lat2 * Mathf.Deg2Rad;
            float Δλ = (lon2 - lon1) * Mathf.Deg2Rad;
            float d = (Mathf.Acos(Mathf.Sin(φ1) * Mathf.Sin(φ2) + Mathf.Cos(φ1) * Mathf.Cos(φ2) * Mathf.Cos(Δλ)) * radius) / 1000f;*/

            float[] sinTable = LookupTable.sinTable;
            float[] cosTable = LookupTable.cosTable;
            int φ1 = LookupTable.Rad2Index(lat1 * Mathf.Deg2Rad);
            int φ2 = LookupTable.Rad2Index(lat2 * Mathf.Deg2Rad);
            int Δλ = LookupTable.Rad2Index((lon2 - lon1) * Mathf.Deg2Rad);
            float d = (Mathf.Acos(sinTable[φ1] * sinTable[φ2] + cosTable[φ1] * cosTable[φ2] * cosTable[Δλ]) * radius) / 1000f;
            return d;

            //Equirectangular approximation - VERY inaccurate, not sure if I have something off in the formula...
            /*float x = Δλ * Mathf.Cos((φ1 + φ2) / 2f);
            float y = (φ2 - φ1);
            float d = (Mathf.Sqrt(x * x + y * y) * R) / 1000f;*/
        }
    }
}