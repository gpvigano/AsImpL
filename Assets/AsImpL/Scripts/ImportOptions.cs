using UnityEngine;

namespace AsImpL
{
    [System.Serializable]
    /// <summary>
    /// Options to define how the model will be loaded and imported.
    /// </summary>
    public class ImportOptions
    {
        [Tooltip("load the OBJ file assumitg its vertical axis is Z instead of Y")]
        public bool zUp = true;

        [Tooltip("Consider diffuse map as already lit (disable lighting) if no other texture is present")]
        public bool litDiffuse = false;

        [Tooltip("Consider to double-sided (duplicate and flip faces and normals")]
        public bool convertToDoubleSided = false;

        [Tooltip("Rescaling for the model (1 = no rescaling)")]
        public float modelScaling = 1f;

        [Header("Local Transform for the imported game object")]
        [Tooltip("Position of the object")]
        public Vector3 localPosition = Vector3.zero;

        [Tooltip("Rotation of the object\n(Euler angles)")]
        public Vector3 localEulerAngles = Vector3.zero;

        [Tooltip("Scaling of the object\n([1,1,1] = no rescaling)")]
        public Vector3 localScale = Vector3.one;

        [Tooltip("Reuse a model in memory if already loaded")]
        public bool reuseLoaded = false;

        [Tooltip("Generate mesh colliders")]
        public bool buildColliders = false;
        [Tooltip("Generate convex mesh colliders (only active if buildColliders = true)\nNote: it could not work for meshes with too many smooth surface regions.")]
        public bool colliderConvex = false;
        [Tooltip("Mesh colliders as trigger (only active if colliderConvex = true)")]
        public bool colliderTrigger = false;
        [Tooltip("Mesh colliders inflated (only active if colliderConvex = true)")]
        public bool colliderInflate = false;
        [Tooltip("Mesh colliders inflation amount (only active if colliderInflate = true)")]
        public float colliderSkinWidth = 0.01f;
        [Tooltip("Use 32 bit indices when needed, if available")]
        public bool use32bitIndices = false;
    }
}
