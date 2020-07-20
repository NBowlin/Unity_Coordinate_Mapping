using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CoordinateMapper {
    public interface IDataLoader {
        TextAsset dataFile { get; set; }
        JsonLoadedEvent loadComplete { get; set; }

        void ParseFile(string fileText);
    }

    //TODO: Updated this to ICoordinatePoint - Check back after rework
    [System.Serializable] public class JsonLoadedEvent : UnityEvent<IEnumerable<ICoordinatePoint>> { }
}
