using System;
using System.Runtime.Serialization;

namespace mitoSoft.Workflows.Exceptions
{
    internal class StateMachineTimedOutException : StateMachineException
    {
        public StateMachineTimedOutException()
        {
        }

        public StateMachineTimedOutException(string message) : base(message)
        {
        }

        public StateMachineTimedOutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public StateMachineTimedOutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}