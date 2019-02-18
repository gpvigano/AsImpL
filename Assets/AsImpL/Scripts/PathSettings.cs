using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsImpL
{
    public enum RootPathEnum
    {
        Url,
        DataPath,
        DataPathParent,
        PersistentDataPath,
        CurrentPath
    }

    public class PathSettings : MonoBehaviour
    {

        [Tooltip("Default root path for models")]
        public RootPathEnum defaultRootPath = RootPathEnum.Url;

        [Tooltip("Root path for models on mobile devices")]
        public RootPathEnum mobileRootPath = RootPathEnum.Url;


        public static PathSettings Get(GameObject obj)
        {
            PathSettings pathSettings = obj.GetComponent<PathSettings>();
            if (pathSettings == null)
            {
                pathSettings = FindObjectOfType<PathSettings>();
            }
            if (pathSettings == null)
            {
                pathSettings = obj.AddComponent<PathSettings>();
            }
            return pathSettings;
        }


        public string RootPath
        {
            get
            {
#if (UNITY_STANDALONE)
                switch (defaultRootPath)
#else
                switch (mobileRootPath)
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
    
    }
}
