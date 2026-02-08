using System;
using System.Threading.Tasks;

namespace MoNbtSearcher {
    public interface IThreadWorker {
        public int LoadTotalNum { get; }
        public int LoadedNum { get; }
        public bool IsCompleted { get; }
    }

    public static class IReaderExtensions {
        public static string GetProgress(this IThreadWorker reader) {
            return $"{reader.LoadedNum}/{reader.LoadTotalNum}";
        }
        public static void ThreadAsyncDo(this IThreadWorker reader, int threadNum, Func<Task> evt, Action<Exception> exEvt, Action callback) {
            Task[] tasks = new Task[threadNum];
            for (int i = 0; i < threadNum; i++) {
                tasks[i] = Task.Run(async () => {
                    try {
                        await evt?.Invoke();
                    }
                    catch (Exception e) {
                        exEvt?.Invoke(e);
                    }
                });
            }
            Task.Run(async () => {
                await Task.WhenAll(tasks);
                callback?.Invoke();
            });
        }
    }
}