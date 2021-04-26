namespace AsImpL
{
    /// <summary>
    /// Interface for classes that select the shader to use based on the material data.
    /// </summary>
    public class ShaderSelector : IShaderSelector
    {
        private readonly string defaultShader;

        public ShaderSelector(string defaultShader = "Standard")
        {
            this.defaultShader = defaultShader;
        }

        /// <inheritdoc/>
        public string Select(MaterialData md)
        {
            return defaultShader;// (md.illumType == 2) ? "Standard (Specular setup)" : defaultShader;
        }
    }
}