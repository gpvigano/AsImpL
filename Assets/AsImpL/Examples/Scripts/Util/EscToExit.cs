using UnityEngine;

namespace AsImpL
{
    namespace Examples
    {
        /// <summary>
        /// Quit when ESC key is pressed
        /// </summary>
        public class EscToExit : MonoBehaviour
        {
            void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Debug.Log("Quit");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
        }
    }
}

