using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ServerSideTest.Services;

public class CalculateService : ICalculateService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://recruitment-test.investcloud.com/api/numbers";

    public CalculateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // GET api/numbers/init/{size}
    public async Task InitializeDatasetAsync(int size)
    {
        var url = $"{BaseUrl}/init/{size}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }

    // GET api/numbers/{dataset}/{type}/{idx}
    public async Task<string> GetDatasetAsync(string dataset, string type, int idx)
    {
        var url = $"{BaseUrl}/{dataset}/{type}/{idx}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    // POST api/numbers/validate
    public async Task<string> ValidateDatasetAsync(object validationData)
    {
        var url = $"{BaseUrl}/validate";
        var content = new StringContent(JsonConvert.SerializeObject(validationData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}