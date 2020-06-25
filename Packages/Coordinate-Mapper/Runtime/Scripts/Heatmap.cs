using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CoordinateMapper.Coordinates;

public class Heatmap : MonoBehaviour
{
    [SerializeField] private bool drawGrid;

    [SerializeField] private Gradient colors;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void DoStuff(IEnumerable<CoordinatePoint> points) {
        /*var p = new CoordinatePoint_Basic();
        var l = new Location();
        l.latitude = 40.7128f;
        l.longitude = -74.0060f;
        p.location = l;
        points = new List<CoordinatePoint>() { p };*/

        GenerateHeatMapGrid(points);
    }

    void GenerateHeatMapGrid(IEnumerable<CoordinatePoint> points) {
        Material mat = GetComponent<Renderer>().material;
        Texture2D overlay = new Texture2D(mat.mainTexture.width, mat.mainTexture.height); //mat.GetTexture("_OverlayTex") as Texture2D;
        int w = 400;
        int h = 200;

        int[,] heatmapGrid = new int[w, h];
        DrawHeatMapGrid(heatmapGrid);

        foreach (CoordinatePoint p in points) {
            float texLat = 90f + p.location.latitude;
            float texLng = 180f + p.location.longitude;

            float latRatio = texLat / 180f;
            float lngRatio = texLng / 360f;

            float xStart = Mathf.Round(lngRatio * w);
            float yStart = Mathf.Round(latRatio * h);

            xStart = Mathf.Clamp(xStart, 0f, w - 1);
            yStart = Mathf.Clamp(yStart, 0f, h - 1);

            heatmapGrid[(int)xStart, (int)yStart] += 50;
            int range = 4;

            //Square Pattern
            /*for (int x = (int)xStart - range; x <= xStart + range; x++) {
                if (x < 0 || x >= w) { continue; }
                for (int y = (int)yStart - range; y <= yStart + range; y++) {
                    if (y < 0 || y >= h) { continue; }
                    heatmapGrid[x, y] += 20;
                }
            }*/


            //Diamond Pattern
            for(int x = 0; x < range; x++) {
                if (x + xStart >= w) { continue; }
                for (int y = 0; y < range - x; y++) {
                    if (y + yStart >= h) { continue; }
                    heatmapGrid[(int)xStart + x, (int)yStart + y] += 20;

                    if(x != 0 && xStart - x > 0) { heatmapGrid[(int)xStart - x, (int)yStart + y] += 20; }
                    if(y != 0 && yStart - y > 0) {
                        heatmapGrid[(int)xStart + x, (int)yStart - y] += 20;
                        if (x != 0 && xStart - x > 0) { heatmapGrid[(int)xStart - x, (int)yStart - y] += 20; }
                    }
                }
            }

            /*Color[] c = new Color[9];
            for (int i = 0; i < c.Length; i++) { c[i] = new Color(1f, 0f, 0f, 0.3f); }
            overlay.SetPixels((int)xStart, (int)yStart, 3, 3, c);
            Debug.Log("X: " + (int)xStart + "/" + xStart + " Y: " + (int)yStart + "/" + yStart);*/
        }

        for (int x = 0; x < heatmapGrid.GetLength(0); x++) {
            string line = "";
            for (int y = 0; y < heatmapGrid.GetLength(1); y++) {
                line = line + heatmapGrid[x, y] + ", ";
            }
            Debug.Log(line);
        }

        DrawHeatmapTexture(heatmapGrid);
    }

    void DrawHeatMapGrid(int[,] heatmapGrid) {
        if(!drawGrid) { return; }

        for(int x = 0; x < heatmapGrid.GetLength(0); x++) {
            for(int y = 0; y < heatmapGrid.GetLength(1); y++) {
                float ratioX = (float)x / (float)heatmapGrid.GetLength(0);
                float endRatioX = (float)(x + 1) / (float)heatmapGrid.GetLength(0);

                float ratioY = (float)y / (float)heatmapGrid.GetLength(1);
                float endRatioY = (float)(y + 1) / (float)heatmapGrid.GetLength(1);

                Vector2 botLeft = new Vector2(transform.position.x - transform.localScale.x / 2f, transform.position.z - transform.localScale.y / 2f);

                Debug.DrawLine(new Vector3(botLeft.x + ratioX * transform.localScale.x, transform.position.y, botLeft.y + ratioY * transform.localScale.y), 
                    new Vector3(botLeft.x + endRatioX * transform.localScale.x, transform.position.y, botLeft.y + ratioY * transform.localScale.y), Color.green, 100f);

                Debug.DrawLine(new Vector3(botLeft.x + ratioX * transform.localScale.x, transform.position.y, botLeft.y + ratioY * transform.localScale.y),
                    new Vector3(botLeft.x + ratioX * transform.localScale.x, transform.position.y, botLeft.y + endRatioY * transform.localScale.y), Color.green, 100f);
            }
        }
    }

    void DrawHeatmapTexture(int[,] heatmap) {
        Material mat = GetComponent<Renderer>().material;
        Texture2D overlay = new Texture2D(heatmap.GetLength(0), heatmap.GetLength(1)); //mat.GetTexture("_OverlayTex") as Texture2D;
        int w = heatmap.GetLength(0);
        int h = heatmap.GetLength(1);

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                overlay.SetPixel(x, y, Color.clear);
            }
        }

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                if (heatmap[x, y] > 0) {
                    //Color using gradient
                    /*var c = colors.Evaluate(heatmap[x, y] / 100f);
                    overlay.SetPixel(x, y, c);*/

                    //Red color using alpha
                    overlay.SetPixel(x, y, new Color(1f, 0f, 0f, heatmap[x, y] / 100f));
                }
            }
        }

        overlay.Apply();
        mat.SetTexture("_OverlayTex", overlay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

//public class DrawOnTexture : MonoBehaviour {

//    public DataLoader_Basic data;
//    float lat = 40.7128f;
//    float lng = -74.006f;
//    //public Texture2D overlay;
//    // Start is called before the first frame update
//    void Start() {
//        Invoke("TestTex", 0.1f);


//        /*longitude = atan2((float)-x, z);
//        latitude = atan(y / sqrt(x * x + z * z));

//        longitude = 1.0 - longitude / (2 * PI);
//        latitude = fabs(0.5 - latitude / PI);

//        u = longitude - floor(longitude);
//        v = latitude;*/

//        /*latitude = 41.145556; // (φ)
//        longitude = -73.995;   // (λ)

//        mapWidth = 200;
//        mapHeight = 100;

//        // get x value
//        x = (longitude + 180) * (mapWidth / 360)

//// convert from degrees to radians
//        latRad = latitude * PI / 180;

//        // get y value
//        mercN = ln(tan((PI / 4) + (latRad / 2)));
//        y = (mapHeight / 2) - (mapWidth * mercN / (2 * PI));*/

//    }

//    void TestTex() {
//        var blah = GetComponent<MeshFilter>();
//        var m = blah.mesh;

//        //(0.3738, 0.3182)
//        //Debug.Log("UV0: " + m.uv[0].ToString("F4"));


//        Renderer rend = GetComponent<Renderer>();
//        Material mat = rend.material;
//        Texture main = mat.mainTexture;
//        int w = main.width;
//        int h = main.height;
//        Texture2D overlay = new Texture2D(w, h);


//        /*Color[] colors = new Color[w * h];
//        int area = w * h;

//        for (int i = 0; i < area; i++) {
//            colors[i] = i < w ? Color.green : Color.blue;
//        }

//        overlay.SetPixels(0, 0, w, h, colors);*/

//        /*float texLat = 90f + lat;
//        float texLng = 180f + lng;

//        float latRatio = texLat / 180f;
//        float lngRatio = texLng / 360f;

//        float latRatioMax = latRatio + 0.001f;
//        float lngRatioMax = lngRatio + 0.001f;*/

//        //Debug.Log("Using Test: " + data.test);

//        //TODO: Rework - this is just for testing lat / lng to x / y
//        /*int xBegin = 0;
//        int xDone = 0;
//        int yBegin = 0;
//        int yDone = 0;
//        for (int x = 0; x < w; x++) {
//            for (int y = 0; y < h; y++) {
//                Color pixColor = Color.clear;

//                foreach (CoordinatePoint p in data.test) {
//                    float texLat = 90f + p.location.latitude;
//                    float texLng = 180f + p.location.longitude;

//                    float latRatio = texLat / 180f;
//                    float lngRatio = texLng / 360f;

//                    float latRatioMax = latRatio + 0.001f;
//                    float lngRatioMax = lngRatio + 0.001f;

//                    if ((float)x / w >= lngRatio && (float)y / h >= latRatio) {
//                        if(xBegin == 0) { xBegin = x; }
//                        if ((float)x / w <= lngRatioMax && (float)y / h <= latRatioMax) {
//                            if (yBegin == 0) {
//                                Debug.Log("B&D ratio: " + latRatio + "/" + lngRatio);
//                                yBegin = y;
//                            }
//                            pixColor = Color.green;
//                        }
//                        else {
//                            if (yBegin != 0 && yDone == 0) { yDone = y; }
//                        }
//                    } else {
//                        if (xBegin != 0 && xDone == 0) { xDone = x; }
//                    }
//                }

//                overlay.SetPixel(x, y, pixColor);
//            }
//        }

//        Debug.Log("B&D X: " + xBegin + "/" + xDone + " Y: " + yBegin + "/" + yDone);*/

//        for (int x = 0; x < w; x++) {
//            for (int y = 0; y < h; y++) {
//                overlay.SetPixel(x, y, Color.clear);
//            }
//        }

//        foreach (CoordinatePoint p in data.test) {

//            /*var xStart = (p.location.longitude + 180f) * (w / 360f);
//            var latRad = p.location.latitude * (Mathf.PI / 180f);
//            var mercN = Mathf.Log(Mathf.Tan((Mathf.PI / 4) + (latRad / 2)));
//            var yStart = (h / 2) - (w * mercN / (2 * Mathf.PI));
//            yStart = h - yStart;

//            Debug.Log("X: " + (int)xStart + "/" + xStart + " Y: " + (int)yStart + "/" + yStart);

//            Color[] c = new Color[30*30];
//            for (int i = 0; i < c.Length; i++) { c[i] = new Color(1f, 0f, 0f, 0.3f); }
//            overlay.SetPixels((int)xStart, (int)yStart, 30, 30, c);*/

//            float texLat = 90f + p.location.latitude;
//            float texLng = 180f + p.location.longitude;

//            float latRatio = texLat / 180f;
//            float lngRatio = texLng / 360f;

//            float xStart = Mathf.Round(lngRatio * w) - 1;
//            float yStart = Mathf.Round(latRatio * h) - 1;

//            Color[] c = new Color[9];
//            for (int i = 0; i < c.Length; i++) { c[i] = new Color(1f, 0f, 0f, 0.3f); }
//            overlay.SetPixels((int)xStart, (int)yStart, 3, 3, c);
//            Debug.Log("X: " + (int)xStart + "/" + xStart + " Y: " + (int)yStart + "/" + yStart);

//            /*overlay.SetPixel((int)latRatio, (int)lngRatio, Color.red);

//            float xStart = (lngRatio * w);
//            float yStart = (latRatio * h);

//            Debug.Log("X: " + (int)xStart + "/" + xStart + " Y: " + (int)yStart + "/" + yStart);*/

//            /*float latRatioMax = latRatio + 0.0008f;
//            float lngRatioMax = lngRatio + 0.0008f;

//            int xStart = (int)(lngRatio * w);
//            int yStart = (int)(latRatio * h);
//            int xEnd = (int)(lngRatioMax * w);
//            int yEnd = (int)(latRatioMax * h);

//            Debug.Log("X: " + xStart + "/" + xEnd + " Y: " + yStart + "/" + yEnd);

//            for(int x = xStart; x < xEnd; x++) {
//                for (int y = yStart; y < yEnd; y++) {
//                    overlay.SetPixel((int)x, (int)y, Color.red);
//                }
//            }*/
//        }
//        overlay.Apply();

//        mat.SetTexture("_OverlayTex", overlay);
//        /*Texture2D tex = Instantiate(rend.material.mainTexture) as Texture2D;
//        Debug.Log("Texture: " + tex + " | W: " + tex.width + " H: " + tex.height + " | Color: " + tex.GetPixel(500, 500));
//        rend.material.mainTexture = tex;

//        var colors = new Color[25];
//        for(int i = 0; i < 25; i++) {
//            colors[i] = Color.clear;
//        }

//        for (int y = 0; y < tex.height; y+=10) {
//            for (int x = 0; x < tex.width; x+=10) {
//                tex.SetPixels(x, y, 5, 5, colors);
//            }
//        }
        
//        tex.Apply();*/
//    }
//}

