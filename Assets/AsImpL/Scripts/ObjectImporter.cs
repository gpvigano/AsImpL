using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AsImpL
{
    /// <summary>
    /// Component that imports objects from a model, both at run-rime and as assets in Editor.
    /// </summary>
    /// <remarks></remarks>
    public class ObjectImporter : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// Import the model as a set of assets. This should be set only by the Editor window.
        /// </summary>
        [HideInInspector]
        public bool importAssets = false;

        /// <summary>
        /// Set the import path for assets. This should be set only by the Editor window.
        /// </summary>
        [HideInInspector]
        public string importAssetPath = "_ImportedOBJ";
#endif
        protected int numTotalImports = 0;
        protected bool allLoaded = false;
        protected ImportOptions buildOptions;
        protected List<Loader> loaderList;

#if UNITY_EDITOR
        // raw subdivision in percentages of the import phases (empirically computed importing a large sample OBJ file)
        // TODO: refine or change this method
        private static float TEX_PHASE_PERC = 13f;
        private static float OBJ_PHASE_PERC = 76f;
        //private static float ASSET_PHASE_PERC = 11f;

        private string importMessage = string.Empty;
#endif
        private ImportPhase importPhase = ImportPhase.Idle;

        /// <summary>
        /// Event triggered when starting to import.
        /// </summary>
        public event Action ImportingStart;

        /// <summary>
        /// Event triggered when finished importing.
        /// </summary>
        public event Action ImportingComplete;

        /// <summary>
        /// Event triggered when a single model has been created and before it is imported.
        /// </summary>
        public event Action<GameObject, string> CreatedModel;

        /// <summary>
        /// Event triggered when a single model has been successfully imported.
        /// </summary>
        public event Action<GameObject, string> ImportedModel;

        /// <summary>
        /// Event triggered when an error occurred importing a model.
        /// </summary>
        public event Action<string> ImportError;

        private enum ImportPhase { Idle, TextureImport, ObjLoad, AssetBuild, Done }


        /// <summary>
        /// Number of pending import activities.
        /// </summary>
        public int NumImportRequests
        {
            get { return numTotalImports; }
        }


#if UNITY_EDITOR
        public bool AllImported
        {
            get
            {
                return importAssets ? importPhase == ImportPhase.Done : allLoaded;
            }
        }


        /// <summary>
        /// Import progress percentage (0..100)
        /// </summary>
        public float ImportProgress
        {
            get
            {
                if (Loader.totalProgress.singleProgress.Count > 0)
                {
                    if (importAssets)
                    {
                        switch (importPhase)
                        {
                            case ImportPhase.TextureImport:
                                return 0f;
                            case ImportPhase.ObjLoad:
                                return TEX_PHASE_PERC + (OBJ_PHASE_PERC / 100f) * Loader.totalProgress.singleProgress[0].percentage;
                            case ImportPhase.AssetBuild:
                                return TEX_PHASE_PERC + OBJ_PHASE_PERC;
                            case ImportPhase.Done:
                                return 100f;
                        }
                    }
                    else
                    {
                        return Loader.totalProgress.singleProgress[0].percentage;
                    }
                }

                return 0f;
            }
        }


        /// <summary>
        /// Message updated while importing of objects 
        /// </summary>
        public string ImportMessage
        {
            get
            {
                if (Loader.totalProgress.singleProgress.Count > 0)
                {
                    if (importAssets)
                    {
                        switch (importPhase)
                        {
                            case ImportPhase.Idle:
                                return string.Empty;
                            case ImportPhase.ObjLoad:
                                return Loader.totalProgress.singleProgress[0].message;
                            case ImportPhase.Done:
                                return "Finalizing....";
                            default:
                                return importMessage;
                        }
                    }
                    else
                    {
                        return Loader.totalProgress.singleProgress[0].message;
                    }
                }

                return string.Empty;
            }
        }


        /// <summary>
        /// Start importing a new object
        /// </summary>
        /// <see cref="ImportOptions"/>
        /// <param name="absolutePath">Absolute path of the file to import</param>
        /// <param name="parentObject">Transform to which attach the new object (it can be null)</param>
        /// <param name="options">Import options</param>
        public void ImportFile(string absolutePath, Transform parentObject, ImportOptions options)
        {
            buildOptions = options;
            StartCoroutine(ImportFileAsync(absolutePath, parentObject));
        }


        /// <summary>
        /// Called as a coroutine by ImportFile() 
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="parentObject"></param>
        /// TODO: refactor this method, it is too long.
        private IEnumerator ImportFileAsync(string absolutePath, Transform parentObject)
        {
            Loader loader = CreateLoader(absolutePath);
            if (loader == null)
            {
                yield break;
            }
            loader.buildOptions = buildOptions;
            Debug.Log("Loading: " + absolutePath);
            float startTotTime = Time.realtimeSinceStartup;
            float startTime = Time.realtimeSinceStartup;

            importPhase = ImportPhase.TextureImport;
            string dirName = Path.GetDirectoryName(absolutePath);
            string sourceBasePath = string.IsNullOrEmpty(dirName) ? "" : dirName;
            if (!sourceBasePath.EndsWith("/"))
            {
                sourceBasePath += "/";
            }

            string newName = Path.GetFileNameWithoutExtension(absolutePath);//fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);

            if (importAssets)
            {
                Debug.LogFormat("Importing assets from {0}...", absolutePath);
                importMessage = "Creating folders...";
                FileInfo fileInfo = new FileInfo(absolutePath);
                string fileName = fileInfo.Name;
                if (!string.IsNullOrEmpty(importAssetPath))
                {
                    EditorUtil.CreateAssetFolder("Assets", importAssetPath);
                    EditorUtil.CreateAssetFolder("Assets/" + importAssetPath, fileName);
                }
                else
                {
                    EditorUtil.CreateAssetFolder("Assets", fileName);
                }

                string prefabRelPath = (!string.IsNullOrEmpty(importAssetPath)) ? importAssetPath + "/" + fileName : fileName;
                string prefabPath = "Assets/" + prefabRelPath;
                string prefabName = prefabPath + "/" + fileName.Replace('.', '_') + ".prefab";

                string[] texturePaths = loader.ParseTexturePaths(absolutePath);
                EditorUtil.CreateAssetFolder(prefabPath, "Textures");
                EditorUtil.CreateAssetFolder(prefabPath, "Materials");
                string destBasePath = Application.dataPath + "/../" + prefabPath;
                foreach (string texPath in texturePaths)
                {
                    string source = texPath;
                    if (!Path.IsPathRooted(source))
                    {
                        source = sourceBasePath + texPath;
                    }
                    FileInfo texFileInfo = new FileInfo(source);
                    string dest = destBasePath + "/Textures/" + texFileInfo.Name;
                    importMessage = "Copying texture " + source + "...";
                    File.Copy(source, dest, true);
                    Debug.LogFormat("Texture {0} copied to {1}", source, dest);
                }
                AssetDatabase.Refresh();
                AssetDatabase.StartAssetEditing();
                foreach (string texPath in texturePaths)
                {
                    FileInfo textFileInfo = new FileInfo(texPath);
                    string texAssetPath = prefabPath + "/Textures/" + textFileInfo.Name;
                    importMessage = "Importing texture " + texAssetPath + "...";
                    EditorUtil.SetTextureReadable(texAssetPath);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.StopAssetEditing();
                //File.Copy(basePath+mtlLibName, destBasePath+mtlLibName, true);
                //File.Copy(absolutePath, destBasePath+fileName, true);

                importMessage = "Updating assets...";
                AssetDatabase.Refresh();
                Debug.LogFormat("Texture files imported in {0} seconds", Time.realtimeSinceStartup - startTime);
                startTime = Time.realtimeSinceStartup;

                importMessage = "Loading OBJ file...";
                importPhase = ImportPhase.ObjLoad;
                loader.altTexPath = prefabPath + "/Textures/";
                loader.buildOptions = buildOptions;
                yield return StartCoroutine(loader.Load(newName, absolutePath, parentObject));

                importMessage = "Saving assets...";
                AssetDatabase.SaveAssets();
                importMessage = "Refreshing assets...";
                AssetDatabase.Refresh();

                Debug.LogFormat("OBJ files loaded in {0} seconds", Time.realtimeSinceStartup - startTime);
                startTime = Time.realtimeSinceStartup;
                GameObject loadedObj = Loader.GetModelByPath(absolutePath);

                importMessage = "Creating mesh assets...";
                importPhase = ImportPhase.AssetBuild;
                // TODO: check if the prefab already exists
                MeshFilter[] meshFilters = loadedObj.GetComponentsInChildren<MeshFilter>();
                foreach (var filter in meshFilters)
                {
                    Mesh mesh = filter.sharedMesh;
                    if (!AssetDatabase.Contains(mesh))
                    {
                        EditorUtil.CreateAssetFolder(prefabPath, "Meshes");
                        AssetDatabase.CreateAsset(mesh, prefabPath + "/Meshes/" + mesh.name + ".asset");
                    }
                }

                importMessage = "Creating material assets...";
                AssetDatabase.StartAssetEditing();
                MeshRenderer[] meshRend = loadedObj.GetComponentsInChildren<MeshRenderer>();
                foreach (var rend in meshRend)
                {
                    Material mtl = rend.sharedMaterial;
                    if (!AssetDatabase.Contains(mtl))
                    {
                        string mtlAssetPath = prefabPath + "/Materials/" + mtl.name + ".mat";
                        AssetDatabase.CreateAsset(mtl, mtlAssetPath);
                    }
                }

                importMessage = "Saving assets...";
                AssetDatabase.SaveAssets();
                AssetDatabase.StopAssetEditing();
                importMessage = "Updating assets...";
                AssetDatabase.Refresh();

                importMessage = "Creating prefab...";
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(loadedObj, prefabName, InteractionMode.AutomatedAction);
#else
                PrefabUtility.CreatePrefab(prefabName, loadedObj, ReplacePrefabOptions.ConnectToPrefab);
#endif
                //GameObject. objObject.GetComponent<OBJ>();
                Debug.LogFormat("Assets created in {0} seconds", Time.realtimeSinceStartup - startTime);
                importPhase = ImportPhase.Done;
            }
            else
            {
                importPhase = ImportPhase.ObjLoad;
                yield return StartCoroutine(loader.Load(newName, absolutePath, parentObject));
            }
            Debug.LogFormat("OBJ files imported in {0} seconds", Time.realtimeSinceStartup - startTotTime);
        }
#endif


        /// <summary>
        /// Create the proper loader cmponent according to the file extension.
        /// </summary>
        /// <param name="absolutePath">path of the model to be imported</param>
        /// <returns>A proper loader or null if not available.</returns>
        private Loader CreateLoader(string absolutePath)
        {
            string ext = Path.GetExtension(absolutePath);
            if (string.IsNullOrEmpty(ext))
            {
                Debug.LogError("No extension defined, unable to detect file format");
                return null;
            }
            Loader loader = null;
            ext = ext.ToLower();
            if (ext.StartsWith(".php"))
            {
                if (!ext.EndsWith(".obj"))
                {
                    // TODO: other formats supported? Remark: often there are zip and rar archives without extension.
                    Debug.LogError("Unable to detect file format in " + ext);
                    return null;
                }
                loader = gameObject.AddComponent<LoaderObj>();
            }
            else
            {
                switch (ext)
                {
                    case ".obj":
                        loader = gameObject.AddComponent<LoaderObj>();
                        break;
                    // TODO: add mode formats here...
                    default:
                        Debug.LogErrorFormat("File format not supported ({0})", ext);
                        return null;
                }
            }
            loader.ModelCreated += OnModelCreated;
            loader.ModelLoaded += OnImported;
            loader.ModelError += OnImportError;

            return loader;
        }


        /// <summary>
        /// Request to load a file asynchronously.
        /// </summary>
        /// <param name="objName"></param>
        /// <param name="filePath"></param>
        /// <param name="parentObj"></param>
        /// <param name="options"></param>
        public void ImportModelAsync(string objName, string filePath, Transform parentObj, ImportOptions options)
        {
            if (loaderList == null)
            {
                loaderList = new List<Loader>();
            }
            if (loaderList.Count == 0)
            {
                numTotalImports = 0;// files.Length;
                if (ImportingStart != null)
                {
                    ImportingStart();
                }
            }
            string absolutePath = filePath.Contains("//") ? filePath : Path.GetFullPath(filePath);
            absolutePath = absolutePath.Replace('\\', '/');
            Loader loader = CreateLoader(absolutePath);
            if (loader == null)
            {
                return;
            }
            numTotalImports++;
            loaderList.Add(loader);
            loader.buildOptions = options;
            if (string.IsNullOrEmpty(objName))
            {
                objName = Path.GetFileNameWithoutExtension(absolutePath);
            }
            allLoaded = false;
            StartCoroutine(loader.Load(objName, absolutePath, parentObj));
        }


        /// <summary>
        /// Update the loading/importing status
        /// </summary>
        public virtual void UpdateStatus()
        {
            if (allLoaded) return;
            int num_loaded_files = numTotalImports - Loader.totalProgress.singleProgress.Count;

            bool loading = num_loaded_files < numTotalImports;
            if (!loading)
            {
                allLoaded = true;
                if (loaderList != null)
                {
                    foreach (var loader in loaderList)
                    {
                        Destroy(loader);
                    }
                    loaderList.Clear();
                }
                OnImportingComplete();
            }
        }


        protected virtual void Update()
        {
            UpdateStatus();
        }


        /// <summary>
        /// Called when finished importing. It triggers ImportingComplete event, if it was set.
        /// </summary>
        protected virtual void OnImportingComplete()
        {
            if (ImportingComplete != null)
            {
                ImportingComplete();
            }
        }


        /// <summary>
        /// Called when each model has been created and before it is imported. It triggers CreatedModel event, if it was set.
        /// </summary>
        protected virtual void OnModelCreated(GameObject obj, string absolutePath)
        {
            if (CreatedModel != null)
            {
                CreatedModel(obj, absolutePath);
            }
        }


        /// <summary>
        /// Called when each model has been imported. It triggers ImportedModel event, if it was set.
        /// </summary>
        protected virtual void OnImported(GameObject obj, string absolutePath)
        {
            if (ImportedModel != null)
            {
                ImportedModel(obj, absolutePath);
            }
        }


        /// <summary>
        /// Called when a model import fails. It triggers ImportError event, if it was set.
        /// </summary>
        protected virtual void OnImportError(string absolutePath)
        {
            if (ImportError != null)
            {
                ImportError(absolutePath);
            }
        }

    }
}
