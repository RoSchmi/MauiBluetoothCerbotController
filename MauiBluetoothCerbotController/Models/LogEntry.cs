using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RoSchmi.BluetoothController.Models
{
    public partial class LogEntry : ObservableObject
    {
        /*
        public LogEntry(string text )
        {
            message = text;
        }
        */

        [ObservableProperty]
        private string message;
    }
}
