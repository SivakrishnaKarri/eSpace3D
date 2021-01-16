namespace Piglet
{
    /// <summary>
    /// Passed to progress callbacks to indicate the
    /// type of glTF entity currently being imported.
    /// </summary>
    public enum ImportStep
    {
        Read,
        Download,
        Parse,
        Buffer,
        Image,
        Texture,
        Material,
        Mesh,
        Node,
        MorphTarget,
        Skin,
        Animation,
        None
    };
}