using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using UPTT.Tool;
using UPTT.Tool.Generator;
using UPTT.Tool.Utils;

[ExecuteInEditMode]
public class UPTerrain : MonoBehaviour
{
    #region Settings
    private const string TerrainLayerPath = "Assets/UPTT/TerrainLayers/";
    private const string TerrainLayerFileExtension = ".tl";

    private static string GetTerrainLayerPath(string tlName)
    {
        return TerrainLayerPath + tlName + TerrainLayerFileExtension;
    }
    #endregion
    
    #region variables
    // Iterations of blur algorithm to run
    [SerializeField] private uint blurIterations = 1;
    
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
    
    
    // Perlin noise parameters List
    [SerializeField] private List<PerlinNoiseSettings> perlinNoiseSettingsList
        = new List<PerlinNoiseSettings>() {new PerlinNoiseSettings()};
    
    // WeightMaps List
    [SerializeField] private List<WeightMap> weightMaps =
        new List<WeightMap>{new WeightMap()};
    
    // Voronoi parameters
    [SerializeField] private VoronoiSettings voronoiSettings = new VoronoiSettings();
    
    // Diamond Square Parameters
    [SerializeField] private DSSettings diamondSquareSettings = new DSSettings();

    // Erosion
    [SerializeField] private Erosion.ErosionMethod erosionMethod;


    private TerrainLayer[] _terrainLayers;
    private float progress = 0;
    #endregion
    
    #region Algorithms

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
                heightMap[x, y] = heightMap[x, y] + Maths.RandomRange(unityRandomRange.x, unityRandomRange.y);
            }
        }
        
        // After generating the heightmap, apply it to the terrain. This will update instantly in the editor.
        terrainData.SetHeights(0,0, heightMap);
    }

    public void PerlinHeightmap()
    {
        // Create a new heightmap with dimension from our terrain data's heightmap resolution (will always be square i.e 1024x1024)
        var heightMap = GetHeightMap();

        // Iterate through all positions in the heightmap and give them a value between our unityRandomRange.x and .y
        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
            {
                switch (replaceHeightmap)
                {
                    case true:
                        // set the height for the current x,y co-ordinate using fractal Brownian Motion
                        heightMap[x, y] =
                            UPTT.Tool.Utils.TerrainAlgorithms.FractalBrownianMotion(
                                (x + seed)   * perlinScaleFactor.x, 
                                (y + seed) * perlinScaleFactor.y,
                                octaves,
                                persistence,
                                2.0f) * perlinHeightScale;
                        break;
                    case false: 
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
                        break;
                }
            }
        }
        
        // After generating the heightmap, apply it to the terrain. This will update instantly in the editor.
        terrainData.SetHeights(0,0, heightMap);
    }
    
    public void VoronoiTessellation()
    {
        Debug.Log("VoronoiTessellation.");

        var hMap = Heightmap();
        var maxVec2 = new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution);
  
        // Calculate greatest length possible with the current heightmap
        var maximumLength = (Vector2.zero - maxVec2).magnitude;
        
        for (var peak = 0; voronoiSettings.numMountains > peak; peak++)
        {
            var mountainPosition = Maths.RandomPositionInTerrain(terrainData.heightmapResolution);
            var peakHeight = Maths.RandomRange(voronoiSettings.lowHeight, voronoiSettings.highHeight);
            
            
            // Only update the height, if its greater than the currently set height
            if (peakHeight > hMap[Convert.ToInt32(mountainPosition.x), Convert.ToInt32(mountainPosition.y)])
            {
                // Set the height in the heightmap
                hMap[Convert.ToInt32(mountainPosition.x), Convert.ToInt32(mountainPosition.y)] = peakHeight;
            }
            else
            {
                continue;
            }

                // Iterate through all positions in the heightmap
            for (var i = 0; terrainData.heightmapResolution > i; i++)
            {
                for (var y = 0; terrainData.heightmapResolution > y; y++)
                {
                    var curPos = new Vector2(i, y);
                    // Don't update if we're at the the position of the mountain peak
                    if(curPos == mountainPosition) continue;

                    // Calculate distance between current position and the peak of the mountain
                    var length = (mountainPosition - curPos).magnitude;
                    var scaled = length / maximumLength;

                    // Calculate height based off voronoi parameters
                    var height = voronoiSettings.falloffType switch
                    {
                        VoronoiSettings.FalloffType.Pow => (peakHeight -
                                                              Mathf.Pow(scaled, voronoiSettings.dropoffAmount) * voronoiSettings.falloffAmount),
                        VoronoiSettings.FalloffType.Combine => (peakHeight - (scaled * voronoiSettings.falloffAmount)) -
                                                               (Mathf.Pow(scaled, voronoiSettings.dropoffAmount) * voronoiSettings.falloffAmount),
                        VoronoiSettings.FalloffType.Linear => (peakHeight - (voronoiSettings.falloffAmount * maximumLength)),
                        VoronoiSettings.FalloffType.PowSin => (peakHeight - Mathf.Pow(scaled * 3, voronoiSettings.falloffAmount))-Mathf.Sin(scaled*2f*3.14f)/voronoiSettings.dropoffAmount,
                        VoronoiSettings.FalloffType.Plateau => peakHeight - Mathf.Sin(scaled)
                                                                 * Mathf.Pow(scaled, (10 - voronoiSettings.falloffAmount))
                                                                 / ((10-voronoiSettings.dropoffAmount)/1000),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    // Assign the height in the heightmap
                    if (hMap[i,y] < height) hMap[i, y] = height;
                }
            }
        }
        // Apply the changes made to the heightmap
        terrainData.SetHeights(0, 0, hMap);
    }

    /// <summary>
    /// Uses the Diamond Square Algorithm for midpoint displacement.
    /// </summary>
    public void DiamondSquareAlgorithm()
    {
        var hMap = Heightmap();
        int square; // hMapWidth is the width of the current heightmap, square is the current width of the square being worked on

        var hMapWidth = square = terrainData.heightmapResolution - 1;

        var minHeight = diamondSquareSettings.MinHeight;
        var maxHeight = diamondSquareSettings.MaxHeight;
        
        var dampeningEffect = Mathf.Pow(diamondSquareSettings.DampenerAmount, diamondSquareSettings.RoughnessExponent * -1);
        
        while (0 < square)
        {
            Vector3 cornerPos, midPos;
            for (var i = 0; i < hMapWidth; i += square)
            {
                for (var j = 0; j < hMapWidth; j += square)
                {
                    // Calculate the coords of the corner
                    cornerPos = new Vector2(square + i, square + j);
                    // Calculate coords for the Midpoint
                    midPos = new Vector2( i + square / 2, j + square / 2);

                    // Calculate the average height, then add some additional height (or subtract)
                    var height = (hMap[i, j] + // Bottom left
                                  hMap[(int) cornerPos.x, j] +
                                  hMap[i, (int) cornerPos.y] +
                                  hMap[(int) cornerPos.x, (int) cornerPos.y]) / 4 +  Maths.RandomRange(minHeight, maxHeight);
                    
                    hMap[(int) midPos.x, (int) midPos.y] = height;

                }
            }

            for (var i = 0; i < hMapWidth; i+= square)
            {
                for (var j = 0; j < hMapWidth; j+= square)
                {
                    cornerPos = new Vector3(i + square, j + square);
                    midPos = new Vector2( i + square / 2, j + square / 2);

                    // Calculate midpoint positions
                    Vector4 squareData; // x: Midpoint Left X, y: Midpoint Right X, z: Midpoint Upper Y, w: Midpoint Lower Y; Used for square step
                    squareData.x = midPos.x - square; //    x: Midpoint Left X
                    squareData.y = midPos.x + square; //    y: Midpoint Right X

                    squareData.z = midPos.y + square; //    z: Midpoint Upper Y,
                    squareData.w = midPos.y - square; //    w: Midpoint Lower Y;
                    
                    // Don't calculate if we're going beyond the heightmap
                    // (happens on first couple of iterations, where we're on outer edge of hMap)
                    // TODO: If out of bounds, wrap around to use values from opposite side of the hMap
                    if (OutOfHeightMapBounds(squareData, hMapWidth))
                    {
                        continue;
                    }
                    
                    // Bottom Middle
                    // Calculate the average height, then add some additional height (or subtract)
                    hMap[(int) midPos.x, j] = (hMap[(int) midPos.x, (int) midPos.y] +       // Middle
                                               hMap[i, j] +                                 // Bottom left
                                               hMap[(int) midPos.x, (int) squareData.w] +   // Lower middle
                                               hMap[(int) cornerPos.x, j])                  // Bottom right
                                              / 4f + Maths.RandomRange(minHeight, maxHeight);

                    // Top Middle
                    hMap[(int) midPos.x, (int) cornerPos.y] =
                        (hMap[(int) midPos.x, (int) squareData.z] +     // Upper middle
                         hMap[i, (int) cornerPos.y] +                   // Top left
                         hMap[(int) midPos.x, (int) midPos.y] +         // Middle
                         hMap[(int) cornerPos.x, (int) cornerPos.y]     // Top right
                        ) / 4f + Maths.RandomRange(minHeight, maxHeight);
                    
                    // Left Middle
                    hMap[i, (int)midPos.y] = (
                        hMap[i, (int) cornerPos.y] +                // Top left
                        hMap[(int)squareData.x, (int) midPos.y] +   // Outer left middle
                        hMap[i, j] +                                // Bottom left
                        hMap[(int) midPos.x, (int) midPos.y])       // Middle
                                             / 4f + Maths.RandomRange(minHeight, maxHeight) ;

                    // Right Middle
                    hMap[(int) cornerPos.x, (int) midPos.y] = 
                        ( hMap[(int) cornerPos.x, (int) cornerPos.y] +  // Top right
                          hMap[(int) midPos.x, (int) midPos.y] +        // Middle
                          hMap[(int) cornerPos.x, j] +                  // Bottom right
                          hMap[(int)squareData.y, (int) midPos.y])      // Outer left middle
                        / 4f + Maths.RandomRange(minHeight, maxHeight);

                }
            }

            square /= 2;
            minHeight *= dampeningEffect;
            maxHeight *= dampeningEffect;
        }
        

      
        terrainData.SetHeights(0, 0, hMap);
    }

    // Smooths the terrain by averaging out each points neighbours
    public void BlurTerrain()
    {
        // Display a progress bar to the user
        progress = 0;
        CreateProgressBar("Blurring Terrain", blurIterations);
        
        for (var i = 1; i <= blurIterations; i++)
        {
            var heightMap = GetHeightMap();

            var blurred = TerrainAlgorithms.BlurAlgorithm(heightMap, terrainData.heightmapResolution);
            terrainData.SetHeights(0, 0, blurred);
            
            progress = i;
            CreateProgressBar("Blurring Terrain", blurIterations);
        }
    }
    #endregion

    #region WeightMaps

    public void ApplyWeightMaps()
    {
        Debug.Log("Applying weightmaps..");

        var count = weightMaps.Count;
        var newTerrainLayers = new TerrainLayer[count];
        
        terrainData.terrainLayers = newTerrainLayers;

        var index = 0;
        foreach (var weightMap in weightMaps)
        {
            newTerrainLayers[index] = new TerrainLayer
            {
                diffuseTexture = weightMap.DiffuseTexture,
                normalMapTexture = weightMap.NormalsTexture,
                metallic = weightMap.Metallic,
                smoothness = weightMap.Smoothness,
                specular = weightMap.Specular,
                tileOffset = weightMap.Offset,
                tileSize = weightMap.Size
            };
            
            AssetDatabase.CreateAsset(newTerrainLayers[index], GetTerrainLayerPath("Terrain Layer " + index.ToString()));
            index++;
            Selection.activeObject = this.gameObject;
        }
        terrainData.terrainLayers = newTerrainLayers;
        
        var hMap = GetHeightMap();
        // Create new float[,] to store the weightmaps to apply
        var appliedWeightMaps =
            new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        
        // Iterate through x,y of the alphamap
        for (var i = 0; i < terrainData.alphamapHeight; i++)
        {
            for (var j = 0; j < terrainData.alphamapWidth; j++)
            {
                var weights = new float[terrainData.alphamapLayers];
                // Iterate through the weightmaps
                for (var k = 0; k < count; k++)
                {
                    
                    // Create blending amount using perlin noise with current x and y coords
                    var blendModifier = 0.01 + 0.01 * Mathf.PerlinNoise(j * 0.01f, i * 0.01f);

                    // Get gradient at current WeightMap position
                    // var gradient = Maths.CalculateGradient(hMap, j, i, terrainData.heightmapResolution,
                    //     terrainData.heightmapResolution);

                    var gradient = terrainData.GetSteepness(i / (float) terrainData.alphamapHeight,
                        j / (float) terrainData.alphamapWidth);
                    
                    // Only add if the height at j, i is inbetween the minimum and max weights
                    if (hMap[j, i] >= weightMaps[k].MINWeight - blendModifier && hMap[j, i] <= weightMaps[k].MAXWeight + blendModifier
                        && (gradient >= weightMaps[k].MINGradient && gradient <= weightMaps[k].MAXGradient))
                    {
                        weights[k] = 1;
                    }
                }

                weights = Maths.NormaliseArray(weights);
                for (var k = 0; k < weights.Length; k++)
                {
                    appliedWeightMaps[j, i, k] = weights[k];
                }
            }
        }
        
        terrainData.SetAlphamaps(0,0, appliedWeightMaps);
        
    }
    
    public void CreateNewWeightMap()
    {
        weightMaps.Add(new WeightMap());
    }
    public void DeleteWeightMap()
    {
        weightMaps = DeleteFromList(weightMaps);
    }
    #endregion

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
                    heightMap[x, y] = heightMap[x, y] + TerrainAlgorithms.FractalBrownianMotion(
                        (x + settings.Seed) * settings.ScaleX,
                        (y + settings.Seed) * settings.ScaleY,
                        settings.FBmOctaves,
                        settings.FBmPersistence) * settings.ScaleHeight;
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
        perlinNoiseSettingsList = DeleteFromList(perlinNoiseSettingsList);
    }
    
    // Generic function for removing items from list and replacing element if empty (new() is needed to allow calling T constructor)
    private static List<T> DeleteFromList<T>(List<T> list) where T : IDeletable, new()
    {
        // Iterate through all the settings that have been marked for deletion and remove them
        foreach (var element in list.Reverse<T>()
            .Where(element => element.ToRemove))
        {
            list.Remove(element);
        }
        
        // If no settings remain, add one back
        if(list.Count == 0) list.Add(new T());
        
        return list;
    }
    #endregion
    
    #region HeightmapUtils

    public int GetTerrainSize()
    {
        return terrainData.heightmapResolution;
    }
    
    public int GetTerrainAlphaMapSize()
    {
        return terrainData.alphamapResolution;
    }
    
    private static bool OutOfHeightMapBounds(Vector4 squareData, int size)
    {
        return squareData.x <= 0 || squareData.w <= 0
                                 || squareData.y >= size - 1 || squareData.z >= size - 1;

        // return (!(0 >= squareData.x)) && (!(0 >= squareData.w)) &&
        //        
        //        (!(size - 1 <= squareData.y)) && (!(size - 1 <= squareData.z));
    }
    
    public TerrainData GetTerrainData()
    {
        return terrainData;
    }
    
    /// <summary>
    /// Gets the state of the current HeightMap
    /// </summary>
    /// <returns>float[,] HeightMap</returns>
    public float[,] GetHeightMap()
    {
        return terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
    }
    
    /// <summary>
    /// Returns the current heightmap or a new one, if replaceHeightmap is true
    /// </summary>
    /// <returns>A heightmap for the UPTerrain</returns>
    private float[,] Heightmap()
    {
        return replaceHeightmap switch
        {
            false => GetHeightMap(), // Give the current heightmap
            true => new float[terrainData.heightmapResolution, terrainData.heightmapResolution] // Give a new heightmap of heightmap resolution size
        };
    }
    
    /// <summary>
    /// Load a heightmap from our heightMapTexture
    /// </summary>
    public void LoadHeightMapFromTexture()
    {
        var heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // Iterate 
        for (var x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (var y = 0; y < terrainData.heightmapResolution; y++)
            {
                // Assign our heightmap x & y adjusted by our scale
                var heightMapX = Convert.ToInt32((x * heightMapScale.x));
                var heightMapY = Convert.ToInt32((y * heightMapScale.y));
                
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
    
    /// <summary>
    /// Reset terrain to a zeroed out heightmap
    /// </summary>
    public void ResetTerrainHeightMap()
    {
        Debug.Log("Resetting Terrain");

        var heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0,0, heightMap);
    }

    #endregion

    #region Unity

    
    /**
     * Used to create a progress bar displaying Progress
     * Call EditorUtility.ClearProgressBar() when finished
     */
    private void CreateProgressBar(string title, float total)
    {
        var progressPercent = 0f;
        if (progress == 0)
        {
            EditorUtility.DisplayProgressBar(title, "Progress", progress);
        }
        else
        {
            progressPercent = (progress / total);
            EditorUtility.DisplayProgressBar(title, "Progress", progressPercent);
        }
        
        if ((int)progressPercent == 1)
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    
    
    
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
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        var tagsProperty = tagManager.FindProperty("tags");
        
        AddTag(tagsProperty, "UPTerrain");
        AddTag(tagsProperty, "UPCloud");
        AddTag(tagsProperty, "UPWater");
        
        
        // Apply modifications to the tag store
        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "UPTerrain";
    }
    private static void AddTag(SerializedProperty tagsProperty, string tag)
    {
        var isFound = false;
        
        // Check to see if tag is already made


        if (tagsProperty == null)
        {
            Debug.Log("TagsProperty is null");
            return;
        }
        
        for (var i = 0; i < tagsProperty.arraySize; i++)
        {
            var checkTag = tagsProperty.GetArrayElementAtIndex(i);
            if (!checkTag.stringValue.Equals(tag)) continue;
            isFound = true;
            break;
        }
        
        // If tag hasn't been found add it
        if (isFound) return;
        Debug.Log("Adding: " + tag);
        tagsProperty.InsertArrayElementAtIndex(0);
        var newTag = tagsProperty.GetArrayElementAtIndex(0);
        newTag.stringValue = tag;
    }
    #endregion

    
}
