using UnityEngine;

namespace AsImpL
{
    /// <summary>
    /// Interface for classes that create materials.
    /// </summary>
    public interface IMaterialFactory
    {
        Material Create(string shaderName);
    }
}