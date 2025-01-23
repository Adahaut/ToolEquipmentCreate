using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using RectCaca;
using UnityEngine.Animations;

public class RectWindow : EditorWindow
{
    [MenuItem("Tools/RectWindow")]
    public static void ShowWindow()
    {
        RectWindow window = (RectWindow)EditorWindow.GetWindow(typeof(RectWindow));
        window.titleContent = new GUIContent("RectWindow");
    }

    public void OnGUI()
    {
        System.Type t = typeof(Data);

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        Data d = new Data();
        Data d2 = new Data();
        Data d3 = new Data();
        Data d4 = new Data();

        List<Data> list = new List<Data>(4);
        list.Add(d);
        list.Add(d2);
        list.Add(d3);
        list.Add(d4);

        EditorGUILayout.BeginHorizontal();
        foreach (FieldInfo field in t.GetFields(flags))
        {
            object fieldValue = field.GetValue(d);
            EditorGUILayout.LabelField($"Field : {field.Name}({field.FieldType.Name})");
        }

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (FieldInfo fieldInfo in t.GetFields(flags))
            {
                object fieldValue = fieldInfo.GetValue(d);
                if (fieldInfo.FieldType == typeof(Color))
                {
                    EditorGUILayout.ColorField((Color)fieldValue);
                }
                else
                {
                    EditorGUILayout.LabelField(fieldValue?.ToString() ?? "null");
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}