using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class UPTerrain : MonoBehaviour
{
    [SerializeField] private Vector2 randomHeightRange = new Vector2(0, 0.1f);
    [SerializeField] private Terrain terrain;
    [SerializeField] private TerrainData terrainData;
    
    public void RandomTerrain()
    {
        Debug.Log("Generating Random Heights");

        var heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0,0, heightMap);
    }

    public void ResetTerrain()
    {
        Debug.Log("Resetting Terrain");

        var heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0,0, heightMap);
    }
    
    private void Reset()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        
        // Serialise the tag manager to create new tags
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProperty = tagManager.FindProperty("tags");
        
        AddTag(tagsProperty, "UPTerrain");
        AddTag(tagsProperty, "UPCloud");
        AddTag(tagsProperty, "UPWater");
        
        
        // Apply modifications to the tag store
        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "UPTerrain";
    }

    private void AddTag(SerializedProperty tagsProperty, string tag)
    {
        bool isFound = false;
        
        // Check to see if tag is already made


        if (tagsProperty == null)
        {
            Debug.Log("TagsProperty is null");
            return;
        }
        
        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            SerializedProperty checkTag = tagsProperty.GetArrayElementAtIndex(i);
            if (!checkTag.stringValue.Equals(tag)) continue;
            isFound = true;
            break;
        }
        
        // If tag hasn't been found add it
        if (isFound) return;
        Debug.Log("Adding: " + tag);
        tagsProperty.InsertArrayElementAtIndex(0);
        SerializedProperty newTag = tagsProperty.GetArrayElementAtIndex(0);
        newTag.stringValue = tag;
    }

    
}
