using System.IO;
using UnityEngine;
using UnityEditor;

namespace AsImpL
{
    /// <summary>
    /// Editor window for OBJ importing configuration and control
    /// </summary>
    /// TODO: replace this with a wizard and support more formats
    public class ObjImportWindow : EditorWindow
    {
        private string defaultImportPath = "";
        private string lastPath = string.Empty;
        private float scale = 1f;
        private bool convertUpAxis = false;
        private bool convertToDoubleSided = false;
        private bool litDiffuseMap = false;
        private bool buildColliders = false;
        private bool colliderConvex = false;
        private bool colliderTrigger = false;
#if !UNITY_2018_3_OR_NEWER
        private bool colliderInflate = false;
        public float colliderSkinWidth = 0.01f;
#endif
        private bool importAssets = false;
        private string importAssetPath = "ImportedOBJ";
#if UNITY_2017_3_OR_NEWER
        private bool use32bitIndices = false;
#endif
        private bool hideWhileLoading = false;

        private bool loading = false;
        private GameObject objObject;
        private ObjectImporter objImporter;


        [MenuItem("Assets/Import OBJ model... [AsImpL]", false, 20)]
        private static void ShowWindow()
        {
            GetWindow<ObjImportWindow>(false, "OBJ import", true);
        }


        [MenuItem("Window/Capture screenshot [AsImpL]", false)]
        private static void Screenshot()
        {
            EditorUtil.AutoCaptureScreenshot("AsImpL");
        }


        private void SaveSettings()
        {
            defaultImportPath = defaultImportPath.Replace('\\', '/');
            importAssetPath = importAssetPath.Replace('\\', '/');
            Debug.Log("Default path set to: " + defaultImportPath);
            Debug.Log("Import path set to: " + importAssetPath);
            EditorPrefs.SetString("AsImpL_ImportPath", defaultImportPath);
            EditorPrefs.SetString("AsImpL_LastPath", lastPath);
            EditorPrefs.SetFloat("AsImpL_AssetScale", scale);
            EditorPrefs.SetBool("AsImpL_AssetVertAxis", convertUpAxis);
            EditorPrefs.SetBool("AsImpL_AssetDoubleSided", convertToDoubleSided);
            EditorPrefs.SetBool("AsImpL_DiffuseHasLightMap", litDiffuseMap);
            EditorPrefs.SetBool("AsImpL_BuildColliders", buildColliders);
            EditorPrefs.SetBool("AsImpL_ColliderConvex", colliderConvex);
            EditorPrefs.SetBool("AsImpL_ColliderTrigger", colliderTrigger);
#if !UNITY_2018_3_OR_NEWER
            EditorPrefs.SetBool("AsImpL_ColliderInflate", colliderInflate);
            EditorPrefs.SetFloat("AsImpL_ColliderSkinWidth", colliderSkinWidth);
#endif
            EditorPrefs.SetBool("AsImpL_HideWhileLoading", hideWhileLoading);
            EditorPrefs.SetBool("AsImpL_ImportAssets", importAssets);
            EditorPrefs.SetString("AsImpL_AssetPath", importAssetPath);
#if UNITY_2017_3_OR_NEWER
            EditorPrefs.SetBool("AsImpL_Use32bitIndices", use32bitIndices);
#endif
        }


        private void ResetSettings()
        {
            if (EditorPrefs.HasKey("AsImpL_ImportPath"))
            {
                defaultImportPath = EditorPrefs.GetString("AsImpL_ImportPath");
            }
            if (EditorPrefs.HasKey("AsImpL_LastPath"))
            {
                lastPath = EditorPrefs.GetString("AsImpL_LastPath");
            }
            if (EditorPrefs.HasKey("AsImpL_AssetScale"))
            {
                scale = EditorPrefs.GetFloat("AsImpL_AssetScale");
            }
            if (EditorPrefs.HasKey("AsImpL_AssetVertAxis"))
            {
                convertUpAxis = EditorPrefs.GetBool("AsImpL_AssetVertAxis");
            }
            if (EditorPrefs.HasKey("AsImpL_AssetDoubleSided"))
            {
                convertToDoubleSided = EditorPrefs.GetBool("AsImpL_AssetDoubleSided");
            }
            if (EditorPrefs.HasKey("AsImpL_DiffuseHasLightMap"))
            {
                litDiffuseMap = EditorPrefs.GetBool("AsImpL_DiffuseHasLightMap");
            }
            if (EditorPrefs.HasKey("AsImpL_BuildColliders"))
            {
                buildColliders = EditorPrefs.GetBool("AsImpL_BuildColliders");
            }
            if (EditorPrefs.HasKey("AsImpL_ColliderConvex"))
            {
                colliderConvex = EditorPrefs.GetBool("AsImpL_ColliderConvex");
            }
            if (EditorPrefs.HasKey("AsImpL_ColliderTrigger"))
            {
                colliderTrigger = EditorPrefs.GetBool("AsImpL_ColliderTrigger");
            }
#if !UNITY_2018_3_OR_NEWER
            if (EditorPrefs.HasKey("AsImpL_ColliderInflate"))
            {
                colliderInflate = EditorPrefs.GetBool("AsImpL_ColliderInflate");
            }
            if (EditorPrefs.HasKey("AsImpL_ColliderSkinWidth"))
            {
                colliderSkinWidth = EditorPrefs.GetFloat("AsImpL_ColliderSkinWidth");
            }
#endif
            if (EditorPrefs.HasKey("AsImpL_HideWhileLoading"))
            {
                hideWhileLoading = EditorPrefs.GetBool("AsImpL_HideWhileLoading");
            }
            if (EditorPrefs.HasKey("AsImpL_ImportAssets"))
            {
                importAssets = EditorPrefs.GetBool("AsImpL_ImportAssets");
            }
            if (EditorPrefs.HasKey("AsImpL_AssetPath"))
            {
                importAssetPath = EditorPrefs.GetString("AsImpL_AssetPath");
            }
#if UNITY_2017_3_OR_NEWER
            if (EditorPrefs.HasKey("AsImpL_use32bitIndices"))
            {
                use32bitIndices = EditorPrefs.GetBool("AsImpL_use32bitIndices");
            }
#endif
        }


        private void OnEnable()
        {
            ResetSettings();
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            defaultImportPath = EditorGUILayout.TextField("Default import path:", defaultImportPath);
            if (GUILayout.Button("...", GUILayout.Width(24), GUILayout.Height(15)))
            {
                defaultImportPath = EditorUtility.OpenFolderPanel("Select a folder", defaultImportPath, "");
                defaultImportPath = defaultImportPath.Replace('\\', '/');
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            lastPath = EditorGUILayout.TextField("OBJ file path:", lastPath);
            if (!string.IsNullOrEmpty(lastPath))
            {
                if (GUILayout.Button("^", GUILayout.Width(24), GUILayout.Height(15)))
                {
                    FileInfo fileInfo = new FileInfo(lastPath);
                    defaultImportPath = fileInfo.Directory.FullName;
                    defaultImportPath = defaultImportPath.Replace('\\', '/');
                }
            }
            if (GUILayout.Button("...", GUILayout.Width(24), GUILayout.Height(15)))
            {
                string[] filters = { "OBJ files", "obj", "All files", "*" };
                string absolutePath = EditorUtility.OpenFilePanelWithFilters("Laod OBJ model", defaultImportPath, filters);
                if (!string.IsNullOrEmpty(absolutePath))
                {
                    FileInfo fileInfo = new FileInfo(absolutePath);
                    Debug.Log(fileInfo.Directory.FullName);
                    //EditorPrefs.SetString("OBJ Import path", fileInfo.Directory.FullName);
                    lastPath = absolutePath;
                }
            }
            EditorGUILayout.EndHorizontal();

            scale = EditorGUILayout.FloatField("Scale:", scale);
            convertUpAxis = EditorGUILayout.Toggle("Convert vertical axis", convertUpAxis);
            convertToDoubleSided = EditorGUILayout.Toggle("Convert to double-sided (duplicate&flip polygons)", convertToDoubleSided);
            litDiffuseMap = EditorGUILayout.Toggle("Lit diffuse map", litDiffuseMap);
            buildColliders = EditorGUILayout.Toggle("Generate mesh colliders", buildColliders);
            if (buildColliders)
            {
                colliderConvex = EditorGUILayout.Toggle("Convex mesh colliders", colliderConvex);
                if (colliderConvex)
                {
                    EditorGUILayout.HelpBox(
                        "Building convex meshes may not work for meshes with too many smooth surface regions.\n" +
                        "If you get errors find each involved object and fix its mesh collider (e.g. remove it or uncheck \"Convex\").",
                        MessageType.Warning);
                    colliderTrigger = EditorGUILayout.Toggle("Mesh colliders as trigger", colliderTrigger);
#if !UNITY_2018_3_OR_NEWER
                    colliderInflate = EditorGUILayout.Toggle("Mesh colliders inflated", colliderInflate);
                    colliderSkinWidth = EditorGUILayout.FloatField("Mesh colliders inflation amount", colliderSkinWidth);
#endif
                }
            }
#if UNITY_2017_3_OR_NEWER
            use32bitIndices = EditorGUILayout.Toggle("Use 32 bit indices", use32bitIndices);
#endif
            hideWhileLoading = EditorGUILayout.Toggle("Hide while loading", hideWhileLoading);

            importAssets = EditorGUILayout.Toggle("Import assets", importAssets);
            if (importAssets)
            {
                importAssetPath = EditorGUILayout.TextField("OBJ asset path:", importAssetPath);
                importAssetPath = importAssetPath.Replace('\\', '/');
            }

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset settings", GUILayout.Width(100), GUILayout.Height(24)))
            {
                ResetSettings();
            }
            if (GUILayout.Button("Save settings", GUILayout.Width(100), GUILayout.Height(24)))
            {
                SaveSettings();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Import", GUILayout.Width(80), GUILayout.Height(24)))
            {
                lastPath = lastPath.Replace('\\', '/');
                string absolute_path = lastPath;
                if (!string.IsNullOrEmpty(absolute_path))
                {
                    objObject = new GameObject();
                    objObject.name = "OBJ Loader";
                    objImporter = objObject.AddComponent<ObjectImporter>();
                    objImporter.importAssets = importAssets;
                    objImporter.importAssetPath = importAssetPath;

                    GameObject parentObject = Selection.activeObject as GameObject;
                    if (parentObject && !parentObject.activeInHierarchy)
                    {
                        parentObject = null;
                    }
                    ImportOptions opt = new ImportOptions();
                    opt.zUp = convertUpAxis;
                    opt.litDiffuse = litDiffuseMap;
                    opt.convertToDoubleSided = convertToDoubleSided;
                    opt.modelScaling = scale;
                    opt.buildColliders = buildColliders;
                    opt.colliderTrigger = colliderTrigger;
                    opt.colliderConvex = colliderConvex;
#if !UNITY_2018_3_OR_NEWER
                    opt.colliderInflate = colliderInflate;
                    opt.colliderSkinWidth = colliderSkinWidth;
#endif
                    opt.hideWhileLoading = hideWhileLoading;
                    objImporter.ImportFile(absolute_path, parentObject ? parentObject.transform : null, opt);
                    loading = true;
                }
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(80), GUILayout.Height(24)))
            {
                EditorUtility.ClearProgressBar();
                if (loading)
                {
                    if (objObject) DestroyImmediate(objObject);
                    loading = false;
                }
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }


        private void Update()
        {
            if (objObject && objImporter && loading)
            {
                // force an update to let couroutine run
                // TODO: find a better way
                objObject.transform.localPosition = Vector3.one * 0.001f;
                objObject.transform.localPosition = Vector3.zero;
                objImporter.UpdateStatus();

                //loading = (ObjLoader.totalProgress.fileProgress.Count > 0);
                float progress = 0;
                string title = string.Empty;
                string msg = string.Empty;
                // TODO: check if an exception occurred in one or more loaders and clear the progress bar
                if (objImporter.AllImported)
                {
                    // done
                    FileInfo fileInfo = new FileInfo(lastPath);
                    string fileName = fileInfo.Name;
                    GameObject loadedObj = LoaderObj.GetModelByPath(lastPath);
                    if (loadedObj)
                    {
                        // detach the loaded object from the importer in case no custom parent object is provided
                        if (objObject.transform.childCount > 0 && loadedObj == objObject.transform.GetChild(0).gameObject)
                        {
                            loadedObj.transform.SetParent(null, false);
                        }
                        Undo.RegisterCreatedObjectUndo(loadedObj, "Import OBJ");
                        Selection.activeObject = loadedObj;
                        DestroyImmediate(objObject);
                        EditorUtility.ClearProgressBar();
                        loading = false;
                        Close();
                    }
                }
                else
                {
                    progress = objImporter.ImportProgress;
                    if (progress > 0)
                    {
                        if (progress >= 100f)
                        {
                            title = "Wait please...";
                            msg = "Finalizing...";
                        }
                        else
                        {
                            title = "Importing " + ((int)progress).ToString() + "%";
                            msg = objImporter.ImportMessage;
                        }
                        EditorUtility.DisplayProgressBar(title, msg, progress / 100f);
                    }
                }
            }
        }


        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
