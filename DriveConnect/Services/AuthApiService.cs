using System.Net.Http.Json;
using DriveConnect.domain.DTO;

namespace DriveConnect.winforms.Services;

public sealed class AuthApiService
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://localhost:7162")
    };

    public async Task<(LoginResponse? Response, string Error)> LoginAsync(
        string username,
        string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "/auth/login",
                new LoginRequest(username, password));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content
                    .ReadFromJsonAsync<LoginResponse>();

                if (result != null)
                    return (result, string.Empty);

                return (null, "The server returned an empty login response.");
            }

            var message = await response.Content.ReadAsStringAsync();

            return (
                null,
                string.IsNullOrWhiteSpace(message)
                    ? "Invalid username or password."
                    : message.Trim('"'));
        }
        catch (HttpRequestException)
        {
            return (
                null,
                "Cannot connect to the DriveConnect API. Make sure the API is running.");
        }
        catch (TaskCanceledException)
        {
            return (null, "The login request timed out.");
        }
        catch
        {
            return (null, "Something went wrong while logging in.");
        }
    }
}
