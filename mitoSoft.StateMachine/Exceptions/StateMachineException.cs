using System;
using System.Runtime.Serialization;

namespace mitoSoft.Workflows.Exceptions
{
    public abstract class StateMachineException : Exception
    {
        public StateMachineException()
        {
        }

        public StateMachineException(string message) : base(message)
        {
        }

        public StateMachineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StateMachineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}