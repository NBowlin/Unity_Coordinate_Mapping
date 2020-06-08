using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoader_Basic : DataLoader {
    protected override IEnumerable<CoordinatePoint> LoadJson() {
        return JsonDataLoader<CoordinatePoint_Basic>.ParseJson(jsonFile.text);
    }
}
