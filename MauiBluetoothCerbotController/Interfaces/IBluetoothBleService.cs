using RoSchmi.BluetoothController.Models;
using System.Collections.ObjectModel;


namespace RoSchmi.BluetoothController.Interfaces
{
    public interface IBluetoothBleService
    {
        ObservableCollection<BleDeviceInfo> Devices { get; }

        Task ScanAsync();
       // Task ConnectAsync(string deviceId);

        //Task<GattCommunicationStatus> ConnectAsync(string deviceId);
        Task<BleConnectionStatus> ConnectAsync(string deviceId);
        

        Task WriteAsync(string deviceId, byte[] data);

    }
}