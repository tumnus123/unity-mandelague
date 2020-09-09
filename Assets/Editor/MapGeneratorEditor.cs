using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        MapGenerator mapGen = (MapGenerator)target;
        float lastNoiseScale = mapGen.noiseScale;
        float lastFractalScale = mapGen.fractalScale;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                if (mapGen.linkScaling)
                {
                    if (mapGen.noiseScale != lastNoiseScale) // update fractalScale
                    {
                        mapGen.fractalScale = mapGen.fractalScale * (mapGen.noiseScale / lastNoiseScale);
                    } else if (mapGen.fractalScale != lastFractalScale) // update noiseScale
                    {
                        mapGen.noiseScale = mapGen.noiseScale * (mapGen.fractalScale / lastFractalScale);
                    }
                }
                mapGen.GenerateMap();
                lastNoiseScale = mapGen.noiseScale;
                lastFractalScale = mapGen.fractalScale;
            }
        }

        if (GUILayout.Button("Generate my Map"))
        {
            mapGen.GenerateMap();
        }
    }
}
