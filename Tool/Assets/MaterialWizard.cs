using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;

public class MaterialWizard : ScriptableWizard
{
    public string materialName = "New Object";
    public Color materialColor = Color.white;

    [MenuItem("Tools/Material Wizard")]
    private static void OpenWizard()
    {
        DisplayWizard<MaterialWizard>("Material Wizard", "Create", "Cancel");
    }

    private void OnWizardCreate()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = materialColor;
        
        AssetDatabase.CreateAsset(material, $"Assets/Materials/{materialName}.mat");
        AssetDatabase.SaveAssets();
    }

    private void OnWizardOtherButton()
    {
        focusedWindow.Close();
    }
}
