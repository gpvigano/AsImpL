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

            [SerializeField]
            private PathSettings pathSettings;

            private ObjectImporter objImporter;


            private void Awake()
            {
                filePath = pathSettings.RootPath + filePath;
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


            private void OnValidate()
            {
                if(pathSettings==null)
                {
                    pathSettings = PathSettings.FindPathComponent(gameObject);
                }
            }

        }
    }
}
