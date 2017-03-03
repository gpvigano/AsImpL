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
            private ObjectImporter objImporter;

            private void Awake()
            {
                objImporter = gameObject.AddComponent<ObjectImporter>();
            }

            private void Start()
            {
                ImportOptions options = new ImportOptions();
                options = new ImportOptions();
                options.modelScaling = 1f;
                options.zUp = true;
                objImporter.ImportModelAsync("MyObject", "models/OBJ_test/objtest_zup.obj", null, options);
            }
        }
    }
}
