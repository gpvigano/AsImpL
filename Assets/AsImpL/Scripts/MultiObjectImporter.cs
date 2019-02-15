using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AsImpL
{
    /// <summary>
    /// Load the objects in the given list with their parameters and positions.
    /// </summary>
    public class MultiObjectImporter : ObjectImporter
    {
        public enum RootPathEnum
        {
            Url,
            DataPath,
            DataPathParent,
            PersistentDataPath,
            CurrentPath
        }

        [Tooltip("Load models in the list on start")]
        public bool autoLoadOnStart = false;

        [Tooltip("Default root path for models")]
        public RootPathEnum defaultRootPath = RootPathEnum.Url;

        [Tooltip("Root path for models on mobile devices")]
        public RootPathEnum mobileRootPath = RootPathEnum.Url;

        [Tooltip("Models to load on startup")]
        public List<ModelImportInfo> objectsList = new List<ModelImportInfo>();

        [Tooltip("Default import options")]
        public ImportOptions defaultImportOptions = new ImportOptions();


        public string RootPath
        {
            get
            {
#if (UNITY_ANDROID || UNITY_IPHONE)
                switch(mobileRootPath)
#else
                switch (defaultRootPath)
#endif
                {
                    case RootPathEnum.DataPath:
                        return Application.dataPath + "/";
                    case RootPathEnum.DataPathParent:
                        return Application.dataPath + "/../";
                    case RootPathEnum.PersistentDataPath:
                        return Application.persistentDataPath + "/";
                }
                return "";
            }
        }

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
                    Debug.LogErrorFormat("File path missing for the model at position {0} in the list.", i);
                    continue;
                }

                filePath = RootPath + filePath;

                ImportOptions options = modelsInfo[i].loaderOptions;
                if (options == null || options.modelScaling==0)
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
                ImportModelListAsync(objectsList.ToArray());
            }
        }

    }
}
