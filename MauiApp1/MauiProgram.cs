﻿using CommunityToolkit.Maui;
using MauiApp1.Pages;
using MauiApp1.Services;
using Microsoft.Extensions.Logging;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Poppins-Regular.ttf", "Poppins");
                    fonts.AddFont("Poppins-Semibold.ttf", "Poppins");
                })
                .UseMauiCommunityToolkit();

            builder.Services.AddSingleton<SignInPage>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddHttpClient<HttpClientService>(client =>
            {
                var baseAddress = DeviceInfo.Platform == DevicePlatform.Android
                    ? (DeviceInfo.DeviceType == DeviceType.Virtual
                        ? "http://10.0.2.2:56114/"
                        : "http://192.168.100.61:56114/")
                        : "http://localhost:56115/";

                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30); // Increase timeout to 30 seconds
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
