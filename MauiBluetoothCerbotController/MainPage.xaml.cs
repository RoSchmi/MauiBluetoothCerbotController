using MauiBluetoothCerbotController.ViewModels;

namespace MauiBluetoothCerbotController
{
    public partial class MainPage : ContentPage
    {
        // For MVVM in .xaml has to be included:            
        // xmlns:pagemodel="clr-namespace:MauiBluetoothCerbotController"
        // xmlns:viewmodels="clr-namespace:MauiBluetoothCerbotController.ViewModels"
        // x:DataType="viewmodels:MainPageViewModel">
        // In 'MauiProgram.cs' References to MainPage and MainPageViewModel have to be added
        // In 'AppShell.xaml' the 'ShellContent' for each page has to be added 
        // In 'AppShell.xaml.cs' the Navigation routes have to be registered
        // 
        // For Windows the initial Windowsize and -position are set in MauiProgam.cs
        // or can be set in 'App.xaml.cs in an override


        private readonly MainPageViewModel vm;
        int count = 1;


        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            vm = viewModel;

            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (BindingContext as MainPageViewModel).InitializeAsync();
        }




        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}

