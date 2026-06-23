using CommunityToolkit.Mvvm.ComponentModel;
#if WINDOWS
//using MauiBluetoothCerbotController.Services;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
#endif

//using Windows.Devices.Bluetooth;
//using Windows.Devices.Bluetooth.Rfcomm;
//using Windows.Devices.Enumeration;
//using Windows.Networking.Sockets;

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using RoSchmi.BluetoothController.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using RoSchmi.BluetoothController.Models;



namespace MauiBluetoothCerbotController.ViewModels
{


    public partial class MainPageViewModel : ObservableObject
    {
        private IBluetoothBleService _bluetooth;

        //private ObservableCollection<BleDeviceInfo> _deviceInfoCollection = new ObservableCollection<BleDeviceInfo>();

        [ObservableProperty]
        private ObservableCollection<BleDeviceInfo> devices;

        [ObservableProperty]
        private BleDeviceInfo selectedDevice;

        [ObservableProperty]
        private bool isConnected;



#if WINDOWS


        private GattDeviceService bleGattDeviceService;
        private IReadOnlyList<GattDeviceService> gdsServices;
        private IReadOnlyList<GattCharacteristic> gdsCharacteristics;
        private IReadOnlyList<GattCharacteristic> readCharacteristic;
#endif
        private bool connected;
        private int speedLeft = 0;
        private int speedRight = 0;


        #region Region Constructor

        public MainPageViewModel(IBluetoothBleService bluetooth)
        {
            _bluetooth = bluetooth;

            Devices = new ObservableCollection<BleDeviceInfo>();

            // _bluetooth.ScanAsync();
            // _deviceInfoCollection = _bluetooth.Devices;

            // _bluetooth.ConnectAsync(_deviceInfoCollection[0].Id);



            int breakpoint23 = 1;
        }

        #endregion

        [RelayCommand]
        private async Task Play_No_1() { SendData("T:1:1"); }

        [RelayCommand]
        private async Task Play_No_2() { SendData("T:2:1"); }

        [RelayCommand]
        private async Task Play_No_4() { SendData("T:4:1"); }

        public async Task InitializeAsync()
        {
            await _bluetooth.ScanAsync();
            // _deviceInfoCollection = _bluetooth.Devices;
            Devices = _bluetooth.Devices;

            /*
            if (Devices.Any())
                await _bluetooth.ConnectAsync(Devices[0].Id);
            */
        }

        partial void OnSelectedDeviceChanged(BleDeviceInfo value)
        {
            if (value != null)
                ConnectCommand.Execute(value);
        }

        /*
        [RelayCommand]
        public async Task ConnectAsync(BleDeviceInfo device)
        {
            var status = await _bluetooth.ConnectAsync(device.Id);
            IsConnected = status == GattCommunicationStatus.Success;
        }
        */

        [RelayCommand]
        public async Task ConnectAsync(BleDeviceInfo device)
        {
            var status = await _bluetooth.ConnectAsync(device.Id);
            IsConnected = status == BleConnectionStatus.Success;
        }



        private async void SendData(string val)
        {
            await _bluetooth.WriteAsync(Devices[0].Id, Encoding.UTF8.GetBytes(val + "\r\n"));
        }

    }
}






/*

using System;
using System.Collections.Generic;
using System.Text;

namespace MauiBluetoothCerbotController.ViewModels
{
    internal class MainPageViewModel
    {
    }
}
*/