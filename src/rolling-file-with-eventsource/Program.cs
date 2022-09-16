using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Threading.Tasks;

namespace rolling_file_with_eventsource
{
    public static class Program
    {
        private static readonly TimeSpan fileRollingInterval = TimeSpan.FromSeconds(10);

        private const string TraceFileNamePrefix = "C:\\Traces\\Trace.";
        private const string TraceFileNameSuffix = ".etl";

        private static int FileNumber = 1;

        public async static Task Main(string[] args)
        {
            string fileName = GetNextFileName();
            string sessionName = "Rolling-File-With-EventSource";
            using TraceEventSession session = new TraceEventSession(sessionName, fileName);
            Console.WriteLine($"Setup session to write to {fileName}.");

            // Register for CTRL+C.
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs cancelArgs)
            {
                session.Dispose();
                Environment.Exit(0);
            };

            // Start writing events.
            EventProducer.LogEvents();

            // Start collecting events from the EventProducer.
            session.EnableProvider("EventProducer", Microsoft.Diagnostics.Tracing.TraceEventLevel.Verbose, unchecked((ulong)-1));


            using TraceEventSession attachedSession = TraceEventSession.GetActiveSession(sessionName);

            do
            {
                // Wait while events are written to the current file.
                Console.WriteLine($"Generating events for {fileRollingInterval.TotalSeconds} seconds.");
                await Task.Delay(fileRollingInterval);

                // Switch to the next file.
                fileName = GetNextFileName();
                attachedSession.SetFileName(fileName);
                Console.WriteLine($"Rolled to new file {fileName}");
            }
            while (true);
        }

        private static string GetNextFileName()
        {
            return $"{TraceFileNamePrefix}{FileNumber++}{TraceFileNameSuffix}";
        }
    }
}