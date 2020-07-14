using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using CoordinateMapper;

public class Jsontester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var json = (TextAsset)Resources.Load("json_test_latlng_keys", typeof(TextAsset));

        var parsed = JsonParser.Parse(json.text, new string[2] { "lat", "lng" });

        /*var blah = parsed["latArr"];
        var fs = blah.Cast<object[]>().ToArray();
        Debug.Log("fs: " + fs);
        foreach(object[] fo in fs) {
            var fa = fo.Cast<float>().ToArray();
            foreach(float f in fa) {
                Debug.Log("F: " + f);
            }
        }*/


        var lats = parsed["lat"].Cast<float>().ToArray();
        var lngs = parsed["lng"].Cast<float>().ToArray();

        for(int i = 0; i < lats.Length; i++) {
            var lat = lats[i];
            var lng = lngs[i];

            var loc = new Location(lat, lng);
            Debug.Log(loc);
        }
    }
}
