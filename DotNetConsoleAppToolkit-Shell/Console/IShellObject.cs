using DotNetConsoleAppToolkit.Component.CommandLine.Processor;

namespace DotNetConsoleAppToolkit.Console
{
    /// <summary>
    /// recommended but not mandatory interface of an object that should almost be display in the shell and used a a command parameter
    /// </summary>
    public interface IShellObject
    {
        /// <summary>
        /// Echo method : ouptut a formatted text display of the object into @out, eventually using the formatting options.
        /// must works with a given command evaluation context
        /// </summary>
        /// <param name="out">output stream</param>
        /// <param name="context">command eval context</param>
        /// <param name="options">formatting options</param>
        void Echo(
            ConsoleTextWriterWrapper @out, 
            CommandEvaluationContext context, 
            FormattingOptions options = null );

        /// <summary>
        /// returns the object as text representation of its value (the returned value might be convertible to a native value)
        /// <para>this method must be in place of ToString() to provide a string value to describe the object. ToString must be reserved for debug purpose only</para>
        /// </summary>
        /// <returns>readable value representing the object</returns>
        string AsText();
    }
}