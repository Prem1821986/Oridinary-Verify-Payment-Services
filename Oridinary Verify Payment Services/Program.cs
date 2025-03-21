using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DMS_WindowsService
{
	static class Program
	{
		static void Main()
		{

#if DEBUG
			DMSWindowsService myService = new DMSWindowsService();
			myService.OnDebug();
#else
					ServiceBase[] ServicesToRun;
					 ServicesToRun = new ServiceBase[]
					 {
						 new DMSWindowsService()
					 };
					 ServiceBase.Run(ServicesToRun);
#endif

		}
	}
}
