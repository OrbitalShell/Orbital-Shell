using System;
using System.Linq;
using System.Collections.Generic;
using OrbitalShell.Lib;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using System.Reflection;
using System.Collections.ObjectModel;
using OrbitalShell.Component.CommandLine.Variable;
using System.Text;

namespace OrbitalShell.Component.CommandLine.Module
{
    /// <summary>
    /// module command manager
    /// </summary>
    public class ModuleCommandManager
    {
        #region attributes

        public IEnumerable<string> CommandDeclaringShortTypesNames => AllCommands.Select(x => x.DeclaringTypeShortName).Distinct();

        public IEnumerable<string> CommandDeclaringTypesNames => AllCommands.Select(x => x.DeclaringTypeFullName).Distinct();

        readonly Dictionary<string, List<CommandSpecification>> _commands = new Dictionary<string, List<CommandSpecification>>();

        public IReadOnlyDictionary<string, List<CommandSpecification>> Commands => new ReadOnlyDictionary<string, List<CommandSpecification>>(_commands);

        public List<CommandSpecification> AllCommands
        {
            get
            {
                var coms = new List<CommandSpecification>();
                foreach (var kvp in _commands)
                    foreach (var com in kvp.Value)
                        coms.Add(com);
                coms.Sort(new Comparison<CommandSpecification>((x, y) => x.Name.CompareTo(y.Name)));
                return coms;
            }
        }

        readonly ModuleSet _modules;

        readonly SyntaxAnalyser _syntaxAnalyzer;

        #endregion

        public ModuleCommandManager(
            SyntaxAnalyser syntaxAnalyser,
            ModuleSet modules
            )
        {
            _syntaxAnalyzer = syntaxAnalyser;
            _modules = modules;
        }

        public bool UnregisterCommand(CommandSpecification comSpec)
        {
            if (_commands.TryGetValue(comSpec.Name, out var cmdLst))
            {
                var r = cmdLst.Remove(comSpec);
                if (r)
                    _syntaxAnalyzer.Remove(comSpec);
                if (cmdLst.Count == 0)
                    _commands.Remove(comSpec.Name);
                return r;
            }
            return false;
        }

        public ModuleSpecification
            UnregisterModuleCommands(
            CommandEvaluationContext context,
            string moduleName)
        {
            var moduleSpecification = _modules.Values.Where(x => x.Name == moduleName).FirstOrDefault();
            if (moduleSpecification != null)
            {
                foreach (var com in AllCommands)
                    if (com.MethodInfo.DeclaringType.Assembly == moduleSpecification.Assembly)
                        UnregisterCommand(com);
                return moduleSpecification;
            }
            else
            {
                context.Errorln($"commands module '{moduleName}' not registered");
                return ModuleSpecification.ModuleSpecificationNotDefined;
            }
        }

        int _RegisterModuleCommands(CommandEvaluationContext context, Type type)
        {
            if (type.GetInterface(typeof(ICommandsDeclaringType).FullName) != null)
                return RegisterCommandClass(context, type, false);
            return 0;
        }

        public void RegisterCommandClass<T>(CommandEvaluationContext context) => RegisterCommandClass(context, typeof(T), true);

        public int RegisterCommandClass(CommandEvaluationContext context, Type type) => RegisterCommandClass(context, type, true);

        public string CheckAndNormalizeCommandNamespace(string[] segments)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                var s = segments[i];
                if (string.IsNullOrWhiteSpace(s)) throw new Exception("invalid namespace segment: '{s}'");
                segments[i] = s.ToLower();
            }
            return string.Join(CommandLineSyntax.CommandNamespaceSeparator, segments);
        }

        /// <summary>
        /// normalize a command name according to naming conventions
        /// </summary>
        /// <param name="s">command name to be normalized</param>
        /// <returns>normalized command name</returns>
        public string CheckAndNormalizeCommandName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new Exception("command name can't be empty");
            var sb = new StringBuilder();
            var t = s.ToCharArray();
            for (int i = 0; i < t.Length; i++)
            {
                var c = t[i];
                if (char.IsUpper(c) && (i == t.Length - 1 || !char.IsUpper(t[i + 1])))
                {
                    sb.Append(CommandLineSyntax.CommandNamespaceSeparator);
                    sb.Append(char.ToLower(c));
                }
                else
                    sb.Append(c);
            }
            var r = sb.ToString();
            if (r.StartsWith(CommandLineSyntax.CommandNamespaceSeparator))
                r = r.Length > 1 ? r.Substring(1) : "";
            return r;
        }

        public int RegisterCommandClass(
            CommandEvaluationContext context,
            Type type,
            bool registerAsModule
            )
        {
            if (type.GetInterface(typeof(ICommandsDeclaringType).FullName) == null)
                throw new Exception($"the type '{type.FullName}' must implements interface '{typeof(ICommandsDeclaringType).FullName}' to be registered as a command class");

            var dtNamespaceAttr = type.GetCustomAttribute<CommandsNamespaceAttribute>();
            var dtNamespace = (dtNamespaceAttr == null) ? "" : CheckAndNormalizeCommandNamespace(dtNamespaceAttr.Segments);

            var comsCount = 0;
            object instance = Activator.CreateInstance(type, new object[] { });
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (registerAsModule && _modules.ContainsKey(type.FullName))
            {
                context.Errorln($"a module with same name than the commands declaring type '{type.FullName}' is already registered");
                return 0;
            }

            foreach (var method in methods)
            {
                var cmd = method.GetCustomAttribute<CommandAttribute>();
                if (cmd != null)
                {
                    if (!method.ReturnType.HasInterface(typeof(ICommandResult)))
                    {
                        context.Errorln($"class={type.FullName} method={method.Name} wrong return type. should be of type '{typeof(ICommandResult).FullName}', but is of type: {method.ReturnType.FullName}");
                    }
                    else
                    {
                        // ⏺ build the command specification from the method meta-data

                        var cmdNamespaceAttr = method.GetCustomAttribute<CommandNamespaceAttribute>();
                        var cmdNamespace = cmdNamespaceAttr == null ? dtNamespace : CheckAndNormalizeCommandNamespace(cmdNamespaceAttr.Segments);

                        #region init from method parameters attributes

                        var paramspecs = new List<CommandParameterSpecification>();
                        bool syntaxError = false;
                        var pindex = 0;
                        foreach (var parameter in method.GetParameters())
                        {
                            if (pindex == 0)
                            {
                                // manadatory: param 0 is CommandEvaluationContext
                                if (parameter.ParameterType != typeof(CommandEvaluationContext))
                                {
                                    context.Errorln($"class={type.FullName} method={method.Name} parameter 0 ('{parameter.Name}') should be of type '{typeof(CommandEvaluationContext).FullName}', but is of type: {parameter.ParameterType.FullName}");
                                    syntaxError = true;
                                    break;
                                }
                            }
                            else
                            {
                                CommandParameterSpecification pspec = null;
                                object defval = null;
                                if (!parameter.HasDefaultValue && parameter.ParameterType.IsValueType)
                                    defval = Activator.CreateInstance(parameter.ParameterType);

                                // param
                                var paramAttr = parameter.GetCustomAttribute<ParameterAttribute>();
                                if (paramAttr != null)
                                {
                                    // TODO: validate command specification (eg. indexs validity)
                                    pspec = new CommandParameterSpecification(
                                        parameter.Name,
                                        paramAttr.Description,
                                        paramAttr.IsOptional,
                                        paramAttr.Index,
                                        null,
                                        null,
                                        true,
                                        parameter.HasDefaultValue,
                                        paramAttr.HasDefaultValue ?
                                            paramAttr.DefaultValue
                                            : ((parameter.HasDefaultValue) ? parameter.DefaultValue : defval),
                                                parameter);
                                }

                                // option
                                var optAttr = parameter.GetCustomAttribute<OptionAttribute>();
                                if (optAttr != null)
                                {
                                    var reqParamAttr = parameter.GetCustomAttribute<OptionRequireParameterAttribute>();
                                    try
                                    {
                                        pspec = new CommandParameterSpecification(
                                            parameter.Name,
                                            optAttr.Description,
                                            optAttr.IsOptional,
                                            -1,
                                            optAttr.OptionName /*?? parameter.Name*/,
                                            optAttr.OptionLongName,
                                            optAttr.HasValue,
                                            parameter.HasDefaultValue,
                                            optAttr.HasDefaultValue ?
                                                optAttr.DefaultValue
                                                : ((parameter.HasDefaultValue) ? parameter.DefaultValue : defval),
                                            parameter,
                                            reqParamAttr?.RequiredParameterName);
                                    }
                                    catch (Exception ex)
                                    {
                                        context.Errorln(ex.Message);
                                    }
                                }

                                if (pspec == null)
                                {
                                    syntaxError = true;
                                    context.Errorln($"invalid parameter: class={type.FullName} method={method.Name} name={parameter.Name}");
                                }
                                else
                                    paramspecs.Add(pspec);
                            }
                            pindex++;
                        }

                        #endregion

                        if (!syntaxError)
                        {
                            var cmdNameAttr = method.GetCustomAttribute<CommandNameAttribute>();

                            var cmdName = CheckAndNormalizeCommandName(
                                    (cmdNameAttr != null && cmdNameAttr.Name != null) ?
                                        cmdNameAttr.Name
                                        : (cmd.Name ?? method.Name));

                            var cmdspec = new CommandSpecification(
                                cmdNamespace,
                                cmdName,
                                cmd.Description,
                                cmd.LongDescription,
                                cmd.Documentation,
                                method,
                                instance,
                                paramspecs);

                            bool registered = true;
                            if (_commands.TryGetValue(cmdspec.Name, out var cmdlst))
                            {
                                if (cmdlst.Select(x => x.MethodInfo.DeclaringType == type).Any())
                                {
                                    context.Errorln($"command already registered: '{cmdspec.Name}' in type '{cmdspec.DeclaringTypeFullName}'");
                                    registered = false;
                                }
                                else
                                    cmdlst.Add(cmdspec);
                            }
                            else
                                _commands.Add(cmdspec.Name, new List<CommandSpecification> { cmdspec });

                            if (registered)
                            {
                                _syntaxAnalyzer.Add(cmdspec);
                                comsCount++;
                            }
                        }
                    }
                }
            }

            if (registerAsModule)
            {
                if (comsCount == 0)
                    context.Errorln($"no commands found in type '{type.FullName}'");
                else
                {
                    var descAttr = type.GetCustomAttribute<CommandsAttribute>();
                    var description = descAttr != null ? descAttr.Description : "";
                    _modules.Add(
                        type.FullName,      // key not from assembly but from type
                        new ModuleSpecification(
                            type.FullName,
                            ModuleUtil.DeclaringTypeShortName(type),
                            description,
                            type.Assembly,
                            new ModuleInfo(
                                1,
                                comsCount
                                ),
                            type
                            ));
                }
            }
            return comsCount;
        }

        public CommandSpecification GetCommandSpecification(MethodInfo commandMethodInfo)
            => AllCommands.Where(x => x.MethodInfo == commandMethodInfo).FirstOrDefault();
    }
}