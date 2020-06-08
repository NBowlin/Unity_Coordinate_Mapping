using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoader_Magnitude : DataLoader {
    protected override IEnumerable<CoordinatePoint> LoadJson() {
        return JsonDataLoader<CoordinatePoint_Magnitude>.ParseJson(jsonFile.text);
    }
}
