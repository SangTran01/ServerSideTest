using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ServerSideTest.Services;

namespace ServerSideTest.Controllers;

[ApiController]
[Route("[controller]")]
public class CalculateController : Controller
{
    private readonly ICalculateService _calculateService;

    public CalculateController(ICalculateService calculateService)
    {
        _calculateService = calculateService;
    }

    
    [HttpGet("InitializeDataset")]
    public async Task<IActionResult> InitializeDataset()
    {
        await _calculateService.InitializeDatasetAsync(1000);
        return Ok("Dataset initialized.");
    }
    
    [HttpGet("MultiplyMatrices")]
    public async Task<IActionResult> MultiplyMatrices()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // Retrieve Dataset A and B
        var matrixA = await GetMatrixAsync("A");
        var matrixB = await GetMatrixAsync("B");

        // Measure time for data retrieval
        stopwatch.Stop();
        var dataRetrievalTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();

        // Start measuring computation time
        stopwatch.Start();
        
        // Multiply the matrices
        var resultMatrix = MultiplyMatrix(matrixA, matrixB);
        
        // Concatenate the result matrix into a string
        var concatenatedResult = ConcatenateMatrix(resultMatrix);
        
        // Compute the MD5 hash
        var md5Hash = ComputeMD5Hash(concatenatedResult);
        
        // Measure computation time
        stopwatch.Stop();
        var computationTime = stopwatch.ElapsedMilliseconds;
        
        // Submit the hash for validation
        var isValid = await _calculateService.ValidateDatasetAsync(new { result = md5Hash });
        
        return Json(new
        {
            DataRetrievalTime = $"{dataRetrievalTime} ms",
            ComputationTime = $"{computationTime} ms",
            IsValid = isValid
        });
    }
    
    // Helper method to retrieve dataset (A or B)
    private async Task<double[,]> GetMatrixAsync(string datasetName)
    {
        var matrix = new double[1000, 1000];

        var maxDegreeOfParallelism = 50;

        await Parallel.ForEachAsync(Enumerable.Range(0, 1000),
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, async (i, _) =>
            {
                // Retrieve the JSON result for each row asynchronously
                var rowDataJson = await _calculateService.GetDatasetAsync(datasetName, "row", i);

                // Deserialize the rowDataJson into ApiResult object
                var rowValues = JsonSerializer.Deserialize<ApiResponse>(rowDataJson);

                // Check if the API call was successful
                if (rowValues != null && rowValues.Success)
                {
                    // Assign the values from the 'Value' array to the matrix row manually
                    for (int j = 0; j < rowValues.Value.Length; j++)
                    {
                        matrix[i, j] = rowValues.Value[j];
                    }
                }
                else
                {
                    // Handle potential error (log or throw an exception)
                    throw new Exception(
                        $"Failed to retrieve row {i} from dataset {datasetName}. Cause: {rowValues?.Cause}");
                }
            });

        return matrix;
    }

    // Efficient matrix multiplication using parallel processing
    private double[,] MultiplyMatrix(double[,] matrixA, double[,] matrixB)
    {
        int size = matrixA.GetLength(0);
        var result = new double[size, size];

        Parallel.For(0, size, i =>
        {
            for (int j = 0; j < size; j++)
            {
                double sum = 0;
                for (int k = 0; k < size; k++)
                {
                    sum += matrixA[i, k] * matrixB[k, j];
                }
                result[i, j] = sum;
            }
        });

        return result;
    }

    // Concatenate matrix contents
    private string ConcatenateMatrix(double[,] matrix)
    {
        var sb = new StringBuilder();
        int size = matrix.GetLength(0); // assuming square matrix

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                sb.Append(matrix[i, j].ToString()); // No separator, direct concatenation
            }
        }

        return sb.ToString();
    }

// Compute MD5 hash
    private string ComputeMD5Hash(string input)
    {
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}