using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UPTT.Tool;
using UPTT.Tool.Utils;

[ExecuteInEditMode]
public class UPTerrain : MonoBehaviour
{

    #region variables
    // Range for map generation based on Unity's Random
    [SerializeField] private Vector2 unityRandomRange = new Vector2(0, 0.1f);
    
    // Data for manipulating our terrain
    [SerializeField] private Terrain terrain;
    [SerializeField] private TerrainData terrainData;
    [SerializeField] private bool replaceHeightmap = false;
    
    // Height map parameters, the texture and scale factor
    [SerializeField] private Texture2D heightMapTexture;
    // Scale factor allows for stretching the heightmap to match our terrain
    [SerializeField] private Vector3 heightMapScale = Vector3.one;
    
    // Perlin noise parameters
    [SerializeField] private int seed;
    [SerializeField] private Vector2 perlinScaleFactor = new Vector2(0.01f,0.01f);
    [SerializeField] private float perlinHeightScale = 0.07f;
    [SerializeField] private int octaves = 4;
    [SerializeField] private float persistence = 0.5f;
    #endregion

    [SerializeField] private List<PerlinNoiseSettings> perlinNoiseSettingsList
        = new List<PerlinNoiseSettings>() {new PerlinNoiseSettings()};

    // Generate a height map using Unity's Random.Range function
    public void UnityRandomHeightMap()
    {
        Debug.Log("Generating Random Heights");

        // Create a new heightmap with dimension from our terrain data's heightmap resolution (will always be square i.e 1024x1024)
        var heightMap = Heightmap();

        // Iterate through all positions in the heightmap and give them a value between our unityRandomRange.x and .y
        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(unityRandomRange.x, unityRandomRange.y);
            }
        }
        
        // After generating the heightmap, apply it to the terrain. This will update instantly in the editor.
        terrainData.SetHeights(0,0, heightMap);
    }

    public void PerlinHeightmap()
    {
        // Create a new heightmap with dimension from our terrain data's heightmap resolution (will always be square i.e 1024x1024)
        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        // Iterate through all positions in the heightmap and give them a value between our unityRandomRange.x and .y
        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
            {
                if (replaceHeightmap)
                {
                    // set the height for the current x,y co-ordinate using fractal Brownian Motion
                    heightMap[x, y] =
                        UPTT.Tool.Utils.TerrainAlgorithms.FractalBrownianMotion(
                            (x + seed)   * perlinScaleFactor.x, 
                              (y + seed) * perlinScaleFactor.y,
                              octaves,
                              persistence,
                              2.0f) * perlinHeightScale;
                }
                else
                {
                    /*
                     *  Set the height for the current x,y co-ordinate usingfractal Brownian Motion
                     *  As Perlin noise is essentially infinite, "seed" is used to change start point
                     */
                    heightMap[x, y] +=
                        UPTT.Tool.Utils.TerrainAlgorithms.FractalBrownianMotion(
                            (x + seed) * perlinScaleFactor.x,
                            (y + seed) * perlinScaleFactor.y,
                            octaves,
                            persistence, 
                            2.0f) * perlinHeightScale;
                }
            }
        }
        
        // After generating the heightmap, apply it to the terrain. This will update instantly in the editor.
        terrainData.SetHeights(0,0, heightMap);
    }


    public void VoronoiTessellation()
    {
        Debug.Log("VoronoiTessellation.");
    }
    
    
    #region PerlinSuperposition
    // Combine all our given perlin noise
    public void PerlinSuperposition()
    {
        var heightMap = Heightmap();

        // Iterate through each pair of x,y co-ordinates
        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
            {
                // Iterate through all of the perlin noise settings that have been given
                foreach (var settings in perlinNoiseSettingsList)
                {
                    // add the value to the current x,y
                    heightMap[x, y] += TerrainAlgorithms.FractalBrownianMotion(
                        (x + settings.seed) * settings.scaleX,
                        (y + settings.seed) * settings.scaleY,
                        settings.fBMOctaves,
                        settings.fBMPersistence) * settings.scaleHeight;
                }
            }
        }
        
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void CreateNewSettings()
    {
        perlinNoiseSettingsList.Add(new PerlinNoiseSettings());
    }
    public void DeleteSettings()
    {
        // Iterate through all the settings that have been marked for deletion and remove them
        foreach (var settings in perlinNoiseSettingsList.Reverse<PerlinNoiseSettings>()
            .Where(settings => settings.canDelete))
        {
            perlinNoiseSettingsList.Remove(settings);
        }

        // If no settings remain, add one back
        if(perlinNoiseSettingsList.Count == 0) CreateNewSettings();
    }
    #endregion
    
    #region HeightmapUtils
    private float[,] Heightmap()
    {
        if (!replaceHeightmap)
        {
            return terrainData.GetHeights(0, 0,
                terrainData.heightmapResolution,
                terrainData.heightmapResolution);
        }
        
        return new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
    }
    
    
    // Load a heightmap from our heightMapTexture
    public void LoadHeightMapFromTexture()
    {
        var heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // Iterate 
        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
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
    {
        Debug.Log("Resetting Terrain");

        var heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0,0, heightMap);
    }

    #endregion

    #region Unity

    /**
     * Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time.
     * This function is only called in editor mode. Reset is most commonly used to give good default values in the Inspector.
     * DOCS: https://docs.unity3d.com/ScriptReference/MonoBehaviour.Reset.html
     */
    private void Reset()
    {
        Debug.Log("Initialising Terrain Data");
        // Set the terrain to the Terrain component on this object and assign our terrain data
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
    #endregion

    
}
