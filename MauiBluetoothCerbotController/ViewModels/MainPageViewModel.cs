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
using System.Diagnostics;



namespace MauiBluetoothCerbotController.ViewModels
{


    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<LogEntry> logLines = new();

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


        public void AddLog(string line)
        {
            if (LogLines.Count >= 100)
                LogLines.RemoveAt(0); // älteste Zeile löschen
            LogLines.Add(new LogEntry() { Message = line });
           
        }

        [RelayCommand]
        private async Task ZwickMe() 
        { 
            SendData("T:1:1"); AddLog("Clicked ZwickMe!");  
        }
        


        [RelayCommand]
        private async Task Play_No_1() { SendData("T:1:1"); AddLog("Played short tone"); }
        public string Text_No_1 { get; } = "Play short tone" ;

        [RelayCommand]
        private async Task Play_No_2() { SendData("T:2:1"); AddLog("Played Tune No. 1"); }
        public string Text_No_2 { get; } = "Play Tune No. 1";

        [RelayCommand]
        private async Task Play_No_3() { SendData("T:3:1"); AddLog("Played Tune No. 2"); }
        public string Text_No_3 { get; } = "Play Tune No. 2";


        [RelayCommand]
        private async Task Play_No_4() { SendData("T:4:1"); AddLog("Played Tune No. 3"); }
        public string Text_No_4 { get; } = "Play Tune No. 3";

        /*
        [RelayCommand]
        private async Task Play_No_5() { SendData("T:5:1"); }
        public string Text_No_5 { get; } = "Play Tune No. 4";
        */

        [RelayCommand]
        private async Task MoveForward()
        {
            if (speedLeft < 0) speedLeft = 0;
            if (speedRight < 0) speedRight = 0;
            speedLeft += 30;
            speedRight += 30;
            SendData("F:" + speedLeft + ":" + speedRight);
            AddLog("Increase forward speed");
        }

        [RelayCommand]
        private async Task TurnLeft()
        {
            speedLeft += 20;
            speedRight -= 20;
            SendData("F:" + speedLeft + ":" + speedRight);
            AddLog("Turn left");
        }

        [RelayCommand]
        private async Task Stop()
        {
            speedLeft = 0;
            speedRight = 0;
            SendData("F:" + speedLeft + ":" + speedRight);
            AddLog("Stop");

        }

        [RelayCommand]
        private async Task TurnRight()
        {
            speedLeft -= 20;
            speedRight += 20;
            SendData("F:" + speedLeft + ":" + speedRight);
            AddLog("Turn right");
        }

        [RelayCommand]
        private async Task MoveBackward()
        {
            if (speedLeft > 0) speedLeft = 0;
            if (speedRight > 0) speedRight = 0;
            speedLeft -= 30;
            speedRight -= 30;
            SendData("F:" + speedLeft + ":" + speedRight);
            AddLog("Increase backward speed");
        }

        public async Task InitializeAsync()
        {
            await _bluetooth.ScanAsync();
            
            Devices = _bluetooth.Devices;        
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
            if (IsConnected)
            {
                AddLog("Successfully connected");
            }
            else
            {
                AddLog("Failed to connected");
            }
            
        }



        private async void SendData(string val)
        {
            if (IsConnected)
            {
                await _bluetooth.WriteAsync(Devices[0].Id, Encoding.UTF8.GetBytes(val + "\r\n"));
            }
            else
            {
                Debug.WriteLine("Sent command but not connectet");
                await Shell.Current.DisplayAlertAsync ("Connection Status", "Not connected to Ble-Device", "OK");
            }
        }

    }
}