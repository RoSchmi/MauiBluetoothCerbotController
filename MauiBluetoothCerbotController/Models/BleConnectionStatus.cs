using System;
using System.Collections.Generic;
using System.Text;

namespace RoSchmi.BluetoothController.Models
{
    public enum BleConnectionStatus
    {
        Success,            // Verbindung erfolgreich
        Unreachable,        // Gerät nicht erreichbar (z.B. ausgeschaltet, außer Reichweite)
        Failed,             // Allgemeiner Fehler (z.B. GATT-Fehler)
        NotSupported,       // Gerät oder Plattform unterstützt benötigte Funktion nicht
        ServiceNotFound,    // Custom-Service nicht gefunden
        CharacteristicNotFound, // TX oder RX nicht gefunden
        AccessDenied,       // Windows blockiert Zugriff (z.B. mehrfaches GetGattServicesAsync)
        Unknown             // Fallback für unerwartete Fälle 
    }
}
