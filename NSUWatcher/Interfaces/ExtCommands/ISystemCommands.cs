﻿using NSU.Shared.DTO.ExtCommandContent;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ISystemCommands
    {
        IExternalCommand ResetMcu(ResetType resetType);
    }
}
