using System; 

namespace Illarion.Client.Common
{
    public interface ILogger
    {
        void Error(string message);
        void Warning(string message);
        void Debug(string message);

        void Error(string message, Exception e);
        void Warning(string message, Exception e);
        void Debug(string message, Exception e);
    }
}
