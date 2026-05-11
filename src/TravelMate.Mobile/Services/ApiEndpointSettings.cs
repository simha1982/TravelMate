namespace TravelMate.Mobile.Services;

public sealed class ApiEndpointSettings
{
    private const string ApiBaseUrlKey = "travelmate_api_base_url";

    public string ApiBaseUrl
    {
        get => Preferences.Default.Get(ApiBaseUrlKey, GetDefaultApiBaseUrl());
        set => Preferences.Default.Set(ApiBaseUrlKey, Normalize(value));
    }

    public Uri ApiBaseUri => new(ApiBaseUrl);

    public void Reset() => Preferences.Default.Remove(ApiBaseUrlKey);

    public static string GetDefaultApiBaseUrl() =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5068/"
            : "http://localhost:5068/";

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GetDefaultApiBaseUrl();
        }

        var trimmed = value.Trim();
        return trimmed.EndsWith("/", StringComparison.Ordinal) ? trimmed : $"{trimmed}/";
    }
}
