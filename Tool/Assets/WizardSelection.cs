using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = System.Object;

public class WizardSelection : ScriptableWizard
{
    public string objectName = "New Object";
    public Color objectColor = Color.white;


    [MenuItem("Tools/Selection Wizard")]
    private static void OpenWizard()
    {
        DisplayWizard<WizardSelection>("Selection Wizard", "Modify", "Cancel");
    }

    private void OnWizardCreate()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            Renderer renderer = o.GetComponent<Renderer>();
            
            if (renderer != null)
            {
                Undo.RecordObject(renderer.material, "Modify Selection");
                renderer.material.color = objectColor;
            }
        }
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
