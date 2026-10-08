namespace DriveConnect.winforms.Services;

public static class DriveConnectApiConfiguration
{
    private const string EnvironmentVariable = "DRIVECONNECT_API_BASE_URL";

    public static Uri BaseUri
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable(EnvironmentVariable);

            var value = string.IsNullOrWhiteSpace(configured)
                ? "https://localhost:7162"
                : configured.Trim();

            if (!value.EndsWith("/", StringComparison.Ordinal))
                value += "/";

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                throw new InvalidOperationException(
                    $"Invalid DriveConnect API URL. Set {EnvironmentVariable} to a valid http/https URL.");
            }

            return uri;
        }
    }
}
