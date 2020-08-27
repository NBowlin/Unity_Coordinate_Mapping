using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CoordinateMapper {
    public interface IDataLoader {
        TextAsset dataFile { get; set; }
        DataLoadedEvent loadComplete { get; set; }

        void ParseFile(string fileText);
    }

    [System.Serializable] public class DataLoadedEvent : UnityEvent<IEnumerable<ICoordinatePoint>> { }
}
