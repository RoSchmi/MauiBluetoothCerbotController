using System.Collections.ObjectModel;
using RoSchmi.BluetoothController.Models;

namespace RoSchmi.BluetoothController.Interfaces
{
    public interface IBluetoothBleService
    {
        ObservableCollection<BleDeviceInfo> Devices { get; }

        Task ScanAsync();
        Task ConnectAsync(string deviceId);


        Task WriteAsync(string deviceId, byte[] data);

    }
}




/*
using System;
using System.Collections.Generic;
using System.Text;

namespace MauiBluetoothCerbotController.Interfaces
{
    internal class IBluetoothBleService
    {
    }
}
*/