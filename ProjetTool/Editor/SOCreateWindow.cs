using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Equipment
{
    public class SOCreateWindow : EditorWindow
    {
        private const string searchFolder = "Assets/ProjetTool/Runtime/ScriptableObjects";
        private const string imageFolder = "Assets/ProjetTool/Editor/Textures";

        private DataEquipment selectedEquipment;
        private DataEquipment editingEquipmentCopy;
        private string[] equipmentOptions;
        private int selectedIndex = 0;

        private Texture2D[] thumbnails;
        private int selectedImageIndex = -1;
        private GUIStyle titleStyle;
        private string errorMessage = null;

        private Dictionary<Type, Action<object, FieldInfo>> drawer;

        private readonly Dictionary<string, Color> rarityColors = new()
        {
            { "Common", Color.gray },
            { "Uncommon", Color.blue },
            { "Rare", Color.magenta },
            { "Legendary", Color.yellow }
        };

        [MenuItem("Tools/CreateEquipment")]
        public static void ShowWindow()
        {
            SOCreateWindow window = (SOCreateWindow)EditorWindow.GetWindow(typeof(SOCreateWindow));
            window.titleContent = new GUIContent("Create Equipment");
            window.minSize = new Vector2(400, 300);
            window.LoadImages();
        }

        private void OnEnable()
        {
            titleStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = Color.white },
                fontSize = 15
            };
            LoadExistingEquipments();
            LoadImages();

            drawer = new Dictionary<Type, Action<object, FieldInfo>>()
            {
                {
                    typeof(string), (obj, field) =>
                    {
                        string value = (string)field.GetValue(obj);
                        string newValue = EditorGUILayout.TextField(field.Name, value);
                        if (newValue != value)
                            field.SetValue(obj, newValue);
                    }
                },
                {
                    typeof(int), (obj, field) =>
                    {
                        int value = (int)field.GetValue(obj);
                        int newValue = EditorGUILayout.IntField(field.Name, value);
                        if (newValue != value)
                            field.SetValue(obj, newValue);
                    }
                },
                {
                    typeof(Color), (obj, field) =>
                    {
                        GUILayout.Label("Select Rarity:", titleStyle);
                        EditorGUILayout.BeginHorizontal();
                        foreach (var rarity in rarityColors)
                        {
                            GUIStyle colorButtonStyle = new GUIStyle(GUI.skin.button)
                            {
                                normal = { background = MakeColorTexture(2, 2, rarity.Value) }
                            };

                            if (GUILayout.Button(rarity.Key, colorButtonStyle, GUILayout.Width(80)))
                            {
                                editingEquipmentCopy.Rarity = rarity.Value;
                            }
                        }

                        Color value = (Color)field.GetValue(obj);
                        Rect rect = GUILayoutUtility.GetRect(75, 75, GUILayout.ExpandWidth(false));
                        EditorGUI.DrawRect(rect, value);
                        EditorGUILayout.EndHorizontal();
                    }
                },
                {
                    typeof(Texture2D), (obj, field) =>
                    {
                        Texture2D value = (Texture2D)field.GetValue(obj);
                        Texture2D newValue =
                            (Texture2D)EditorGUILayout.ObjectField(field.Name, value, typeof(Texture2D), false);
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

        private void LoadImages()
        {
            if (!Directory.Exists(imageFolder)) return;

            string[] imagePaths = Directory.GetFiles(imageFolder, "*.png");
            int maxImages = Mathf.Min(4, imagePaths.Length);
            thumbnails = new Texture2D[maxImages];

            for (int i = 0; i < maxImages; i++)
            {
                thumbnails[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePaths[i]);
            }
        }

        public void OnGUI()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                hover = { textColor = Color.green }
            };

            GUILayout.Label("Select or Create Equipment", titleStyle);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }

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
                    string[] guids =
                        AssetDatabase.FindAssets($"t:DataEquipment {selectedName}", new[] { searchFolder });
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
                        drawer[field.FieldType].Invoke(editingEquipmentCopy, field);
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"No drawer for {field.Name} ({field.FieldType})");
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }


                GUILayout.Label("Select an Icon:", titleStyle);
                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < thumbnails.Length; i++)
                {
                    if (thumbnails[i] != null)
                    {
                        bool isSelected = selectedImageIndex == i;
                        GUIStyle style = new GUIStyle(GUI.skin.box);
                        style.normal.background = isSelected ? MakeColorTexture(2, 2, Color.green) : null;

                        EditorGUILayout.BeginVertical(style, GUILayout.Width(100), GUILayout.Height(100));

                        if (GUILayout.Button(thumbnails[i], GUILayout.Width(100), GUILayout.Height(100)))
                        {
                            selectedImageIndex = i;
                            editingEquipmentCopy.Icone = thumbnails[i];
                        }

                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);
                if (GUILayout.Button("Save Equipment", buttonStyle, GUILayout.Height(50)))
                {
                    SaveEquipment();
                }

                EditorGUILayout.HelpBox("Save the equipment only in : Runtime/ScriptableObjects !",
                    MessageType.Warning);
            }
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
                if (string.IsNullOrEmpty(selectedEquipment.Name))
                {
                    errorMessage = "Chose a name";
                }
                else if (selectedEquipment.Icone == null)
                {
                    errorMessage = "Chose an icon";
                }
                else
                {
                    errorMessage = null;
                    string path = EditorUtility.SaveFilePanelInProject("Save Equipment", selectedEquipment.Name,
                        "asset",
                        $"Save the Equipment in {searchFolder}", searchFolder);
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(selectedEquipment, path);
                        CreateNewEquipment();
                        LoadExistingEquipments();
                    }
                }
            }

            EditorUtility.SetDirty(selectedEquipment);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private Texture2D MakeColorTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}