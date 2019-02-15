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

            [Tooltip("Default import options")]
            public ImportOptions defaultImportOptions = new ImportOptions();


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
                        options = defaultImportOptions;
                    }
                    ImportModelAsync(objName, filePath, transform, options);
                }
            }


            /// <summary>
            /// Import the list of objects in objectList.
            /// </summary>
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
