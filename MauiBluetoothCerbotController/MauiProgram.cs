using RoSchmi.BluetoothController.Interfaces;
using MauiBluetoothCerbotController.ViewModels;
using MauiBluetoothCerbotController.Converters;
using MauiBluetoothCerbotController.Services;



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
            builder.Services.AddSingleton<IAppLogger, AppLogger>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<ViewModels.MainPageViewModel>();
            builder.Services.AddSingleton<BoolToColorConverter>();
            builder.Services.AddSingleton<InverseBoolConverter>();

#if WINDOWS
            builder.Services.AddSingleton<IBluetoothBleService, WindowsBluetoothBleService>();
            

#endif


            return builder.Build();
        }
    }
}
