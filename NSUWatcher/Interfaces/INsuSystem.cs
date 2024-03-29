﻿using NSU.Shared.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NSUWatcher.Interfaces
{
#nullable enable
    public interface INsuSystem
    {
        event EventHandler<SystemStatusChangedEventArgs> SystemStatusChanged;
        event EventHandler<PropertyChangedEventArgs> EntityStatusChanged;
        NsuSystemStatus CurrentStatus { get; }
        string GetSnapshot(SnapshotType snapshotType);
        IEnumerable<T>? GetEntityData<T>() where T : INSUSysPartDataContract;
    }

    public enum SnapshotType
    {
        Xml,
        Json
    }

    public enum NsuSystemStatus
    {
        NotReady,
        Ready
    }

    public class SystemStatusChangedEventArgs
    {
        public NsuSystemStatus State { get; }

        public SystemStatusChangedEventArgs(NsuSystemStatus state)
        {
            State = state;
        }
    }
#nullable disable    
}
