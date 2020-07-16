using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoordinateMapper {
    public class CreatePlanet {

        [MenuItem("Coordinate Mapper/Create Planet/Lit Planet", false, 0)]
        [MenuItem("GameObject/Planet/Lit Planet", false, 0)]
        static void SpawnLit() {
            SpawnPlanet("Earth_Lit");
        }

        [MenuItem("Coordinate Mapper/Create Planet/Unlit Planet", false, 0)]
        [MenuItem("GameObject/Planet/Unlit Planet", false, 0)]
        static void SpawnUnlit() {
            SpawnPlanet("Earth_Unlit");
        }

        [MenuItem("Coordinate Mapper/Create Planet/Lit Overlay Planet", false, 0)]
        [MenuItem("GameObject/Planet/Lit Overlay Planet", false, 0)]
        static void SpawnLitOverlay() {
            SpawnPlanet("Earth_Lit_Overlay");
        }

        [MenuItem("Coordinate Mapper/Create Planet/Unlit Overlay Planet", false, 0)]
        [MenuItem("GameObject/Planet/Unlit Overlay Planet", false, 0)]
        static void SpawnUnlitOverlay() {
            SpawnPlanet("Earth_Unlit_Overlay");
        }

        static void SpawnPlanet(string planetName) {
            var guids = AssetDatabase.FindAssets(planetName);
            foreach (string guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);

                if (assetType == typeof(GameObject)) {
                    Object planet = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                    if (planet != null) {
                        PrefabUtility.InstantiatePrefab(planet);
                        break;
                    }
                }
            }
        }
    }
}
