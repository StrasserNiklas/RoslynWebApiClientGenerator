namespace TestingContracts.Responses;

public class DataResponse<T>
{
    public DataResponse(T data)
    {
        Data = data;
    }

    public T Data { get; }
}
