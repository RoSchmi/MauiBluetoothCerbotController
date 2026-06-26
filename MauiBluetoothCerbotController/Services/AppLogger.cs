using System;
using System.Collections.Generic;
using System.Text;
using RoSchmi.BluetoothController.Interfaces;

namespace MauiBluetoothCerbotController.Services
{
    public class AppLogger : IAppLogger
    {
        public event Action<string>? MessageLogged;

        public void Log(string message)
        {
            MessageLogged?.Invoke(message);
        }
    }
}
