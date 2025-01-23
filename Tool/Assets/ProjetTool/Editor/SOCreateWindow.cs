using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SOCreateWindow : EditorWindow
{
    private const string searchFolder = "Assets/ProjetTool/Runtime/ScriptableObjects"; 

    private DataEquipment selectedEquipment;      
    private DataEquipment editingEquipmentCopy;  
    private string[] equipmentOptions;           
    private int selectedIndex = 0;               

    private Dictionary<Type, Action<object, FieldInfo>> drawer;

    [MenuItem("Tools/CreateEquipment")]
    public static void ShowWindow()
    {
        SOCreateWindow window = (SOCreateWindow)EditorWindow.GetWindow(typeof(SOCreateWindow));
        window.titleContent = new GUIContent("Create Equipment");
    }

    private void OnEnable()
    {
        LoadExistingEquipments();

        drawer = new Dictionary<Type, Action<object, FieldInfo>>()
        {
            { typeof(string), (obj, field) =>
                {
                    string value = (string)field.GetValue(obj);
                    string newValue = EditorGUILayout.TextField(field.Name, value);
                    if (newValue != value)
                        field.SetValue(obj, newValue);
                }
            },
            { typeof(int), (obj, field) =>
                {
                    int value = (int)field.GetValue(obj);
                    int newValue = EditorGUILayout.IntField(field.Name, value);
                    if (newValue != value)
                        field.SetValue(obj, newValue);
                }
            },
            { typeof(Color), (obj, field) =>
                {
                    Color value = (Color)field.GetValue(obj);
                    Color newValue = EditorGUILayout.ColorField(field.Name, value);
                    if (newValue != value)
                        field.SetValue(obj, newValue);
                }
            },
            { typeof(Texture2D), (obj, field) =>
                {
                    Texture2D value = (Texture2D)field.GetValue(obj);
                    Texture2D newValue = (Texture2D)EditorGUILayout.ObjectField(field.Name, value, typeof(Texture2D), false);
                    if (newValue != value)
                        field.SetValue(obj, newValue);
                }
            }
        };
        
        CreateNewEquipment();
    }

    private void LoadExistingEquipments()
    {
        string[] guids = AssetDatabase.FindAssets("t:DataEquipment", new[] { searchFolder });

        List<string> options = new List<string> { "Create New Equipment" }; 
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DataEquipment equipment = AssetDatabase.LoadAssetAtPath<DataEquipment>(path);

            if (equipment != null) 
            {
                options.Add(equipment.name);
            }
        }

        
        equipmentOptions = options.ToArray();

        selectedIndex = 0;
        selectedEquipment = null; 
    }

    private void CreateNewEquipment()
    {
        selectedEquipment = CreateInstance<DataEquipment>();
        editingEquipmentCopy = Instantiate(selectedEquipment); 
    }

    private void StartEditingEquipment(DataEquipment equipment)
    {
        selectedEquipment = equipment;
        editingEquipmentCopy = Instantiate(selectedEquipment);
    }

    public void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = Color.white },
            fontSize = 15
        };

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            hover = { textColor = Color.green }
        };

        GUILayout.Label("Select an equipment to modify or create a new", titleStyle);
        GUILayout.Space(10);

        int newIndex = EditorGUILayout.Popup("Equipment", selectedIndex, equipmentOptions);
        if (newIndex != selectedIndex)
        {
            selectedIndex = newIndex;

            if (selectedIndex == 0)
            {
                CreateNewEquipment();
            }
            else
            {
                string selectedName = equipmentOptions[selectedIndex];
                string[] guids = AssetDatabase.FindAssets($"t:DataEquipment {selectedName}", new[] { searchFolder });
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    DataEquipment equipment = AssetDatabase.LoadAssetAtPath<DataEquipment>(path);
                    StartEditingEquipment(equipment);
                }
            }
        }

        if (editingEquipmentCopy != null)
        {
            Type t = typeof(DataEquipment);
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (FieldInfo field in t.GetFields(flags))
            {
                EditorGUILayout.BeginHorizontal();

                if (drawer.ContainsKey(field.FieldType))
                {
                    drawer[field.FieldType].Invoke(editingEquipmentCopy, field); // Utilise la copie temporaire
                }
                else
                {
                    EditorGUILayout.LabelField($"No drawer for {field.Name} ({field.FieldType})");
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Save Equipment", buttonStyle, GUILayout.Height(50)))
            {
                SaveEquipment();
            }
        }
        EditorGUILayout.HelpBox("Create equipment only in the folder : Runtime/ScriptableObjects",MessageType.Warning);
    }

    private void SaveEquipment()
    {
        if (selectedEquipment == null || editingEquipmentCopy == null) return;

        Type t = typeof(DataEquipment);
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (FieldInfo field in t.GetFields(flags))
        {
            object value = field.GetValue(editingEquipmentCopy);
            field.SetValue(selectedEquipment, value);
        }

        if (!AssetDatabase.Contains(selectedEquipment))
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Equipment", "NewEquipment", "asset", $"Save the Equipment in {searchFolder}");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(selectedEquipment, path);
            }
        }

        EditorUtility.SetDirty(selectedEquipment);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        LoadExistingEquipments();
        Debug.Log($"Equipment saved: {selectedEquipment.name}");
    }
}

