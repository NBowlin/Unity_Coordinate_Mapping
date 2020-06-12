using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordinateMapper.Data;
using CoordinateMapper.Coordinates;
public class DrawOnTexture : MonoBehaviour
{

    public DataLoader_Basic data;
    float lat = 40.7128f;
    float lng = -74.006f;
    //public Texture2D overlay;
    // Start is called before the first frame update
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        Material mat = rend.material;
        Texture main = mat.mainTexture;
        int w = main.width;
        int h = main.height;
        Texture2D overlay = new Texture2D(w, h);

        /*Color[] colors = new Color[w * h];
        int area = w * h;

        for (int i = 0; i < area; i++) {
            colors[i] = i < w ? Color.green : Color.blue;
        }

        overlay.SetPixels(0, 0, w, h, colors);*/

        /*float texLat = 90f + lat;
        float texLng = 180f + lng;

        float latRatio = texLat / 180f;
        float lngRatio = texLng / 360f;

        float latRatioMax = latRatio + 0.001f;
        float lngRatioMax = lngRatio + 0.001f;*/

        //TODO: Rework - this is just for testing lat / lng to x / y
        for (int x = 0; x < w; x++) {
            for(int y = 0; y < h; y++) {
                Color pixColor = Color.clear;

                foreach(CoordinatePoint p in data.test) {
                    float texLat = 90f + p.location.latitude;
                    float texLng = 180f + p.location.longitude;

                    float latRatio = texLat / 180f;
                    float lngRatio = texLng / 360f;

                    float latRatioMax = latRatio + 0.001f;
                    float lngRatioMax = lngRatio + 0.001f;

                    if ((float)x / w >= lngRatio && (float)y / h >= latRatio) {
                        if ((float)x / w <= lngRatioMax && (float)y / h <= latRatioMax) {
                            pixColor = Color.red;
                        }
                    }
                }

                overlay.SetPixel(x, y, pixColor);
            }
        }
        overlay.Apply();

        mat.SetTexture("_OverlayTex", overlay);
        /*Texture2D tex = Instantiate(rend.material.mainTexture) as Texture2D;
        Debug.Log("Texture: " + tex + " | W: " + tex.width + " H: " + tex.height + " | Color: " + tex.GetPixel(500, 500));
        rend.material.mainTexture = tex;

        var colors = new Color[25];
        for(int i = 0; i < 25; i++) {
            colors[i] = Color.clear;
        }

        for (int y = 0; y < tex.height; y+=10) {
            for (int x = 0; x < tex.width; x+=10) {
                tex.SetPixels(x, y, 5, 5, colors);
            }
        }
        
        tex.Apply();*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
