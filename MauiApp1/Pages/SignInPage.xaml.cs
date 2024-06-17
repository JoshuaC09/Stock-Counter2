using MauiApp1.Services;
using MySqlConnector;
using System.Data;

namespace MauiApp1.Pages;

public partial class SignInPage : ContentPage
{
    private readonly HttpClientService _httpClientService;
    private readonly string _fileName = "config.bgc";

    public SignInPage(HttpClientService httpClientService)
    {
        InitializeComponent();
        _httpClientService = httpClientService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string downloadPath = GetDownloadPath();
        string filePath = Path.Combine(downloadPath, _fileName);

        if (File.Exists(filePath))
        {
            try
            {
                var connectionString = await File.ReadAllTextAsync(filePath);
                var validationResult = await Task.Run(() => ValidateConnectionString(connectionString));

                if (validationResult?.StartsWith("Upload success") == true) // Null check added
                {
                    var apiResult = await _httpClientService.SetConnectionStringAsync(connectionString);
                    if (apiResult)
                    {
                        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                    }
                    else
                    {
                        await DisplayAlert("API Error", "Validation successful, but failed to send connection string to API.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Validation Error", validationResult ?? "Validation result is null", "OK"); // Null check added
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("File Read Error", $"Error reading file: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("File Not Found", $"File not found: {filePath}", "OK");
        }
    }

    private string GetDownloadPath()
    {
#if ANDROID
        var status = Permissions.CheckStatusAsync<Permissions.StorageRead>().Result;
        status = status != PermissionStatus.Granted ? Permissions.RequestAsync<Permissions.StorageRead>().Result : status;
        return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
#else
        var platform = Microsoft.Maui.Devices.DeviceInfo.Platform;
        return platform == Microsoft.Maui.Devices.DevicePlatform.iOS ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) :
               platform == Microsoft.Maui.Devices.DevicePlatform.MacCatalyst ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") :
               Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
    }

    private string ValidateConnectionString(string connStr)
    {
        try
        {
            if (!connStr.Contains("Connection Timeout", StringComparison.OrdinalIgnoreCase))
            {
                connStr += ";Connection Timeout=30;";
            }

            using (var connection = new MySqlConnection(connStr))
            {
                connection.Open();
                return connection.State == ConnectionState.Open ? "Upload success. Connected to the database." : "Failed to connect to the database.";
            }
        }
        catch (MySqlException ex)
        {
            return $"MySQL error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
