using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class UPTerrain : MonoBehaviour
{
<<<<<<< HEAD
    
    // Range for map generation based on Unity's Random
    [SerializeField] private Vector2 unityRandomRange = new Vector2(0, 0.1f);
    
    // Data for manipulating our terrain
    [SerializeField] private Terrain terrain;
    [SerializeField] private TerrainData terrainData;

    // Height map parameters, the texture and scale factor
    [SerializeField] private Texture2D heightMapTexture;
    // Scale factor allows for stretching the heightmap to match our terrain
    [SerializeField] private Vector3 heightMapScale = Vector3.one;
    
    
    // Generate a height map using Unity's Random.Range function
    public void UnityRandomHeightMap()
    {
        Debug.Log("Generating Random Heights");

        // Create a new heightmap with dimension from our terrain data's heightmap resolution (will always be square i.e 1024x1024)
=======
    [SerializeField] private Vector2 randomHeightRange = new Vector2(0, 0.1f);
    [SerializeField] private Terrain terrain;
    [SerializeField] private TerrainData terrainData;
    
    public void RandomTerrain()
    {
        Debug.Log("Generating Random Heights");

>>>>>>> UPTT-2-Develop
        var heightMap = terrainData.GetHeights(0, 0,
            terrainData.heightmapResolution,
            terrainData.heightmapResolution);

<<<<<<< HEAD
        // Iterate through all positions in the heightmap and give them a value between our unityRandomRange.x and .y
=======
>>>>>>> UPTT-2-Develop
        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
            {
<<<<<<< HEAD
                heightMap[x, y] += UnityEngine.Random.Range(unityRandomRange.x, unityRandomRange.y);
            }
        }
        
        // After generating the heightmap, apply it to the terrain. This will update instantly in the editor.
        terrainData.SetHeights(0,0, heightMap);
    }
    
    // Load a heightmap from our heightMapTexture
    public void LoadHeightMapFromTexture()
    {
        var heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // Iterate 
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                // assign our heightmap x & y adjusted by our scale
                var heightMapX = (int) (x * heightMapScale.x);
                var heightMapY = (int) (y * heightMapScale.y);
                
                /*
                 * Grab the pixel data from the texture and scale it
                 * Only care about the grayscale value for our maps
                 */
                heightMap[x, y] = heightMapTexture.GetPixel(heightMapX, heightMapY).grayscale * heightMapScale.z;
            }
        }
        
        // Currently overriding the current heightmap with the loaded one. TODO: Implement blend mode (i.e additive, lerp, invert)
        terrainData.SetHeights(0, 0, heightMap);
    }
    
    // Reset terrain to a zeroed out heightmap
    public void ResetTerrainHeightMap()
=======
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0,0, heightMap);
    }

    public void ResetTerrain()
>>>>>>> UPTT-2-Develop
    {
        Debug.Log("Resetting Terrain");

        var heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0,0, heightMap);
    }
    
<<<<<<< HEAD
    
    /**
     * Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time.
     * This function is only called in editor mode. Reset is most commonly used to give good default values in the Inspector.
     * DOCS: https://docs.unity3d.com/ScriptReference/MonoBehaviour.Reset.html
     */
    private void Reset()
    {
        Debug.Log("Initialising Terrain Data");
        // Set the terrain to the Terrain component on this object and assign our terrain data
=======
    private void Reset()
    {
        Debug.Log("Initialising Terrain Data");
>>>>>>> UPTT-2-Develop
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

    private static void AddTag(SerializedProperty tagsProperty, string tag)
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
