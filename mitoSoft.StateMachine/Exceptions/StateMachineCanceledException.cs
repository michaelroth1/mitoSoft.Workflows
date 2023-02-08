using System;
using System.Runtime.Serialization;

namespace mitoSoft.Workflows.Exceptions
{
    internal class StateMachineCanceledException : StateMachineException
    {
        public StateMachineCanceledException()
        {
        }

        public StateMachineCanceledException(string message) : base(message)
        {
        }

        public StateMachineCanceledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public StateMachineCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}