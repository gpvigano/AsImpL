using UnityEngine;

namespace AsImpL
{
    /// <summary>
    /// Interface for classes that select the shader to use based on the material data.
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