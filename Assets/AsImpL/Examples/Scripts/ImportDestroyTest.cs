using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsImpL
{
    namespace Examples
    {
        /// <summary>
        /// Demonstrate how to load and destroy a model multiple times.
        /// </summary>
        public class ImportDestroyTest : AsImpLSample
        {
            [SerializeField]
            protected float reloadInterval = 1.0f;


            protected override void Start()
            {
                importOptions.reuseLoaded = false;
                objImporter.ImportedModel += OnModelImported;
                objImporter.ImportModelAsync(objectName, filePath, null, importOptions);
            }

            private void OnModelImported(GameObject obj, string objPath)
            {
                StartCoroutine(ReloadObject(obj));
            }

            private IEnumerator ReloadObject(GameObject obj)
            {
                yield return new WaitForSecondsRealtime(reloadInterval);
                Destroy(obj);
                yield return null;
                objImporter.ImportModelAsync(objectName, filePath, null, importOptions);
                yield return null;
            }
        }
    }
}
