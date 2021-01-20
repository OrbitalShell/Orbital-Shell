using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using static DotNetConsoleAppToolkit.Component.CommandLine.Variable.VariableSyntax;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Variable
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
        public Variables( (VariableNamespace ns,DataObject o)? providedNS = null ) {
            // standard namespaces
            foreach (var ns in Enum.GetValues(typeof(VariableNamespace)))
            {
                if (providedNS.HasValue && providedNS.Value.ns==(VariableNamespace)ns)
                    _dataRegistry.Set(ns + "", providedNS.Value.o );
                else
                    _dataRegistry.Set(ns + "", new DataObject(ns+"",false));
            }

            // os Env vars
            var pfx = Nsp(VariableNamespace.env,ShellEnvironmentNamespace.os+"");
            foreach (DictionaryEntry envvar in Environment.GetEnvironmentVariables())
                _dataRegistry.Set(Nsp(pfx,envvar.Key+""), envvar.Value);
        }

        #region setters

        public void Set( string path, object value) => _dataRegistry.Set( path, value);
        public void Set( string rootPath, string path, object value) => _dataRegistry.Set( Nsp(rootPath,path), value);
        public void Set( VariableNamespace rootPath, string path, object value) => _dataRegistry.Set(Nsp(rootPath, path), value);

        public void Unset(string path) => _dataRegistry.Unset(path);
        public void Unset(string rootPath,string path) => _dataRegistry.Unset(Nsp(rootPath,path));
        public void Unset( VariableNamespace rootPath, params string[] path) => _dataRegistry.Unset(Nsp(rootPath,path));

        #endregion

        #region value setters

        public DataValue SetValue(string path,string value) 
        {        
            var o = GetValue(path);
            return o;
        }

        public DataValue SetValue(string rootPath,string path,string value) { 
            var o = GetValue(Nsp(rootPath,path));
            return o;
        }

        public DataValue SetValue(VariableNamespace rootPath,string path) {
            var o = GetValue(Nsp(rootPath,path));
            return o;
        }

        #endregion

        #region getters

        /// <summary>
        /// search in variables the path according to these precedence rules:
        /// - absolute path
        /// - path related to Local
        /// - path related to Env
        /// - path related to Global
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Get( string path, out object value, bool throwException=true )
        {
            var r = _dataRegistry.Get(path, out value)
            || _dataRegistry.Get(Nsp(VariableNamespace.local, path), out value)
            || _dataRegistry.Get(Nsp(VariableNamespace.global, path), out value)
            || _dataRegistry.Get(Nsp(VariableNamespace.env, path), out value);
            if (!r && throwException)
                throw new VariablePathNotFoundException(path);            
            return r;
        }

        public bool Get( string rootPath, string path, out object value, bool throwException = true )
            => Get(Nsp(rootPath, path), out value, throwException);

        public bool Get(VariableNamespace rootPath, string path, out object value, bool throwException = true)
            => Get(Nsp(rootPath, path), out value, throwException);

        public T GetValue<T>(string path) => (T)GetValue(path).Value;
        public T GetValue<T>(string rootPath,string path) => (T)GetValue(Nsp(rootPath,path)).Value;
        public T GetValue<T>(VariableNamespace rootPath,string path) => (T)GetValue(Nsp(rootPath,path)).Value;

        public bool GetDataObject(string path, out IDataObject value, bool throwException = true)
        {
            if (Get(path,out var obj,throwException))
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

        public DataValue GetValue(string path,bool throwException=true)
        {
            if (Get(path, out var data, false))
                return (DataValue)data;
            if (throwException)
                    throw new VariablePathNotFoundException(path);
            return null;
        }
        public DataValue GetValue(string rootPath, string path, bool throwException = true)
            => GetValue(Nsp(rootPath, path), throwException);
        public DataValue GetValue(VariableNamespace rootPath, string path, bool throwException = true)
            => GetValue(Nsp(rootPath, path), throwException);

        public bool GetValue(string path,out DataValue value,bool throwException=true)
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

        public bool GetPathOwner(string path,out object data)
            => _dataRegistry.GetPathOwner(path,out data);

        public static string Nsp( string @namespace, string key) => @namespace + CommandLineSyntax.VariableNamePathSeparator + key;
        public static string Nsp( params string[] key) => string.Join( CommandLineSyntax.VariableNamePathSeparator , key);
        public static string Nsp( VariableNamespace @namespace, params string[] key) => @namespace + ((key.Length==0?"": (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join( CommandLineSyntax.VariableNamePathSeparator, key)));
    }
}
