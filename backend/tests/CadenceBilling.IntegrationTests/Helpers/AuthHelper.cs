using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CadenceBilling.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client)
    {
        var email = $"test_{Guid.NewGuid()}@test.com";
        var password = "Test1234!";

        var registerResponse = await client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "Test Company",
            email,
            password
        });

        var content = await registerResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        if (json.RootElement.TryGetProperty("token", out var tokenElement) ||
            json.RootElement.TryGetProperty("Token", out tokenElement))
        {
            return tokenElement.GetString()!;
        }

        throw new InvalidOperationException(
            $"Token not found in response. Status: {registerResponse.StatusCode}. Body: {content}");
    }

    public static void SetBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}