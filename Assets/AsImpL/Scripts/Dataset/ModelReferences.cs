using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
using Unity.Collections; // ReadOnly attribute
#endif

namespace AsImpL
{
    /// <summary>
    /// Component used to keep track of resources used by a loaded model.
    /// When the component is destroyed all the referenced resources are destroyed.
    /// </summary>
    /// <remarks>This component is added by Loader and it should not be removed.</remarks>
    public class ModelReferences : MonoBehaviour
    {
#if UNITY_2018_1_OR_NEWER
        [ReadOnly]
        [SerializeField]
#endif
        private List<Mesh> meshReferences = new List<Mesh>();
#if UNITY_2018_1_OR_NEWER
        [ReadOnly]
        [SerializeField]
#endif
        private List<Material> materialReferences = new List<Material>();
#if UNITY_2018_1_OR_NEWER
        [ReadOnly]
        [SerializeField]
#endif
        private List<Texture2D> textureReferences = new List<Texture2D>();

        /// <summary>
        /// Add a reference to a created mesh.
        /// </summary>
        /// <param name="mesh">created mesh</param>
        /// <returns>The created mesh itself.</returns>
        public Mesh AddMesh(Mesh mesh)
        {
            meshReferences.Add(mesh);
            return mesh;
        }

        /// <summary>
        /// Add a reference to a created material.
        /// </summary>
        /// <param name="material">created material</param>
        /// <returns>The created material itself.</returns>
        public Material AddMaterial(Material material)
        {
            materialReferences.Add(material);
            return material;
        }

        /// <summary>
        /// Add a reference to a created texture.
        /// </summary>
        /// <param name="texture">created texture</param>
        /// <returns>The created texture itself.</returns>
        public Texture2D AddTexture(Texture2D texture)
        {
            textureReferences.Add(texture);
            return texture;
        }


        /// <summary>
        /// Destroy all the referenced resources when the component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            int numResources = textureReferences.Count + materialReferences.Count + meshReferences.Count;
            foreach (Texture2D texture in textureReferences)
            {
                Destroy(texture);
            }
            foreach (Material material in materialReferences)
            {
                Destroy(material);
            }
            foreach (Mesh mesh in meshReferences)
            {
                Destroy(mesh);
            }
            Debug.LogFormat("{0} resources destroyed for object {1}", numResources, gameObject.name);
        }

    }
}
