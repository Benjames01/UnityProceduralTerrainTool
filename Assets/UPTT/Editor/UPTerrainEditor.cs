using System;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;


[CustomEditor((typeof(UPTerrain)))]
[CanEditMultipleObjects]
public class UPTerrainEditor : Editor
{
    // UPTerrain properties
    private SerializedProperty _randomHeightRange;
    
    // Editor Foldouts
    private bool _showRandom = true;
    
    
    private void OnEnable()
    {
        // Get the property from the UPTerrain script
        _randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        
        
    }

    public override void OnInspectorGUI()
    {
        // Update all of the serialised values between the editor and the terrain object
        serializedObject.Update();

        UPTerrain terrain = (UPTerrain) target;

        // Random Height Foldout UI
        _showRandom = EditorGUILayout.Foldout(_showRandom, "Random Height");
        if (_showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set heights between given range.", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_randomHeightRange);
            
            if(GUILayout.Button("Generate Random Heights"))
            {
                terrain.RandomTerrain();
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
