using System;
using System.Collections.Generic;
using System.Text;

namespace RoSchmi.BluetoothController.Interfaces
{
    public interface IAppLogger
    {
        public event Action<string> MessageLogged;
        public void Log(string message);
    }
}
