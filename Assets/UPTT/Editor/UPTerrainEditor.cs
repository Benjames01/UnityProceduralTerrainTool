using System;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;


[CustomEditor((typeof(UPTerrain)))]
[CanEditMultipleObjects]
public class UPTerrainEditor : Editor
{
    // UPTerrain properties
    private SerializedProperty _unityRandomRange;
    
    // Height Map settings
    private SerializedProperty _heightMapTexture;
    private SerializedProperty _heightMapScale;
    
    // Editor Foldouts
    private bool _showRandom, _showHeightMapProperties = true;
    
    
    private void OnEnable()
    {
        // Get our properties from the UPTerrain script
        _unityRandomRange = serializedObject.FindProperty("unityRandomRange");
        _heightMapTexture = serializedObject.FindProperty("heightMapTexture");
        _heightMapScale   = serializedObject.FindProperty("heightMapScale");
        
        
    }

    public override void OnInspectorGUI()
    {
        // Update all of the serialised values between the editor and the terrain object
        serializedObject.Update();

        UPTerrain terrain = (UPTerrain) target;
        
        
        // Show ResetButton
        GUILayout.Label("Reset Terrain", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrainHeightMap();
        }

        _showHeightMapProperties = EditorGUILayout.Foldout(_showHeightMapProperties, "HeightMap Settings");
        if (_showHeightMapProperties)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("HeightMap Texture");
            EditorGUILayout.PropertyField(_heightMapTexture);
            EditorGUILayout.PropertyField(_heightMapScale);
            if (GUILayout.Button("Load HeightMap"))
            {
                terrain.LoadHeightMapFromTexture();
            }
        }
        
        // Random Height Foldout UI
        _showRandom = EditorGUILayout.Foldout(_showRandom, "Random HeightMap Generator");
        if (_showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set heights between given range.", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_unityRandomRange);
            
            if(GUILayout.Button("Generate Random Heights"))
            {
                terrain.UnityRandomHeightMap();
            }
            
        }
        
        // Apply the changes made to the object
        serializedObject.ApplyModifiedProperties();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
