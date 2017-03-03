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
        private bool litDiffuseMap = false;
        private bool importAssets = false;
        private string importAssetPath = "ImportedOBJ";

        private bool loading = false;
        private GameObject objObject;
        private ObjectImporter objImporter;

        [MenuItem("AsImpL/Import OBJ model", false)]
        private static void ShowWindow()
        {
            GetWindow<ObjImportWindow>(false, "OBJ import", true);
        }

        [MenuItem("AsImpL/Capture screenshot", false)]
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
            EditorPrefs.SetBool("AsImpL_DiffuseHasLightMap", litDiffuseMap);
            EditorPrefs.SetBool("AsImpL_ImportAssets", importAssets);
            EditorPrefs.SetString("AsImpL_AssetPath", importAssetPath);
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
            if (EditorPrefs.HasKey("AsImpL_DiffuseHasLightMap"))
            {
                litDiffuseMap = EditorPrefs.GetBool("AsImpL_DiffuseHasLightMap");
            }
            if (EditorPrefs.HasKey("AsImpL_ImportAssets"))
            {
                importAssets = EditorPrefs.GetBool("AsImpL_ImportAssets");
            }
            if (EditorPrefs.HasKey("AsImpL_AssetPath"))
            {
                importAssetPath = EditorPrefs.GetString("AsImpL_AssetPath");
            }
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
            litDiffuseMap = EditorGUILayout.Toggle("Lit diffuse map", litDiffuseMap);
            importAssets = EditorGUILayout.Toggle("Import assets", importAssets);
            if (importAssets)
            {
                importAssetPath = EditorGUILayout.TextField("OBJ asset path:", importAssetPath);
                importAssetPath = importAssetPath.Replace('\\', '/');
            }
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

                    //string absolute_path = @"E:\DEV\PROTO\DATA\Models\teleportbeam.obj";
                    //absolute_path = absolute_path.Replace('\\', '/');
                    GameObject parentObject = Selection.activeObject as GameObject;
                    if (parentObject && !parentObject.activeInHierarchy)
                    {
                        parentObject = null;
                    }
                    ImportOptions opt = new ImportOptions();
                    opt.zUp = convertUpAxis;
                    opt.litDiffuse = litDiffuseMap;
                    opt.modelScaling = scale;
                    objImporter.ImportFile(absolute_path, parentObject ? parentObject.transform : null, opt);
                    //ObjLoader loader = new ObjLoader();
                    //loader.zUpToYUp = false;
                    //loader.scaling = Vector3.one * 10f;
                    //Debug.Log("Loading OBJ: " + absolute_path);
                    //loader.Load("obj_import_test", absolute_path, go);
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
                if (objImporter.AllImported)
                {
                    // done
                    FileInfo fileInfo = new FileInfo(lastPath);
                    string fileName = fileInfo.Name;
                    GameObject loadedObj = LoaderObj.GetModelByPath(lastPath);
                    if (loadedObj)
                    {
                        // detach the loaded object from the importer in case
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
