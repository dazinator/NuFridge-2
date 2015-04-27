using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Web.Http;
using NuFridge.Service.Diagnostics;
using NuFridge.Service.Diagnostics.Win;

namespace NuFridge.Service.Website.Controllers
{
    public class DiagnosticsController : ApiController
    {
        [Route("api/diagnostics")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var system = GetComputerSystem();
            var process = GetProcess();

            var response = new SystemInfo(system, process);

            return Request.CreateResponse(response);
        }

        private Win32_Process GetProcess()
        {
            var currentProcess = Process.GetCurrentProcess();

            int processId = currentProcess.Id;

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ProcessID =" + processId))
            {
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                   return new Win32_Process(obj);
                }
            }

            return null;
        }

        private Win32_ComputerSystem GetComputerSystem()
        {
            SelectQuery query = new SelectQuery(@"Select * from Win32_ComputerSystem");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    return new Win32_ComputerSystem(obj);
                }
            }

            return null;
        }
    }
}