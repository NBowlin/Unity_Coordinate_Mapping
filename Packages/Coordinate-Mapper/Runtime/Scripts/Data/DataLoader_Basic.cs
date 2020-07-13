using System.Collections;
using System.Collections.Generic;

namespace CoordinateMapper {
    public class DataLoader_Basic : DataLoader {
        protected override IEnumerable<CoordinatePoint> LoadJson() {
            return JsonDataLoader<CoordinatePoint_Basic>.ParseJson(jsonFile.text, parseStyle, latitudeKey, longitudeKey);
        }
    }
}
