   
namespace DotNetConsoleAppToolkit.Component.CommandLine.Processor
{
    public class CommandVoidResult : ICommandResult
    {
        public int ReturnCode { get; protected set; }

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
            ReturnCode returnCode)
        {
            ReturnCode = (int)returnCode;
        }
    }

    public class CommandResult<T> : ICommandResult
    {
        public T OutputData;
        public int ReturnCode { get; protected set; }

        public CommandResult(
            T outputData
            )
        {
            OutputData = outputData;
            ReturnCode = (int)Processor.ReturnCode.OK;
        }

        public CommandResult(
            T outputData,
            int returnCode
            )
        {
            OutputData = outputData;
            ReturnCode = returnCode;
        }

        public CommandResult(
            T outputData,
            ReturnCode returnCode
            )
        {
            OutputData = outputData;
            ReturnCode = (int)returnCode;
        }

        public CommandResult(
            int returnCode
            )
        {
            ReturnCode = returnCode;
        }

        public CommandResult(
            ReturnCode returnCode
            )
        {
            ReturnCode = (int)returnCode;
        }

        public CommandResult(
            )
        {
            ReturnCode = (int)Processor.ReturnCode.OK;
        }

        public object GetOuputData() => OutputData;
    }
}
