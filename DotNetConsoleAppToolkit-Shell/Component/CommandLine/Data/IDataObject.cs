using System;
using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Data
{
    public interface IDataObject
    {
        string Name { get; }
        DataObject Parent { get; set; }
        bool IsReadOnly { get; }
        bool HasAttributes { get; }

        List<IDataObject> GetAttributes();
        bool Get(ArraySegment<string> path,out object data );
        bool GetPathOwner(ArraySegment<string> path,out object data);
        bool Has(ArraySegment<string> path,out object data);
        IDataObject Set(ArraySegment<string> path, object value);
        void Unset(ArraySegment<string> path);
    }
}