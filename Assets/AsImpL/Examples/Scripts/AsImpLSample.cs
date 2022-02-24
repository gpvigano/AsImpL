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
            protected string filePath = "models/OBJ_test/objtest_zup.obj";

            [SerializeField]
            protected string objectName = "MyObject";

            [SerializeField]
            protected ImportOptions importOptions = new ImportOptions();

            [SerializeField]
            protected PathSettings pathSettings;

            protected ObjectImporter objImporter;


            private void Awake()
            {
                filePath = pathSettings.RootPath + filePath;
                objImporter = gameObject.GetComponent<ObjectImporter>();
                if (objImporter == null)
                {
                    objImporter = gameObject.AddComponent<ObjectImporter>();
                }
            }


            protected virtual void Start()
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
