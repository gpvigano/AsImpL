using UnityEngine;

namespace AsImpL
{
    /// <summary>
    /// Implementation of IMaterilFactory that creates a material based on the shader name.
    /// </summary>
    public class MaterialFactory : IMaterialFactory
    {
        /// <inheritdoc/>
        public Material Create(string shaderName)
        {
            return new Material(Shader.Find(shaderName));
        }
    }
}