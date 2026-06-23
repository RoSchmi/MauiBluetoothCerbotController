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

        public async Task<BleConnectionStatus> ConnectAsync(string deviceId)
        {
            try
            {
                BluetoothLEDevice device;

                if (IsWindowsDeviceId(deviceId))
                {
                    // ⭐ Fall 1: Echte Windows-DeviceId
                    device = await BluetoothLEDevice.FromIdAsync(deviceId);
                }
                else
                {
                    // ⭐ Fall 2: MAC-Adresse → in ulong umwandeln
                    ulong address = ParseBluetoothAddress(deviceId);
                    device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                }

                if (device == null)
                    return BleConnectionStatus.Unreachable;

                var result = await device.GetGattServicesAsync();
                return MapStatus(result.Status);
            }
            catch
            {
                return BleConnectionStatus.Failed;
            }
        }

        private bool IsWindowsDeviceId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // typische Windows-IDs enthalten '#'
            if (id.Contains("#"))
                return true;

            // typische Windows-Pfade beginnen mit \\?\
            if (id.StartsWith(@"\\?\"))
                return true;

            return false;
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

        private ulong ParseBluetoothAddress(string deviceId)
        {
            // Fall 1: Windows liefert "BluetoothLE#BluetoothLEXX:XX:XX:XX:XX:XX-..."
            if (deviceId.Contains("BluetoothLE"))
            {
                // MAC extrahieren
                var mac = deviceId.Split('#')
                                  .Last()
                                  .Split('-')
                                  .First()
                                  .Replace(":", "");

                return Convert.ToUInt64(mac, 16);
            }

            // Fall 2: MAC-Adresse "AA:BB:CC:DD:EE:FF"
            if (deviceId.Contains(":"))
            {
                var mac = deviceId.Replace(":", "");
                return Convert.ToUInt64(mac, 16);
            }

            // Fall 3: Reine Zahl
            if (ulong.TryParse(deviceId, out ulong result))
                return result;

            throw new FormatException($"Ungültige DeviceId: {deviceId}");
        }

        private BleConnectionStatus MapStatus(GattCommunicationStatus status)
        {
            return status switch
            {
                GattCommunicationStatus.Success => BleConnectionStatus.Success,
                GattCommunicationStatus.Unreachable => BleConnectionStatus.Unreachable,
                GattCommunicationStatus.ProtocolError => BleConnectionStatus.Failed,
                GattCommunicationStatus.AccessDenied => BleConnectionStatus.NotSupported,
                _ => BleConnectionStatus.Unknown
            };
        }


    }
}
#endif


