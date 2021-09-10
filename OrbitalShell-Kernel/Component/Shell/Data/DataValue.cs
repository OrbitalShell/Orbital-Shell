using System;
using System.Collections.Generic;
using System.Linq;

using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Lib;

namespace OrbitalShell.Component.Shell.Data
{
    public sealed class DataValueReadOnlyException : Exception
    {
        public DataValueReadOnlyException(IDataObject dataObject) : base(
            $"DataValue name='{dataObject}' is read only"
            )
        { }
    }

    public sealed class DataValue<T>
    {

    }

    public sealed class DataValue : IDataObject
    {
        public string Name { get; private set; }
        public DataObject Parent { get; set; }

        object _value;
        public object Value
        {
            get { return _value; }
            private set { _value = value; HasValue = true; }
        }
        public Type ValueType { get; private set; }

        public bool HasValue { get; private set; }

        public bool IsReadOnly { get; private set; }
        public bool HasAttributes => false;

        public string ObjectPath
        {
            get
            {
                if (Parent == null)
                    return "";
                else
                {
                    var r = Parent.ObjectPath;
                    if (!string.IsNullOrWhiteSpace(r))
                        r += CommandLineSyntax.VariableNamePathSeparator;
                    r += Parent.Name;
                    return r;
                }
            }
        }

        public DataValue(
            string name,
            object value,
            Type valueType = null,
            bool isReadOnly = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ValueType = valueType ?? value?.GetType();
            ValueType = ValueType ?? throw new ArgumentNullException(nameof(valueType));
            IsReadOnly = isReadOnly;
            /*Value = value;
            HasValue = true;*/
            SetValue(value);
        }

        public DataValue(
            string name,
            object value,
            bool isReadOnly)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ValueType = value?.GetType();
            IsReadOnly = isReadOnly;
            Value = value;
            HasValue = true;
        }

        public DataValue(
            string name,
            bool isReadOnly)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsReadOnly = isReadOnly;
        }

        public List<IDataObject> GetAttributes()
        {
            return new List<IDataObject>() { };
        }

        public bool Get(ArraySegment<string> path, out object data)
            => Get(Value, path, out data);

        bool Get(object target, ArraySegment<string> path, out object data)
        {
            data = null;
            if (target == null) return false;
            if (path.Count == 0) return false;
            var attrname = path[0];
            /*var membersInfos = target.GetType().GetFields().ToDictionary((x) => x.Name);
            if (membersInfos.TryGetValue(attrname, out var memberInfo))
            {
                if (path.Count == 1)
                {
                    data = memberInfo.GetValue(target);
                    return true;
                }
                return Get(target, path.Slice(1),out data);
            }*/
            var membersInfos = target.GetFieldsAndProperties();
            var memberInfo = membersInfos.FirstOrDefault(x => x.Name == attrname);
            if (memberInfo == null) return false;
            if (path.Count == 1)
            {
                data = memberInfo.GetMemberValue(target);
                return true;
            }
            target = memberInfo.GetMemberValue(target);
            return Get(target, path.Slice(1), out data);
        }

        public bool GetPathOwner(ArraySegment<string> path, out object data)
            => GetPathOwner(Value, path, out data);

        bool GetPathOwner(object target, ArraySegment<string> path, out object data)
        {
            data = null;
            if (path.Count == 0) return false;
            var attrname = path[0];
            var fieldsInfos = target.GetType().GetFields().ToDictionary((x) => x.Name);
            if (fieldsInfos.TryGetValue(attrname, out var fieldInfo))
            {
                if (path.Count == 1)
                {
                    data = fieldInfo.GetValue(target);
                    return true;
                }
                return GetPathOwner(target, path.Slice(1), out data);
            }
            return false;
        }

        public bool Has(ArraySegment<string> path, out object data)
            => Has(Value, path, out data);

        bool Has(object target, ArraySegment<string> path, out object data)
            => GetPathOwner(target, path, out data);

        /// <summary>
        /// set - isReadOnly and type are currently ignored (no usage)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="isReadOnly">not implemented</param>
        /// <param name="type">not implemented</param>
        /// <returns></returns>
        public IDataObject Set(ArraySegment<string> path, object value, bool isReadOnly = false, Type type = null)
            => Set(this, path, value, isReadOnly, type);


        /// <summary>
        /// set - isReadOnly and type are currently ignored (no usage)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="isReadOnly">not implemented</param>
        /// <param name="type">not implemeted</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Supprimer le paramètre inutilisé", Justification = "<En attente>")]
        IDataObject Set(object target, ArraySegment<string> path, object value, bool isReadOnly = false, Type type = null)
        {
            IDataObject r = Parent;
            if (IsReadOnly) throw new DataObjectReadOnlyException(this);
            if (target == null) return r;
            if (path.Count == 0) return r;
            var attrname = path[0];
            var fieldsInfos = target.GetType().GetFields().ToDictionary((x) => x.Name);
            if (fieldsInfos.TryGetValue(attrname, out var fieldInfo))
            {
                if (path.Count == 1)
                {
                    fieldInfo.SetValue(target, value);
                }
                else
                    Set(target, path.Slice(1), value);
            }
            else
                throw new DataValueReadOnlyException(this);
            return r;
        }

        public void Unset(ArraySegment<string> path)
            => Unset(this, path);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Supprimer le paramètre inutilisé", Justification = "<En attente>")]
        void Unset(object target, ArraySegment<string> path)
        {
            throw new DataValueReadOnlyException(this);
        }

        public void SetValue(object value, Type type = null)      // TODO return DataValue here , and propagates returns to callers and interfaces
        {
            if (IsReadOnly) throw new Exception($"{ValueId} is readonly");
            Type reverseTypeIfFail = ValueType;

            if (type != null)
            {
                // try trans-type
                ValueType = type;
            }

            if (value != null)
            {
                if (ValueType != null)
                {
                    var objType = value.GetType();
                    if (!objType.IsOrInheritsFrom(ValueType))
                    {
                        AffectValueFromString(value, reverseTypeIfFail);
                        return;
                    }
                }
            }
            this.Value = value;
            this.HasValue = true;
        }

        private void AffectValueFromString(object value, Type reverseTypeIfFail = null)
        {
            if (ValueTextParser.ToTypedValue(value, ValueType, null, out var v, out _, ValueType))
            {
                Value = v;
                return;
            }
            else
            {
                if (reverseTypeIfFail != null) ValueType = reverseTypeIfFail;
                throw new Exception($"value: '{value}' can't be converted to type: {ValueType.FullName} from type {value.GetType().FullName}");
            }
        }

        /// <summary>
        ///  set a typed variable from a string value<br/>
        ///  don't set the value if conversion has failed - exception
        /// </summary>
        /// <param name="value">a string value that must be converted to var type an assigned to the var</param>
        public void SetValue(string value)
        {
            if (ValueTextParser.ToTypedValue(value, ValueType, null, out var v, out _))
                SetValue((object)v);
        }

        string ValueId => $"DataValue '{Name}'";
    }
}
