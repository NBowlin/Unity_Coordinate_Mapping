using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CoordinateMapper {
    public static class LookupTable {
        private const float pi2 = Mathf.PI * 2;

        private const int subdivision = 1000000;
        private const int subdivisionMinusOne = subdivision - 1;
        public static float[] sinTable = new float[subdivision];
        public static float[] cosTable = new float[subdivision];

        static LookupTable() {
            double delta = Math.PI * 2 / subdivision;
            double v = 0.0;
            for (int i = 0; i < subdivision; i++) {
                sinTable[i] = (float)Math.Sin(v);
                cosTable[i] = (float)Math.Cos(v);
                v += delta;
            }
        }

        public static int Rad2Index(float radians) {
            while (radians < 0) { radians += pi2; }
            return (int)((radians / pi2) * subdivisionMinusOne) % subdivision;
        }

        public static float Sin(float radians) {
            while (radians < 0) { radians += pi2; }
            return sinTable[Rad2Index(radians)];
        }

        public static float Cos(float radians) {
            return cosTable[Rad2Index(radians)];
        }
    }
}