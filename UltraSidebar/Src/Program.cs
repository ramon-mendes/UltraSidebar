using SciterSharp;
using SciterSharp.Interop;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraSidebar
{
	class Program
	{
		public static Window WndGlobal;
		public static Hooker HookerInstance { get; private set; } = new Hooker();
		public static Host AppHost { get; private set; }

		[STAThread]
		static void Main(string[] args)
		{
			// Sciter needs this for drag'n'drop support; STAThread is required for OleInitialize succeess
			int oleres = PInvokeWindows.OleInitialize(IntPtr.Zero);
			Debug.Assert(oleres == 0);
			
			// Create the window
			var wnd = WndGlobal = new Window();

			// Prepares SciterHost and then load the page
			var host = AppHost = new Host();
			host.Setup(wnd);
			host.AttachEvh(new HostEvh());
			host.RegisterBehaviorHandler(typeof(AreaChart));
			host.SetupPage("index.html");
			//host.SetupPage("widgets/dolar.html");

			HookerInstance.SetMessageHook();

			// Show window and Run message loop
			wnd.Show();
			PInvokeUtils.RunMsgLoop();

			HookerInstance.ClearHook();

			FinalizeApp();
		}

		public static void FinalizeApp()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}