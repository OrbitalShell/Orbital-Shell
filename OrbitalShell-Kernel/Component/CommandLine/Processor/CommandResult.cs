   
namespace OrbitalShell.Component.CommandLine.Processor
{
    public class CommandVoidResult : ICommandResult
    {
        static CommandVoidResult _Instance;
        public static CommandVoidResult Instance
        {
            get
            {
                if (_Instance == null) _Instance = new CommandVoidResult();
                return _Instance;
            }
        }

        public int ReturnCode { get; protected set; }

        public string ExecErrorText { get; protected set; }

        public object GetOuputData()
        {
            return null;
        }

        public CommandVoidResult()
        {
            ReturnCode = (int)Processor.ReturnCode.OK;
        }

        public CommandVoidResult(
            int returnCode)
        {
            ReturnCode = returnCode;
        }

        public CommandVoidResult(
            ReturnCode returnCode,
            string execErrorText
            )
        {
            ReturnCode = (int)returnCode;
            ExecErrorText = execErrorText;
        }

        public CommandVoidResult(
            int returnCode,
            string execErrorText)
        {
            ReturnCode = returnCode;
            ExecErrorText = execErrorText;
        }

        public CommandVoidResult(
            ReturnCode returnCode)
        {
            ReturnCode = (int)returnCode;
        }
    }

    public class CommandResult<T> : ICommandResult
    {
        public T OutputData;
        public int ReturnCode { get; protected set; }
        public string ExecErrorText { get; protected set; }

        public CommandResult(
            T outputData,
            string execErrorText = null
            )
        {
            OutputData = outputData;
            ReturnCode = (int)Processor.ReturnCode.OK;
            ExecErrorText = execErrorText;
        }

        public CommandResult(
            T outputData,
            int returnCode,
            string execErrorText = null
            )
        {
            OutputData = outputData;
            ReturnCode = returnCode;
            ExecErrorText = execErrorText;
        }

        public CommandResult(
            T outputData,
            ReturnCode returnCode,
            string execErrorText = null
            )
        {
            OutputData = outputData;
            ReturnCode = (int)returnCode;
            ExecErrorText = execErrorText;
        }

        public CommandResult(
            int returnCode,
            string execErrorText = null
            )
        {
            ReturnCode = returnCode;
            ExecErrorText = execErrorText;
        }

        public CommandResult(
            ReturnCode returnCode,
            string execErrorText = null
            )
        {
            ReturnCode = (int)returnCode;
            ExecErrorText = execErrorText;
        }

        public CommandResult(
            )
        {
            ReturnCode = (int)Processor.ReturnCode.OK;
        }

        public object GetOuputData() => OutputData;
    }
}
