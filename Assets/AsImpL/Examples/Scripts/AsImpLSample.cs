using UnityEngine;

namespace AsImpL
{
    namespace Examples
    {
        /// <summary>
        /// Demonstrate how to load a model with ObjectImporter.
        /// </summary>
        public class AsImpLSample : MonoBehaviour
        {
            public string filePath = "models/OBJ_test/objtest_zup.obj";
            public ImportOptions importOptions = new ImportOptions();
            private ObjectImporter objImporter;

            private void Awake()
            {
#if (UNITY_ANDROID || UNITY_IPHONE)
                filePath = Application.persistentDataPath + "/" + filePath;
#endif
                objImporter = gameObject.AddComponent<ObjectImporter>();
            }

            private void Start()
            {
                objImporter.ImportModelAsync("MyObject", filePath, null, importOptions);
            }
        }
    }
}
