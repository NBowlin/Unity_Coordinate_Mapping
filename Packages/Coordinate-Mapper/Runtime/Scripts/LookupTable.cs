using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LookupTable {
    public static float[,] sinTable = new float[361, 10000];
    public static float[,] cosTable = new float[361, 10000];

    public static void GenerateTables() {
        var onesCount = sinTable.GetLength(0);
        var decCount = sinTable.GetLength(1);

        for (int i = 0; i < onesCount; i++) {
            for (int j = 0; j < decCount; j++) {
                float num = i + j / (float)decCount;
                float radians = num * Mathf.Deg2Rad;

                sinTable[i, j] = Mathf.Sin(radians);
                cosTable[i, j] = Mathf.Cos(radians);
            }
        }
    }

    public static float LookupSinValue(float degrees) {
        var numParts = BreakOutParts(Mathf.Abs(degrees));
        float sinVal = sinTable[numParts.onesPlace, numParts.decimals];

        //sin(x) == -sin(-x)
        return degrees < 0 ? -sinVal : sinVal;
    }

    public static float LookupCosValue(float degrees) {
        var numParts = BreakOutParts(Mathf.Abs(degrees));
        float cosVal = cosTable[numParts.onesPlace, numParts.decimals];

        //cos(x) == cos(-x)
        return cosVal;
    }

    private static (int onesPlace, int decimals) BreakOutParts(float degrees) {
        //degrees = degrees < 0 ? (degrees - degrees * 2) : degrees; //ABS value

        int ones = Mathf.FloorToInt(degrees);
        //int ones = (int)degrees;
        int dec = Mathf.RoundToInt((degrees - ones) * sinTable.GetLength(1));
        if (dec >= sinTable.GetLength(1)) { dec -= 1; } //Adjust for .995+ decimal rounding

        return (ones, dec);
    }
}