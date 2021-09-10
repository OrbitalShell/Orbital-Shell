using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.Shell.Data;

namespace OrbitalShell.Component.Shell.Variable
{
    /// <summary>
    /// variables data store
    /// </summary>
    public class Variables
    {
        public sealed class WrongVariableTypeException : Exception
        {
            public WrongVariableTypeException(Type attempted, Type getted)
                : base($"variable type not as expected ({attempted?.Name}): {getted?.Name}")
            { }
        }

        public sealed class VariablePathNotFoundException : Exception
        {
            public VariablePathNotFoundException(string variablePath)
                : base($"variable path not found: '{variablePath}'")
            { }
        }

        protected readonly DataRegistry _dataRegistry = new DataRegistry();

        public DataObject RootObject => _dataRegistry.RootObject;

        /// <summary>
        /// creates a standard variable rush with known namespaces
        /// </summary>
        public Variables((VariableNamespace ns, DataObject o)? providedNS = null)
        {
            // standard namespaces
            foreach (var ns in Enum.GetValues(typeof(VariableNamespace)))
            {
                if (providedNS.HasValue && providedNS.Value.ns == (VariableNamespace)ns)
                    _dataRegistry.Set(ns + "", providedNS.Value.o);
                else
                    _dataRegistry.Set(ns + "", new DataObject(ns + "", false));
            }

            // os Env vars
            var pfx = Nsp(VariableNamespace.env, ShellEnvironmentNamespace.os + "");
            foreach (DictionaryEntry envvar in Environment.GetEnvironmentVariables())
                _dataRegistry.Set(Nsp(pfx, envvar.Key + ""), envvar.Value);
        }

        #region setters

        public Variables Set(string path, object value, bool isReadOnly = false, Type type = null) { _dataRegistry.Set(path, value, isReadOnly, type); return this; }
        public Variables Set(string rootPath, string path, object value, bool isReadOnly = false, Type type = null) { _dataRegistry.Set(Nsp(rootPath, path), value, isReadOnly, type); return this; }
        public Variables Set(VariableNamespace rootPath, string path, object value, bool isReadOnly = false, Type type = null) { _dataRegistry.Set(Nsp(rootPath, path), value, isReadOnly, type); return this; }

        public Variables Unset(string path) { _dataRegistry.Unset(path); return this; }
        public Variables Unset(string rootPath, string path) { _dataRegistry.Unset(Nsp(rootPath, path)); return this; }
        public Variables Unset(VariableNamespace rootPath, params string[] path) { _dataRegistry.Unset(Nsp(rootPath, path)); return this; }

        #endregion

        #region value setters

        public DataValue AddValue(string var, object value, bool readOnly = false)
        {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly);
            Set(path, val);
            return val;
        }

        public DataValue SetValue(string path, object value)
        {
            //var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            if (!HasValue(path))
                return AddValue(path, value);
            var o = GetValue(path);
            o.SetValue(value);
            return o;
        }

        public DataValue SetValue(string rootPath, string path, object value)
        {
            path = Nsp(rootPath, path);
            //var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            if (!HasValue(path))
                return AddValue(path, value);
            var o = GetValue(path);
            o.SetValue(value);
            return o;
        }

        public DataValue SetValue(VariableNamespace rootPath, string path, object value)
        {
            path = Nsp(rootPath, path);
            //var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            if (!HasValue(path))
                return AddValue(path, value);
            var o = GetValue(path);
            o.SetValue(value);
            return o;
        }

        #endregion

        #region getters

        public bool HasValue(string path) => GetObject(Nsp(path), out var o, false) && o is DataValue;

        /// <summary>
        /// search in variables the path according to these precedence rules:
        /// - absolute path
        /// - path related to _
        /// - path related to Local
        /// - path related to Env
        /// - path related to Global
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Get(string path, out object value, bool throwException = true)
        {
            var r = _dataRegistry.Get(path, out value)
            || _dataRegistry.Get(Nsp(VariableNamespace.local, path), out value)
            || _dataRegistry.Get(Nsp(VariableNamespace._, path), out value)
            || _dataRegistry.Get(Nsp(VariableNamespace.global, path), out value)
            || _dataRegistry.Get(Nsp(VariableNamespace.env, path), out value);

            if (!r && throwException)
                throw new VariablePathNotFoundException(path);

            return r;
        }

        public bool Get(string rootPath, string path, out object value, bool throwException = true)
            => Get(Nsp(rootPath, path), out value, throwException);

        public bool Get(VariableNamespace rootPath, string path, out object value, bool throwException = true)
            => Get(Nsp(rootPath, path), out value, throwException);

        public T GetValue<T>(string path, bool throwException = true) => (T)GetValue(path, throwException).Value;
        public T GetValue<T>(string rootPath, string path, bool throwException = true) => (T)GetValue(Nsp(rootPath, path), throwException).Value;
        public T GetValue<T>(VariableNamespace rootPath, string path, bool throwException = true) => (T)GetValue(Nsp(rootPath, path), throwException).Value;

        public bool GetDataObject(string path, out IDataObject value, bool throwException = true)
        {
            if (Get(path, out var obj, throwException))
            {
                value = (IDataObject)obj;
                return true;
            }
            value = null;
            return false;
        }
        public bool GetDataObject(string rootPath, string path, out IDataObject value, bool throwException = true)
            => GetDataObject(Nsp(rootPath, path), out value, throwException);
        public bool GetDataObject(VariableNamespace rootPath, string path, out IDataObject value, bool throwException = true)
            => GetDataObject(Nsp(rootPath, path), out value, throwException);

        public bool GetObject(string path, out object value, bool throwException = true)
        {
            if (Get(path, out var obj, throwException))
            {
                value = obj;
                return true;
            }
            value = null;
            return false;
        }

        public bool GetObject(string rootPath, string path, out object value, bool throwException = true)
            => GetObject(Nsp(rootPath, path), out value, throwException);
        public bool GetObject(VariableNamespace rootPath, string path, out object value, bool throwException = true)
            => GetObject(Nsp(rootPath, path), out value, throwException);

        public DataValue GetValue(string path, bool throwException = true)
        {
            if (Get(path, out var data, false))
                return (DataValue)data;
            if (throwException)
                throw new VariablePathNotFoundException(path);
            // a real null DataValue
            return new DataValue(path, null, false);
        }
        public DataValue GetValue(string rootPath, string path, bool throwException = true)
            => GetValue(Nsp(rootPath, path), throwException);
        public DataValue GetValue(VariableNamespace rootPath, string path, bool throwException = true)
            => GetValue(Nsp(rootPath, path), throwException);

        public bool GetValue(string path, out DataValue value, bool throwException = true)
        {
            if (Get(path, out var data, false))
            {
                if (data is DataValue dv)
                {
                    value = dv;
                    return true;
                }
                else
                    throw new WrongVariableTypeException(typeof(DataValue), data?.GetType());
            }
            if (throwException)
                throw new VariablePathNotFoundException(path);
            value = null;
            return false;
        }

        public bool GetValue(string rootPath, string path, out DataValue value, bool throwException = true)
            => GetValue(Nsp(rootPath, path), out value, throwException);
        public bool GetValue(VariableNamespace rootPath, string path, out DataValue value, bool throwException = true)
            => GetValue(Nsp(rootPath, path), out value, throwException);

        public List<IDataObject> GetDataValues() => _dataRegistry.GetDataValues();

        #endregion

        public bool GetPathOwner(string path, out object data)
            => _dataRegistry.GetPathOwner(path, out data);

        public static string Nsp(string @namespace, string key) => @namespace + CommandLineSyntax.VariableNamePathSeparator + key;
        public static string Nsp(params string[] key) => string.Join(CommandLineSyntax.VariableNamePathSeparator, key);
        public static string Nsp(VariableNamespace @namespace, params string[] key) => @namespace + ((key.Length == 0 ? "" : (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));
    }
}
