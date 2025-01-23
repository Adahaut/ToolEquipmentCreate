using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ExampleWizard : ScriptableWizard
{
    public string objectName = "New Object";
    public Color objectColor = Color.white;

    [MenuItem("Tools/Example Wizard")]
    private static void OpenWizard()
    {
        DisplayWizard<ExampleWizard>("Example Wizard", "Create", "Cancel");
    }

    private void OnWizardCreate()
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.GetComponent<MeshRenderer>().sharedMaterial.color = objectColor;
        obj.name = objectName;
        
        Undo.RegisterCreatedObjectUndo(obj, objectName);

    }

    private void OnWizardUpdate()
    {
        errorString = string.IsNullOrEmpty(objectName) ? "Name is empty" : objectName;
        isValid = !string.IsNullOrEmpty(errorString);
    }

    private void OnWizardOtherButton()
    {
        focusedWindow.Close();
    }
}
