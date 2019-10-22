using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciterSharp;
using SciterSharp.Interop;
using System.Net;
using UltraSidebar.Native;
//using Ion;
using PInvoke;

namespace UltraSidebar
{
	class SciterMessages : SciterDebugOutputHandler
	{
		protected override void OnOutput(SciterXDef.OUTPUT_SUBSYTEM subsystem, SciterXDef.OUTPUT_SEVERITY severity, string text)
		{
			Console.WriteLine(text);
			//Debug.Write(text);// so I can see Debug output even if 'native debugging' is off
		}
	}

	class Program
	{
		private static SciterMessages sm = new SciterMessages();
		public static Hooker HookerInstance = new Hooker();
		public static MainWindow WndMain;

		[STAThread]
		static void Main(string[] args)
		{
			#region Args handling
			bool arg_in_test = false;

			if(args.Length > 0)
			{
				if(args[0].StartsWith("-jumplist:"))
				{
					MainWindow.SendJumplistCmd(args[0].Substring(10));
					return;
				}
			}
			#endregion

			//UpdateControl.Setup();

			// Sciter needs this for drag'n'drop support; STAThread is required for OleInitialize succeess
			int oleres = PInvokeWindows.OleInitialize(IntPtr.Zero);
			Debug.Assert(oleres == 0);
			Debug.WriteLine("Sciter " + SciterX.Version);

			// Create the window
			var wnd = WndMain = new MainWindow();
			wnd.CreateMainWindow(800, 600, SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_ALPHA | SciterXDef.SCITER_CREATE_WINDOW_FLAGS.SW_ENABLE_DEBUG);
			wnd.CreateTaskbarIcon();
			wnd.CreateJumplists();
			wnd.Title = MainWindow.WND_TITLE;
			wnd.CenterTopLevelWindow();
			wnd.Show();

			// Prepares SciterHost and then load the page
			var host = new BaseHost();
			host.Setup(wnd);
			host.AttachEvh(new HostEvh());
			host.RegisterBehaviorHandler(typeof(AreaChart));
			//host.SetupPage("index.html");
			host.SetupPage("widgets/dolar.html");

			HookerInstance.SetMessageHook();

			if(!arg_in_test && SingleInstance.IsRunningAndAcquire())
			{
				Debug.WriteLine("ALREADY RUNNING!");
				return;
			}

			//MainWindow.SendJumplistCmd("BringToFront");

			// Run message loop
			PInvokeUtils.RunMsgLoop();

			Exit();
		}

		public static void Exit()
		{
#if WINDOWS
			WndMain.Destroy();
			WndMain.Dispose();

			SingleInstance.Release();
			HookerInstance.ClearHook();

			Thread.Sleep(200);
			Environment.Exit(0);
			User32.PostQuitMessage(0);

			GC.Collect();
			GC.WaitForPendingFinalizers();
#else
			AppKit.NSApplication.SharedApplication.Terminate(null);
#endif
		}
	}
}