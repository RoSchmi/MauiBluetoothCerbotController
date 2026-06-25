#if WINDOWS

using RoSchmi.BluetoothController.Interfaces;
using RoSchmi.BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.WindowsRuntime;
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

        private GattCharacteristic? _rxCharacteristic;
        private GattCharacteristic? _txCharacteristic;

        private GattDeviceService? _customService;

        public event EventHandler<byte[]>? DataReceived;

        

        private void RxCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //Debug.WriteLine("### NOTIFY EVENT ###");
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] data = new byte[args.CharacteristicValue.Length];
            reader.ReadBytes(data);

            // Event nach außen geben
            DataReceived?.Invoke(this, data);
        }

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
            _device = await OpenDeviceAsync(deviceId);
            if (_device == null)
                return BleConnectionStatus.Unreachable;

            var servicesResult = await _device.GetGattServicesAsync();
            if (servicesResult.Status != GattCommunicationStatus.Success)
                return BleConnectionStatus.Failed;

            _customService = servicesResult.Services
                .FirstOrDefault(s => s.Uuid == Guid.Parse("2ac94b65-c8f4-48a4-804a-c03bc6960b80"));

            if (_customService == null)
                return BleConnectionStatus.ServiceNotFound;

            var charsResult = await _customService.GetCharacteristicsAsync();
            if (charsResult.Status != GattCommunicationStatus.Success)
                return BleConnectionStatus.Failed;

            _txCharacteristic = charsResult.Characteristics
                .FirstOrDefault(c => c.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write));

            _rxCharacteristic = charsResult.Characteristics
                .FirstOrDefault(c => c.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify));

            if (_txCharacteristic == null || _rxCharacteristic == null)
                return BleConnectionStatus.Failed;

            return BleConnectionStatus.Success;
        }

        /*
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

        */

        private async Task<BluetoothLEDevice?> OpenDeviceAsync(string deviceId)
        {
            try
            {
                // Fall 1: echte Windows-DeviceId
                if (IsWindowsDeviceId(deviceId))
                {
                    return await BluetoothLEDevice.FromIdAsync(deviceId);
                }

                // Fall 2: MAC-Adresse oder numerische Adresse
                ulong address = ParseBluetoothAddress(deviceId);
                return await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SubscribeToNotificationsAsync(string deviceId)
        {
            if (_rxCharacteristic == null)
                return false;

            _rxCharacteristic.ValueChanged += RxCharacteristic_ValueChanged;

            var status = await _rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify);

            Debug.WriteLine("Notify-Status: " + status);
           // _logger.Log($"Notify-Status:  {status}...");
            return status == GattCommunicationStatus.Success;
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

        public async Task<bool> WriteAndConfirmAsync(byte[] payload, byte messageCounter)
        {
            var message = payload
                .Concat(new byte[] { messageCounter, 0x0D, 0x0A })
                .ToArray();

            await _txCharacteristic.WriteValueAsync(message.AsBuffer());

            await Task.Delay(50);

            var result = await _txCharacteristic.ReadValueAsync();
            if (result.Status != GattCommunicationStatus.Success)
                return false;

            var reader = DataReader.FromBuffer(result.Value);
            byte[] readBack = new byte[result.Value.Length];
            reader.ReadBytes(readBack);

            return message.SequenceEqual(readBack);
        }

        public async Task WriteAsync(string deviceId, byte[] data)
        {
            if (_txCharacteristic == null)
                throw new Exception("TX characteristic not initialized");

            await _txCharacteristic.WriteValueAsync(data.AsBuffer());

            Thread.Sleep(10);

            // Reading back via the _txCharacteristic
            // Retrieves the same value, that was sent
            var result = await _txCharacteristic.ReadValueAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var reader = DataReader.FromBuffer(result.Value);
                byte[] backData = new byte[result.Value.Length];
                reader.ReadBytes(backData);

                Debug.WriteLine("RX READ: " + Encoding.UTF8.GetString(backData));
                //  _logger.Log($"RX READ: {Encoding.UTF8.GetString(backData)}");

            }
        }

        /*

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

        */

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


