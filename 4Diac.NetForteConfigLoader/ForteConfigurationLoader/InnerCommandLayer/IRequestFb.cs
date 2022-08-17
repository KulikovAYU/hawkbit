namespace ForteConfigurationLoader.InnerCommandLayer
{
    public interface IRequestFb
    {
        string Action { get; }
        bool HasErrors { get; } 
        string Context { get;}
        int Id { get; }
        string Tag { get;}
    }
}