using RoSchmi.BluetoothController.Models;
using System.Collections.ObjectModel;


namespace RoSchmi.BluetoothController.Interfaces
{
    public interface IBluetoothBleService
    {
        ObservableCollection<BleDeviceInfo> Devices { get; }

        Task ScanAsync();
      
        Task<BleConnectionStatus> ConnectAsync(string deviceId);

        Task<bool> SubscribeToNotificationsAsync(string deviceId);


        Task WriteAsync(string deviceId, byte[] data);

        Task<bool> WriteAndConfirmAsync(string deviceId, byte[] payload, byte incrementingSequenceNumber);

    }
}