using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AsImpL
{
    /// <summary>
    /// Examples for demonstrating AsImpL features.
    /// </summary>
    namespace Examples
    {
        /// <summary>
        /// Load the objects in the given list with their parameters and positions.
        /// </summary>
        public class ObjImporterTest : ObjectImporter
        {
            [Tooltip("Load models in the list on start")]
            public bool autoLoadOnStart = false;

            [Tooltip("Models to load on startup")]
            public List<ModelImportInfo> objectList = new List<ModelImportInfo>();

            [Tooltip("Default scale for models loaded")]
            public float defaultScale = 0.01f;

            [Tooltip("Default vertical axis for models\ntrue = Z axis, false = Y axis")]
            public bool defaultZUp = false;

            [Tooltip("Reuse a model in memory if already loaded (ovverides for each object)")]
            public bool reuseLoaded = true;

            [Tooltip("Inherit parent layer")]
            public bool inheritLayer = false;


            /// <summary>
            /// Load a set of files with their own import options
            /// </summary>
            /// <param name="modelsInfo">List of file import entries</param>
            public void ImportModelListAsync(ModelImportInfo[] modelsInfo)
            {
                if (modelsInfo == null)
                {
                    return;
                }
                for (int i = 0; i < modelsInfo.Length; i++)
                {
                    if (modelsInfo[i].skip) continue;
                    string objName = modelsInfo[i].name;
                    string filePath = modelsInfo[i].path;
                    if (string.IsNullOrEmpty(filePath))
                    {
                        Debug.LogWarning("File path missing");
                        continue;
                    }
#if (UNITY_ANDROID || UNITY_IPHONE)
                    filePath = Application.persistentDataPath + "/" + filePath;
#endif
                    ImportOptions options = modelsInfo[i].loaderOptions;
                    if (options == null)
                    {
                        options = new ImportOptions();
                        options.modelScaling = defaultScale;
                        options.zUp = defaultZUp;
                    }
                    if (defaultScale != 0 && options.localScale == Vector3.zero)
                    {
                        options.localScale.Set(defaultScale, defaultScale, defaultScale);
                    }
                    options.reuseLoaded = reuseLoaded;
                    options.inheritLayer = inheritLayer;
                    ImportModelAsync(objName, filePath, transform, options);
                }
            }

            // Use this for initialization
            protected virtual void Start()
            {
                if (autoLoadOnStart)
                {
                    ImportModelListAsync(objectList.ToArray());
                }

            }

        }

    }
}
