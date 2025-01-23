using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SOCreateWindow : EditorWindow
{
    private const string searchFolder = "Assets/ProjetTool/Runtime/ScriptableObjects"; // Dossier où chercher les ScriptableObjects

    private DataEquipment selectedEquipment;      // Instance actuellement sélectionnée
    private DataEquipment editingEquipmentCopy;  // Copie temporaire pour éditer
    private string[] equipmentOptions;           // Liste des noms des ScriptableObjects disponibles
    private int selectedIndex = 0;               // Index sélectionné dans le dropdown

    // Dictionnaire pour dessiner les champs
    private Dictionary<Type, Action<object, FieldInfo>> drawer;

    [MenuItem("Tools/CreateEquipment")]
    public static void ShowWindow()
    {
        SOCreateWindow window = (SOCreateWindow)EditorWindow.GetWindow(typeof(SOCreateWindow));
        window.titleContent = new GUIContent("Create Equipment");
    }

    private void OnEnable()
    {
        // Charger la liste des ScriptableObjects existants
        LoadExistingEquipments();

        // Initialiser le drawer
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

        // Si aucun équipement n'est sélectionné, en créer un par défaut
        if (selectedEquipment == null && equipmentOptions.Length == 0)
        {
            CreateNewEquipment();
        }
    }

    private void LoadExistingEquipments()
    {
        // Trouver les GUID uniquement dans le dossier spécifié
        string[] guids = AssetDatabase.FindAssets("t:DataEquipment", new[] { searchFolder });

        // Créer une liste temporaire pour stocker les noms des équipements
        List<string> options = new List<string> { "Create New Equipment" }; // Option par défaut

        // Charger chaque ScriptableObject à partir des GUID trouvés
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DataEquipment equipment = AssetDatabase.LoadAssetAtPath<DataEquipment>(path);

            if (equipment != null) // Vérifier que l'objet a été chargé avec succès
            {
                options.Add(equipment.name);
            }
        }

        // Stocker les noms des équipements dans l'array equipmentOptions
        equipmentOptions = options.ToArray();

        // Réinitialiser l'index et l'objet sélectionné
        selectedIndex = 0;
        selectedEquipment = null; // Par défaut, aucune sélection
    }

    private void CreateNewEquipment()
    {
        selectedEquipment = CreateInstance<DataEquipment>();
        editingEquipmentCopy = Instantiate(selectedEquipment); // Créer une copie temporaire
    }

    private void StartEditingEquipment(DataEquipment equipment)
    {
        selectedEquipment = equipment;
        editingEquipmentCopy = Instantiate(selectedEquipment); // Créer une copie temporaire
    }

    public void OnGUI()
    {
        // Styles
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

        // Dropdown pour sélectionner ou créer un ScriptableObject
        GUILayout.Label("Select or Create Equipment", titleStyle);
        GUILayout.Space(10);

        int newIndex = EditorGUILayout.Popup("Equipment", selectedIndex, equipmentOptions);
        if (newIndex != selectedIndex)
        {
            selectedIndex = newIndex;

            if (selectedIndex == 0)
            {
                // Option "Create New Equipment"
                CreateNewEquipment();
            }
            else
            {
                // Charger un équipement existant
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

        // Modification de l'équipement sélectionné
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

            // Bouton pour sauvegarder l'équipement
            GUILayout.Space(20);
            if (GUILayout.Button("Save Equipment", buttonStyle, GUILayout.Height(50)))
            {
                SaveEquipment();
            }
        }
    }

    private void SaveEquipment()
    {
        if (selectedEquipment == null || editingEquipmentCopy == null) return;

        // Copier les valeurs de la copie temporaire dans l'objet sélectionné
        Type t = typeof(DataEquipment);
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (FieldInfo field in t.GetFields(flags))
        {
            object value = field.GetValue(editingEquipmentCopy);
            field.SetValue(selectedEquipment, value);
        }

        // Si l'équipement est nouveau, demander où le sauvegarder
        if (!AssetDatabase.Contains(selectedEquipment))
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Equipment", "NewEquipment", "asset", $"Save the Equipment in {searchFolder}");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(selectedEquipment, path);
            }
        }

        // Sauvegarder les changements
        EditorUtility.SetDirty(selectedEquipment);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        LoadExistingEquipments();
        Debug.Log($"Equipment saved: {selectedEquipment.name}");
    }
}

