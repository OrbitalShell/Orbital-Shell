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
        public sealed class VariableNotFoundException : Exception
        {
            public VariableNotFoundException(string variableName)
                : base($"variable not found: '{variableName}'")
            { }
        }

        protected readonly DataRegistry _dataRegistry = new DataRegistry();

        /// <summary>
        /// creates a standard variable rush with known namespaces
        /// </summary>
        public Variables() {
            // standard namespaces
            foreach (var ns in Enum.GetValues(typeof(VariableNameSpace)))
                _dataRegistry.Set(ns + "", new DataObject(ns+"",false));

            // Env vars
            var pfx = Nsp(VariableNameSpace.Env);
            foreach (DictionaryEntry envvar in Environment.GetEnvironmentVariables())
                _dataRegistry.Set(pfx+envvar.Key, envvar.Value);
        }

        #region setters

        public void Set( string path, object value) => _dataRegistry.Set( path, value);
        public void Set( string rootPath, string path, object value) => _dataRegistry.Set( Nsp(rootPath,path), value);
        public void Set( VariableNameSpace rootPath, string path, object value) => _dataRegistry.Set(Nsp(rootPath, path), value);

        public void Unset(string path) => _dataRegistry.Unset(path);
        public void Unset(string rootPath,string path) => _dataRegistry.Unset(Nsp(rootPath,path));
        public void Unset( VariableNameSpace rootPath, params string[] path) => _dataRegistry.Unset(Nsp(rootPath,path));

        #endregion

        #region getters

        /// <summary>
        /// serch in data context the path according to these precedence rules:
        /// - full path
        /// - path related to Local
        /// - path related to Env
        /// - path related to Global
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Get( string path, out object value, bool throwException=true )
        {
            var r = _dataRegistry.Get(path, out value)
            || _dataRegistry.Get(Nsp(VariableNameSpace.Local, path), out value)
            || _dataRegistry.Get(Nsp(VariableNameSpace.Env, path), out value)
            || _dataRegistry.Get(Nsp(VariableNameSpace.Env, path), out value);
            if (!r && throwException)
                throw new VariableNotFoundException(GetVariableName(path));            
            return r;
        }

        public bool Get( string rootPath, string path, out object value, bool throwException = true )
            => Get(Nsp(rootPath, path), out value, throwException);

        public bool Get(VariableNameSpace rootPath, string path, out object value, bool throwException = true)
            => Get(Nsp(rootPath, path), out value, throwException);

        public T GetValue<T>(string path) => (T)GetValue(path).Value;
        public T GetValue<T>(string rootPath,string path) => (T)GetValue(Nsp(rootPath,path)).Value;
        public T GetValue<T>(VariableNameSpace rootPath,string path) => (T)GetValue(Nsp(rootPath,path)).Value;

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
        public bool GetDataObject(VariableNameSpace rootPath, string path, out IDataObject value, bool throwException = true)
            => GetDataObject(Nsp(rootPath, path), out value, throwException);

        public DataValue GetValue(string path,bool throwException=true)
        {
            if (Get(path, out var data, false))
                return (DataValue)data;
            if (throwException)
                    throw new VariableNotFoundException(GetVariableName(path));
            return null;
        }
        public DataValue GetValue(string rootPath, string path, bool throwException = true)
            => GetValue(Nsp(rootPath, path), throwException);
        public DataValue GetValue(VariableNameSpace rootPath, string path, bool throwException = true)
            => GetValue(Nsp(rootPath, path), throwException);

        public bool GetValue(string path,out DataValue value,bool throwException=true)
        {
            if (Get(path, out var data, false))
            {
                value = (DataValue)data;
                return true;
            }
            if (throwException)
                throw new VariableNotFoundException(GetVariableName(path));
            value = null;
            return false;
        }

        public bool GetValue(string rootPath, string path, out DataValue value, bool throwException = true)
            => GetValue(Nsp(rootPath, path), out value, throwException);
        public bool GetValue(VariableNameSpace rootPath, string path, out DataValue value, bool throwException = true)
            => GetValue(Nsp(rootPath, path), out value, throwException);

        public List<IDataObject> GetDataValues() => _dataRegistry.GetDataValues();

        #endregion

        public bool GetPathOwner(string path,out object data)
            => _dataRegistry.GetPathOwner(path,out data);

        public static string Nsp( string @namespace, string key) => @namespace + CommandLineSyntax.VariableNamePathSeparator + key;
        public static string Nsp( params string[] key) => string.Join( CommandLineSyntax.VariableNamePathSeparator , key);
        public static string Nsp( VariableNameSpace @namespace, string key = "") => @namespace + (CommandLineSyntax.VariableNamePathSeparator + "") + key;
        public static string Nsp( VariableNameSpace @namespace, params string[] key) => @namespace + (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join( CommandLineSyntax.VariableNamePathSeparator, key);
    }
}
