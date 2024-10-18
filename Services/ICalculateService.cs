namespace ServerSideTest.Services;

public interface ICalculateService
{
    Task InitializeDatasetAsync(int size);
    Task<string> GetDatasetAsync(string dataset, string type, int idx);
    Task<string> ValidateDatasetAsync(object validationData);
}