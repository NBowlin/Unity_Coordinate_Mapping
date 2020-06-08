using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;

namespace CoordinateMapper.Data {
    public class DataLoader_Magnitude : DataLoader {
        protected override IEnumerable<CoordinatePoint> LoadJson() {
            return JsonDataLoader<CoordinatePoint_Magnitude>.ParseJson(jsonFile.text);
        }
    }
}
