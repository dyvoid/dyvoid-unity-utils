namespace Temple.State
{
    public interface IDataContainer<T>
    {
        T Data { get; set; }
    }
}