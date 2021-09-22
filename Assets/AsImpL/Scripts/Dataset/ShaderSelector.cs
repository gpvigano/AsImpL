namespace AsImpL
{
    /// <summary>
    /// Default implementation of IShaderSelector using predefined shaders.
    /// </summary>
    public class ShaderSelector : IShaderSelector
    {
        private readonly string defaultShader;

        public ShaderSelector(string defaultShader = "Standard")
        {
            this.defaultShader = defaultShader;
        }

        /// <inheritdoc/>
        public string Select(MaterialData md, bool useUnlit, ModelUtil.MtlBlendMode blendMode)
        {
            //const bool specularMode = false;// (md.specularTex != null);

            bool? diffuseIsTransparent = null;
            if (useUnlit)
            {
                // do not use unlit shader if the texture has transparent pixels
                diffuseIsTransparent = ModelUtil.ScanTransparentPixels(md.diffuseTex, ref blendMode);
            }

            if (useUnlit && !diffuseIsTransparent.Value)
            {
                return "Unlit/Texture";
            }
            //else if (specularMode)
            //{
            //    return "Standard (Specular setup)";
            //}

            return defaultShader; // (md.illumType == 2) ? "Standard (Specular setup)" : defaultShader
        }
    }
}
