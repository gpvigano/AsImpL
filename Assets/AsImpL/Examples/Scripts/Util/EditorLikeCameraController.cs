using UnityEngine;

namespace AsImpL
{
    namespace Examples
    {
        public class EditorLikeCameraController : MonoBehaviour
        {
#if (UNITY_ANDROID || UNITY_IPHONE)
        void Awake()
        {
            Debug.LogWarning(GetType().Name + " cannot be used for mobile devices.");
        }
#endif
#if ((!UNITY_ANDROID && !UNITY_IPHONE) || UNITY_EDITOR)
            void Update()
            {
                float x = Input.GetAxis("Horizontal") * 0.1f;
                float y = 0.0f;
                float z = Input.GetAxis("Vertical") * 0.1f;

                if (Input.GetMouseButton(1))
                {
                    float rx = Input.GetAxis("Mouse Y") * 2.0f;
                    float ry = Input.GetAxis("Mouse X") * 2.0f;

                    Vector3 rot = transform.eulerAngles;
                    rot.x -= rx;
                    rot.y += ry;
                    transform.eulerAngles = rot;
                }
                if (Input.GetMouseButton(2))
                {
                    x -= Input.GetAxis("Mouse X") * 0.1f;
                    y -= Input.GetAxis("Mouse Y") * 0.1f;
                }
                z += Input.GetAxis("Mouse ScrollWheel");

                transform.Translate(x, y, z);
            }
#endif
        }
    }
}

