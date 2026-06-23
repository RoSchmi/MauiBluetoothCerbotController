#if WINDOWS

using RoSchmi.BluetoothController.Interfaces;
using RoSchmi.BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Custom;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace RoSchmi.BluetoothController.Services
{
    public class WindowsBluetoothBleService : IBluetoothBleService
    {
        public ObservableCollection<BleDeviceInfo> Devices { get; } = new();

        private BluetoothLEDevice? _device = null;

       public WindowsBluetoothBleService() { }

       
        public async Task ScanAsync()
        {
            Devices.Clear();

            // Geräte anzeigen, die BLE sprechen
            // Funtioniert erst, nachdem das Gerät über das Bluetooth Icon in der Systemtray als Bluetooth Gerät hinzugefügt wurde.
            var selector = BluetoothLEDevice.GetDeviceSelector();
            var result = await DeviceInformation.FindAllAsync(selector);
        
            foreach (var device in result)
                Devices.Add(new BleDeviceInfo
                { Id = device.Id,
                    Name = device.Name 
                });
        }

        public async Task ConnectAsync(string deviceId)
        {
            
            // 1. Open device using its DeviceId
       
            _device = await BluetoothLEDevice.FromIdAsync(deviceId);

            if (_device == null)
                throw new Exception("Bluetooth-Device could not be opened.");

            // 2. GATT-Services available ?
            var result = await _device.GetGattServicesAsync();

            if (result.Status != GattCommunicationStatus.Success)
                throw new Exception($"GATT-Services konnten nicht gelesen werden: {result.Status}");

            // Optional: Printout Services (Debug)
            foreach (var service in result.Services)
            {
                System.Diagnostics.Debug.WriteLine($"Service: {service.Uuid}");
            }
            
        }

        
        public async Task WriteAsync(string deviceId, byte[] data)    
        {
            var device = await BluetoothLEDevice.FromIdAsync(deviceId);
            var servicesResult = await device.GetGattServicesAsync();
            

            var customService = servicesResult.Services.First(s => s.Uuid == Guid.Parse("2ac94b65-c8f4-48a4-804a-c03bc6960b80"));

            var charsResult = await customService.GetCharacteristicsAsync();

            var writeCharacteristic = charsResult.Characteristics.FirstOrDefault(ch =>
            ch.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write) ||
            ch.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse)
            );

            var writer = new DataWriter();
            writer.WriteBytes(data);

            try
            {
                await writeCharacteristic.WriteValueAsync(writer.DetachBuffer());
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                int breakpoint93 = 1;
            }
        }

    }
}
#endif




/*

using System;
using System.Collections.Generic;
using System.Text;

namespace MauiBluetoothCerbotController.Services
{
    internal class WindowsBluetoothBleService
    {
    }
}
*/
