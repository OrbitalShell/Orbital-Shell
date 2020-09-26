using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Data
{
    public sealed class DataObjectReadOnlyException : Exception
    {
        public DataObjectReadOnlyException(IDataObject dataObject) : base(
            $"{dataObject?.GetType().Name} name='{dataObject.Name}' is read only"
            ) { }
    }

    public class DataObject : IDataObject
    {
        public string Name { get; private set; }

        public DataObject Parent { get; set; }

        private readonly Dictionary<string, IDataObject> _attributes
            = new Dictionary<string, IDataObject>();

        public bool IsReadOnly { get; private set; }

        public bool HasAttributes => _attributes.Count > 0;

        public DataObject(string name, bool isReadOnly = false)
        {
            Name = name;
            IsReadOnly = isReadOnly;
        }

        public List<IDataObject> GetAttributes()
        {
            var r = new List<IDataObject>();
            foreach ( var attrkv in _attributes )
            {
                /*if (attrkv.Value is DataValue dataValue)
                    r.Add(dataValue);
                else
                    r.AddRange(attrkv.Value.GetDataValues());*/
                r.Add(attrkv.Value);
            }
            return r;
        }

        public IDataObject Set(ArraySegment<string> path, object value)
        {
            IDataObject r = null;
            if (IsReadOnly) throw new DataObjectReadOnlyException(this);
            if (path.Count == 0) return r;
            var attrname = path[0];
            if (_attributes.TryGetValue(attrname, out var attr))
            {
                if (path.Count == 1)
                {
                    r = (value is IDataObject) ? (IDataObject)value : new DataValue(attrname, value);
                    r.Parent = this;
                    _attributes[attrname] = r;
                }
                else
                    r = attr.Set(path.Slice(1), value);
            }
            else
            {
                if (path.Count == 1)
                {
                    r = (value is IDataObject) ? (IDataObject)value : new DataValue(attrname, value);
                    r.Parent = this;
                    _attributes.Add(attrname, r);
                }
                else
                {
                    var node = new DataObject(attrname);
                    _attributes[attrname] = node;
                    node.Parent = this;
                    r = node.Set(path.Slice(1), value);
                }
            }
            return r;
        }

        public void Unset(ArraySegment<string> path)
        {
            if (IsReadOnly) throw new DataObjectReadOnlyException(this);
            if (path.Count == 0) return;
            var attrname = path[0];
            if (_attributes.TryGetValue(attrname, out var attr))
            {
                if (path.Count == 1)
                {
                    _attributes.Remove(attrname);
                }
                else
                    attr.Unset(path.Slice(1));
            }
        }

        public bool Get(ArraySegment<string> path,out object data)
        {
            data = null;
            if (path.Count == 0) return false;
            var attrname = path[0];
            if (_attributes.TryGetValue(attrname, out var attr))
            {
                if (path.Count == 1) return true;
                if (attr.Get(path.Slice(1), out var sdata))
                {
                    data = sdata;
                    return true;
                }
            }
            return false;
        }

        public bool Has(ArraySegment<string> path,out object data)
            => GetPathOwner(path,out data);

        public bool GetPathOwner(ArraySegment<string> path,out object data)
        {
            data = null;
            if (path.Count == 0) return false;
            var attrname = path[0];
            if (_attributes.ContainsKey(attrname))
            {
                if (path.Count == 1)
                {
                    data = this;
                    return true;
                }
                return GetPathOwner(path.Slice(1),out data);
            }
            return false;
        }
    }

}
