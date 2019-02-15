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
            [SerializeField]
            private string filePath = "models/OBJ_test/objtest_zup.obj";
            [SerializeField]
            private string objectName = "MyObject";
            [SerializeField]
            private ImportOptions importOptions = new ImportOptions();

            private ObjectImporter objImporter;


            private void Awake()
            {
#if (UNITY_ANDROID || UNITY_IPHONE)
                filePath = Application.persistentDataPath + "/" + filePath;
#endif
                objImporter = gameObject.GetComponent<ObjectImporter>();
                if (objImporter == null)
                {
                    objImporter = gameObject.AddComponent<ObjectImporter>();
                }
            }


            private void Start()
            {
                objImporter.ImportModelAsync(objectName, filePath, null, importOptions);
            }

        }
    }
}
