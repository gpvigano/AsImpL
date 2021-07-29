namespace AsImpL
{
    /// <summary>
    /// Interface for classes that select the shader to use based on the material data.
    /// </summary>
    public interface IShaderSelector
    {
        string Select(MaterialData md, bool useUnlit, ModelUtil.MtlBlendMode blendMode);
    }
}