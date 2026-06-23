using RoSchmi.BluetoothController.Interfaces;
using RoSchmi.BluetoothController.Converters;

#if WINDOWS
    using RoSchmi.BluetoothController.Services;
#endif


using Microsoft.Extensions.Logging;

namespace MauiBluetoothCerbotController
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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<ViewModels.MainPageViewModel>();
#if WINDOWS
            builder.Services.AddSingleton<IBluetoothBleService, WindowsBluetoothBleService>();
            builder.Services.AddSingleton<BoolToColorConverter>();

#endif


            return builder.Build();
        }
    }
}
