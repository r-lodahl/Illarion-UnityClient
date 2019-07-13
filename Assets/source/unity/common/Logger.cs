using System;
using UnityDebug = UnityEngine.Debug;
using Illarion.Client.Common;

namespace Illarion.Client.Unity.Common
{
    public class Logger : ILogger
    {
        public void Debug(string message) => UnityDebug.Log(message);
    
        public void Debug(string message, Exception e) => UnityDebug.Log($"{message}: {e.StackTrace}");
    
        public void Error(string message) => UnityDebug.LogError(message);
    
        public void Error(string message, Exception e) => UnityDebug.LogError($"{message}: {e.StackTrace}");
    
        public void Warning(string message) => UnityDebug.LogWarning(message);
    
        public void Warning(string message, Exception e) => UnityDebug.LogWarning($"{message}: {e.StackTrace}");
    }
}