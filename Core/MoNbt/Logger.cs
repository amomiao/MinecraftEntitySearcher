using MoNbtSearcher;

namespace MoNbt {
    public static class Logger {
        /// <summary> 普通的日志 </summary>
        /// <param name="weight"> 根据权重可以进行一些行为 </param>
        public static void Log(string message, int weight = 0) => MoNbtSearcherHelper.CrossPlatform.Log(message, weight);

        /// <summary> 会显式提示用户的日志 </summary>
        public static void LogUserWarning(string message) => MoNbtSearcherHelper.CrossPlatform.LogUserWarning(message);
    }
}