using System;
using System.ComponentModel;

namespace NSUWatcher.Interfaces
{
    public interface INsuSystem
    {
        event EventHandler<PropertyChangedEventArgs> StatusChanged;
    }
/*    
    public enum ChangedProperty
    { 
        Status,
        Temperature,
        MCUStatus
    }

    public class PropertyChangedEventArgs 
    {
        public INSUSysPartDataContract Source { get; }
        public ChangedProperty Property { get; }

        public PropertyChangedEventArgs(INSUSysPartDataContract source, ChangedProperty property)
        {
            Source = source;
            Property = property;
        }
    }
*/
}
