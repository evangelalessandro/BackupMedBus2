﻿using System;

namespace BackupMedBus
{
    public class LogEventArgs : EventArgs
    {
        public enum LogWarning
        {
            Info,
            Ok,
            Warning,
            Error,
        }

        public LogEventArgs(string text, LogWarning state)
        {
            Log = text;
            State = state;
        }

        public LogEventArgs(string text)
            : this(text, LogWarning.Ok)
        {
        }

        public LogWarning State { get; private set; }
        public string Log { get; private set; }
    }
}