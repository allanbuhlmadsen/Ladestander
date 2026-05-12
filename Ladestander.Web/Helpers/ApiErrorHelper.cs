using System.Net.Http.Json;
using System.Text.Json;
using Ladestander.Web.DTOs;

namespace Ladestander.Web.Helpers;

public static class ApiErrorHelper
{
    public static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string fallbackMessage)
    {
        string content;

        try
        {
            content = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return fallbackMessage;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return fallbackMessage;
        }

        try
        {
            var error = JsonSerializer.Deserialize<ErrorResponseDto>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (error?.Errors is not null && error.Errors.Any())
            {
                return string.Join(
                    " ",
                    error.Errors.SelectMany(e => e.Value));
            }

            return error?.Message ?? fallbackMessage;
        }
        catch
        {
            return fallbackMessage;
        }
    }
}