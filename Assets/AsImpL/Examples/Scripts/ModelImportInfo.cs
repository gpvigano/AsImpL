using UnityEngine;

namespace AsImpL
{
    namespace Examples
    {
        /// <summary>
        /// Model import settings, used for batch importing.
        /// </summary>
        [System.Serializable]
        public class ModelImportInfo
        {
            [Tooltip("Name for the game object created\n(leave it blank to use its file name)")]
            public string name;

            [Tooltip("Path relative to the project folder")]
            public string path;

            [Tooltip("Check this to skip this model")]
            public bool skip = false;

            public ImportOptions loaderOptions;

            // Default constructor needed by XmlSerializer
            public ModelImportInfo()
            {
            }

            public ModelImportInfo(GameObject gameObj)
            {
                UpdateFrom(gameObj);
            }

            public void UpdateFrom(GameObject gameObj)
            {
                name = gameObj.name;
                if (loaderOptions == null)
                {
                    loaderOptions = new ImportOptions();
                }
                loaderOptions.localPosition = gameObj.transform.localPosition;
                loaderOptions.localEulerAngles = gameObj.transform.localRotation.eulerAngles;
                loaderOptions.localScale = gameObj.transform.localScale;
            }

            public void ApplyTo(GameObject gameObj)
            {
                gameObj.name = name;
                //game_object.transform.localScale = scale;
                if (loaderOptions != null)
                {
                    gameObj.transform.localPosition = loaderOptions.localPosition;
                    gameObj.transform.localRotation = Quaternion.Euler(loaderOptions.localEulerAngles);
                    gameObj.transform.localScale = loaderOptions.localScale;
                }
            }
        }
    }
}
