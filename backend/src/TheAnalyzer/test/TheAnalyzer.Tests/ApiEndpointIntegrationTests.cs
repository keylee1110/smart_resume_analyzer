using Xunit;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TheAnalyzer.Models;

namespace TheAnalyzer.Tests;

/// <summary>
/// Integration tests for API endpoints
/// These tests require a deployed API Gateway and valid authentication tokens
/// Set the API_BASE_URL and AUTH_TOKEN environment variables before running
/// </summary>
public class ApiEndpointIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string? _authToken;
    private readonly bool _skipTests;

    public ApiEndpointIntegrationTests()
    {
        _httpClient = new HttpClient();
        _apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "";
        _authToken = Environment.GetEnvironmentVariable("AUTH_TOKEN");
        
        // Skip tests if environment variables are not set
        _skipTests = string.IsNullOrEmpty(_apiBaseUrl);
        
        if (!_skipTests && !string.IsNullOrEmpty(_authToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _authToken);
        }
    }

    [Fact]
    public async Task AnalyzeEndpoint_ValidInput_Returns200()
    {
        // Skip if not configured
        if (_skipTests)
        {
            return;
        }

        // Arrange
        var request = new AnalyzeRequest
        {
            ResumeText = "John Doe\njohn.doe@example.com\n+1-555-0123\nSkills: C#, .NET, AWS"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/analyze", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify CORS headers
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(responseBody);
        
        var analyzeResponse = JsonSerializer.Deserialize<AnalyzeResponse>(
            responseBody, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(analyzeResponse);
        Assert.NotNull(analyzeResponse.ResumeId);
    }

    [Fact]
    public async Task GetResumeEndpoint_ValidId_Returns200()
    {
        // Skip if not configured
        if (_skipTests)
        {
            return;
        }

        // Arrange
        // First create a resume to get a valid ID
        var analyzeRequest = new AnalyzeRequest
        {
            ResumeText = "Jane Smith\njane@example.com\nSkills: Python, Java"
        };
        
        var analyzeContent = new StringContent(
            JsonSerializer.Serialize(analyzeRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        var analyzeResponse = await _httpClient.PostAsync($"{_apiBaseUrl}/analyze", analyzeContent);
        var analyzeBody = await analyzeResponse.Content.ReadAsStringAsync();
        var analyzeResult = JsonSerializer.Deserialize<AnalyzeResponse>(
            analyzeBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(analyzeResult);
        var resumeId = analyzeResult.ResumeId;

        // Act
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/resumes/{resumeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify CORS headers
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(responseBody);
        
        var profile = JsonSerializer.Deserialize<ProfileRecord>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(profile);
        Assert.Equal(resumeId, profile.ResumeId);
    }

    [Fact]
    public async Task ListResumesEndpoint_ValidAuth_Returns200()
    {
        // Skip if not configured or no auth token
        if (_skipTests || string.IsNullOrEmpty(_authToken))
        {
            return;
        }

        // Act
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/resumes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify CORS headers
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(responseBody);
        
        // Response should be a JSON array
        var resumes = JsonSerializer.Deserialize<List<ProfileRecord>>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(resumes);
    }

    [Fact]
    public async Task GetUploadUrlEndpoint_ValidParams_Returns200()
    {
        // Skip if not configured or no auth token
        if (_skipTests || string.IsNullOrEmpty(_authToken))
        {
            return;
        }

        // Arrange
        var fileName = "test-resume.pdf";
        var contentType = "application/pdf";

        // Act
        var response = await _httpClient.GetAsync(
            $"{_apiBaseUrl}/upload-url?fileName={fileName}&contentType={contentType}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify CORS headers
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(responseBody);
        
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
        Assert.True(result.TryGetProperty("uploadUrl", out var uploadUrl) || 
                    result.TryGetProperty("UploadUrl", out uploadUrl));
        Assert.NotEqual(default, uploadUrl);
    }

    [Fact]
    public async Task CorsPreflightRequest_AnalyzeEndpoint_Returns200()
    {
        // Skip if not configured
        if (_skipTests)
        {
            return;
        }

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, $"{_apiBaseUrl}/analyze");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type,Authorization");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        // CORS preflight should return 200 or 204
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.NoContent,
            $"Expected 200 or 204, got {response.StatusCode}"
        );
        
        // Verify CORS headers are present
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.True(response.Headers.Contains("Access-Control-Allow-Methods"));
        Assert.True(response.Headers.Contains("Access-Control-Allow-Headers"));
    }

    [Fact]
    public async Task CorsPreflightRequest_GetResumeEndpoint_Returns200()
    {
        // Skip if not configured
        if (_skipTests)
        {
            return;
        }

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, $"{_apiBaseUrl}/resumes/test-id");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Authorization");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.NoContent,
            $"Expected 200 or 204, got {response.StatusCode}"
        );
        
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task CorsPreflightRequest_ListResumesEndpoint_Returns200()
    {
        // Skip if not configured
        if (_skipTests)
        {
            return;
        }

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, $"{_apiBaseUrl}/resumes");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Authorization");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.NoContent,
            $"Expected 200 or 204, got {response.StatusCode}"
        );
        
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task CorsPreflightRequest_UploadUrlEndpoint_Returns200()
    {
        // Skip if not configured
        if (_skipTests)
        {
            return;
        }

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, $"{_apiBaseUrl}/upload-url");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Authorization");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.NoContent,
            $"Expected 200 or 204, got {response.StatusCode}"
        );
        
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
