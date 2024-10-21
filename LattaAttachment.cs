using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace LattaASPNet
{
    public class LattaAttachment
    {
        private readonly Exception _exception;
        private readonly HttpContext _context;
        private readonly IEnumerable<LattaLog> _logs;

        public LattaAttachment(Exception exception, HttpContext httpContext, IEnumerable<LattaLog> logs)
        {
            _exception = exception;
            _context = httpContext;
            _logs = logs;
        }

        private double GetCpuLoad()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var process = Process.GetCurrentProcess();
                return (process.TotalProcessorTime.TotalMilliseconds /
                        (Environment.ProcessorCount * 1000)) * 100;
            }

            var load = new double[3];
            using (var reader = new System.IO.StreamReader("/proc/loadavg"))
            {
                var line = reader.ReadLine();
                var parts = line.Split(' ');
                load[0] = Convert.ToDouble(parts[0]); // 1 minute load average
            }

            return load[0];
        }

        private (long freeMemory, long totalMemory) GetRam()
        {
            var totalMemory = GC.GetGCMemoryInfo().HeapSizeBytes;
            var freeMemory = Environment.WorkingSet;

            return (freeMemory, totalMemory);
        }

        public override string ToString()
        {
            var request = _context.Request;
            var response = _context.Response;

            var (freeMemory, totalMemory) = GetRam();
            var cpuLoad = GetCpuLoad();

            var queryObject = new Dictionary<string, string>(request.Query.Select(param => new KeyValuePair<string, string>(param.Key, param.Value!)));

            var record = new
            {
                type = "record",
                data = new
                {
                    type = "request",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    level = "ERROR",
                    request = new
                    {
                        method = request.Method,
                        url = $"{(request.Scheme)}://{request.Host}{request.Path}{request.QueryString}",
                        route = request.Path,
                        query = queryObject,
                        headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                        body = new StreamReader(request.Body).ReadToEndAsync().Result
                    },
                    response = new
                    {
                        status_code = response.StatusCode,
                        body = "",
                        headers = response.Headers
                    },
                    name = "Fatal Error",
                    message = _exception.Message,
                    stack = _exception.StackTrace,
                    environment_variables = Environment.GetEnvironmentVariables(),
                    system_info = new
                    {
                        free_memory = freeMemory,
                        total_memory = totalMemory,
                        cpu_usage = cpuLoad,
                    },
                    logs = new
                    {
                        entries = _logs.Select(log => new { message = log.message, level = log.level, timestamp = log.timestamp })
                    }
                }
            };

            return JsonConvert.SerializeObject(record);
        }
    }
}
