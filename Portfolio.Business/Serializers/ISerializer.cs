namespace Portfolio.Business.Serializers
{
    public interface ISerializer<T>
        where T : class
    {
        T Deserialize(string value);

        bool IsValid(string value);

        string Serialize(T value);
    }
}
