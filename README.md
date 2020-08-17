# Global Coordinate Mapper

The Global Coordinate Mapper is a free, open source project for visualizing data sets across a globe in Unity. It gives you the flexibility to visualize data without having to set up custom models or adhere to specific json schemas, but the power to add any customization you need!

Global Coordinate Mapper will do the spherical UV mapping for you, so no need for custom planet models or meshes.

## Installation

Please download and install Global Coordinate Mapper from the Unity Asset Store: {Add link once available}

## How to Use

**Note**: In order to use any of the Global Coordinate Mapper scripts in code you must include the namespace by adding `using CoordinateMapper` to the top of your script.

### Quick Setup
All you need to get started with visualizing data is a Json or Csv dataset which contains keys for latitude and longitude.

**Add a planet:** 
To add a planet just right click on the scene hierarchy and choose `Planet` or use the `Coordinate Mapper > Create Planet` menu item. There are 4 preset options:
+ **Lit Planet:** Standard planet that respects lighting within the scene
+ **Unlit Planet:** Same as Lit but ignores lighting within the scene
+ **Lit Overlay Planet:** Planet that contains 2 textures, one for the planet texture itself, and another that gets overlaid using the alpha (Used for heatmaps) and respects lighting within the scene
+ **Unlit Overlay Planet:** Same as Lit Overlay, but ignored lighting within the scene

**Note:** In order for the the plotting to work properly, you must add a `Planet` layer to your project, and set the created planet to use that layer. You can add a layer by going to `Edit > Project Settings > Tags and Layers`.

**Add the visualizer script**:
Once you have your planet setup, select it in the scene view and add the `Default Visualizer` script. This script will take a data set, parse it, and plot the points using the default implementations provided with the framework. It supports a number of different data schemas outlined below.

**Properties:**
*Data File:* The file to parse containing the coordinate data
*Point Prefab:* The model that gets plotted on to the planet (There are a couple that come with the framework inside the *Prefabs* folder)
*Key Format:* How the latitude and longitude keys are stored within the data set:
* Json Lat and Lng Keys - json dataset has separate latitude and longitude keys for each location
* Json Single Lat Lng Array - json dataset for each location is an array of alternating latitude and longitude values
* Json Lat Lng Array - json dataset for locations has an array containing values for latitude and a separate array containing keys for longitude
* Csv - Csv dataset, must contain column for latitude and separate column for longitude

*Latitude Key:* The key within the dataset containing the latitude values
*Longitude Key:* The key within the dataset containing the longitude values

The default visualizer also supports an **optional** magnitude key, which will scale the Z value of the point prefab for the location by the magnitude value. (See Earthquake Demo)
*Magnitude Key:* The key within the dataset containing the magnitude values

*Load Complete:* An event that can be used to pass along the plotted points if any other script wants access to them

So now just fill out the appropriate properties and press play! You should see your data mapped to the planet!

//TODO: Creating your own point prefab

#### IDataLoader
Extend this interface on any custom scripts you might need to parse your data sets. This is useful if you need to parse out more values than the default visualizer allows (latitude, longitude and magnitude).
IDataLoader ensures your class has the following:
- *TextAsset dataFile:* The file containing your data, should be json or csv format
- *void ParseFile(string fileText):* Should be used to take in the string representation of the data set and parse out and serialize any values needed
- *DataLoadedEvent loadComplete:* A unity event for passing along the plotted coordinate points. If needed, will most likely be called at the end of the `ParseFile` function

#### ICoordinatePoint
Extend this interface on the actual script that represents what should get plotted on to the planet.
ICoordinatePoint ensures your class has the following:
- *Location location:* See below
- *GameObject pointPrefab:* The model that gets plotted on to the planet
- *GameObject Plot(Transform planet, Transform container, int layer):* Should be used to plot the prefab itself, and then perform an custom manipulation needed on it (such as scaling):
    - *planet:* The transform of the gameobject the point prefab should map to
    - *container:* The transform of the gameobject to add as a parent of the plotted point prefab
    - *layer:* The layer from *Tags and Layers* to use for the plotted point prefab

#### Heatmap
Can be added to the planet gameobject in order to visualize the data as a heatmap. Contains the following properties:
- *M Planet Radius:* This is the planet's **radius** (not diameter) in meters (i.e. - The Earth is 6371000)
- *Km Range:* The range, in kilometers, each points of data should effect on the heatmap
- *Start Value:* The value (between 0 and 100) that is added to the heatmap at the location of the data point
- *End Value:* As we get farther from the location of the data point (up to the *Km Range*), the amount of point effects the location diminishes. *End Value* is how much effect should be added to the heatmap at the limit of *Km Range*. Should be less than *Start Value*.
- *Heatmap Size:* The size of the heatmap texture (in pixels) to use. (Keep in mind, the larger the texture, the more processing intesive the heatmap)
- *Colors:* The color gradient used to draw the heatmap (Gradient location 0 corresponds to heatmap value 0 and so on to gradient location 100).
- *Hm Renderer:* The gameobject containing the Mesh Renderer with the proper *Overlay* material (described above)

## Extras

TODO: Importing Textures: settings for textures

TODO

## Demos

TODO