using UnityEngine;

namespace PikePush.Utls
{
    public static class LogHelper
    {
    
        public static void debug(object message)
        {
             Debug.Log(message);
        }
        public static void info(object message)
        {
             Debug.Log(message);
        }
        public static void warn(object message)
        {
             Debug.LogWarning(message);
        }
        public static void error(object message)
        {
             Debug.LogError(message);
        }
    }
}