using mitoSoft.Workflows.Exceptions;
using System.Threading;

namespace mitoSoft.Workflows.Extensions
{
    internal static class CancellationTokenExtensions
    {
        public static void ThrowIfCanceled(this CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                throw new StateMachineCanceledException();
            }
        }
    }
}