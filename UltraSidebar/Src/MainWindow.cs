using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;
using PInvoke;
using SciterSharp;
using SciterSharp.Interop;

namespace UltraSidebar
{
	class MainWindow : SciterWindow, IDisposable
	{
		private static NotifyIcon _ni;

		static uint WM_TASKBAR_CREATED = RegisterWindowMessage("TaskbarCreated");
		static Dictionary<string, uint> _cmd2jumplist_msg = new Dictionary<string, uint>()
		{
			["BringToFront"] = RegisterWindowMessage(Consts.AppName + ".BringToFront"),
			["ClearHistoryArg"] = RegisterWindowMessage(Consts.AppName + ".Jumplist.ClearHistoryArg")
		};
		
		public const	string WND_TITLE = "Sciter-based desktop Sticky notes";
		const   uint WM_APP = 0x8000;
		const	uint WM_DESKTOP_CHANGED = WM_APP + 99;

		protected override bool ProcessWindowMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref IntPtr lResult)
		{
			foreach(var item in _cmd2jumplist_msg)
			{
				if(msg == item.Value)
				{
				}
			}


			if(msg == WM_TASKBAR_CREATED)
			{
				Program.HookerInstance.SetMessageHook();
				return true;
			}

			if(msg == WM_DESKTOP_CHANGED)
			{
				if(wParam.ToInt32() == 0)
				{
					ShowIt(true);
					Debug.WriteLine("WM_DESKTOP_CHANGED show " + DateTime.Now);
				}
				else
				{
					ShowIt(false);
					Debug.WriteLine("WM_DESKTOP_CHANGED hide " + DateTime.Now);
				}
				return true;
			}

			if(msg == (uint)User32.WindowMessage.WM_ENDSESSION)
			{
				// system is shuting down, close app
				User32.SendMessage(_hwnd, User32.WindowMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
				User32.PostQuitMessage(0);
				return true;
			}

			return false;
		}


		public static void SendJumplistCmd(string cmd)
		{
			var wnd = User32.FindWindow(null, WND_TITLE);
			if(wnd != IntPtr.Zero)
			{
				User32.SendMessage(wnd, (User32.WindowMessage) _cmd2jumplist_msg[cmd], IntPtr.Zero, IntPtr.Zero);
				SciterSharp.MessageBox.Show(IntPtr.Zero, cmd, cmd);
			}
		}

		public void CreateJumplists()
		{
			JumpList list = JumpList.CreateJumpListForIndividualWindow(TaskbarManager.Instance.ApplicationId, _hwnd);
			JumpListCustomCategory userActionsCategory = new JumpListCustomCategory("Actions");
			JumpListLink userActionLink = new JumpListLink(Assembly.GetEntryAssembly().Location, "Clear History");
			userActionLink.Arguments = "-jumplist:gogo";

			//add this link to the Actions Category
			userActionsCategory.AddJumpListItems(userActionLink);

			//finally add the category to the JumpList
			list.AddCustomCategories(userActionsCategory);

			//get the notepad.exe path
			string notepadPath = Path.Combine(Environment.SystemDirectory, "notepad.exe");

			//attach it to the JumpListLink
			JumpListLink jlNotepad = new JumpListLink(notepadPath, "Notepad");

			//set its icon path
			jlNotepad.IconReference = new IconReference(notepadPath, 0);

			//add it to the list
			list.AddUserTasks(jlNotepad);

			list.Refresh();
		}

		public void CreateTaskbarIcon()
		{
			var menu = new ContextMenu();
			//menu.MenuItems.Add(new MenuItem("Add Note", (e, a) => CreateNote()));
			menu.MenuItems.Add(new MenuItem("Quit", (e, a) => Program.Exit()));

			_ni = new NotifyIcon();
			_ni.Icon = Properties.Resources.note;
			_ni.Visible = true;
			_ni.ContextMenu = menu;
			_ni.Click += (s, e) =>
			{
				if((e as MouseEventArgs).Button == MouseButtons.Left)
					Activate();
			};
		}

		public void SetUltraTopmost(bool top)
		{
			if(top)
			{
				SetWindowPos(_hwnd, new IntPtr((int)SetWindowPosWindow.HWND_BOTTOM), 0, 0, 0, 0,
					SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE);
				SetWindowPos(_hwnd, new IntPtr((int)SetWindowPosWindow.HWND_TOPMOST), 0, 0, 0, 0,
					SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE);
			}
			else
			{
				SetWindowPos(_hwnd, new IntPtr((int)SetWindowPosWindow.HWND_NOTOPMOST), 0, 0, 0, 0,
							SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSENDCHANGING);
				SetWindowPos(_hwnd, new IntPtr((int)SetWindowPosWindow.HWND_BOTTOM), 0, 0, 0, 0,
					SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSENDCHANGING);
			}
		}

		public void Dispose()
		{
			_ni.Dispose();
		}

		private void ShowIt(bool show)
		{
			SetUltraTopmost(show);
			Show();
		}

		private void Activate()
		{
			new Win32Hwnd(_hwnd).FocusAndActivate();
				
		}

		public void EmulateMoveWnd()
		{
			SendMessageW(_hwnd, (uint)User32.WindowMessage.WM_NCLBUTTONDOWN, new IntPtr(HTCAPTION), IntPtr.Zero);
		}

		public void HideTaskbarIcon()
		{
			new Win32Hwnd(Handle).ModifyStyleEx(User32.SetWindowLongFlags.WS_EX_APPWINDOW, User32.SetWindowLongFlags.WS_EX_TOOLWINDOW);
			return;

			const int GWL_EXSTYLE = -20;
			const int WS_EX_TOOLWINDOW = 0x00000080;
			const int WS_EX_LAYERED = 0x00080000;

			SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_TOOLWINDOW);
		}

		#region PInvoke stuff
		const int HTERROR = (-2);
		const int HTTRANSPARENT = (-1);
		const int HTNOWHERE = 0;
		const int HTCLIENT = 1;
		const int HTCAPTION = 2;
		const int HTSYSMENU = 3;
		const int HTGROWBOX = 4;
		const int HTSIZE = HTGROWBOX;
		const int HTMENU = 5;
		const int HTHSCROLL = 6;
		const int HTVSCROLL = 7;
		const int HTMINBUTTON = 8;
		const int HTMAXBUTTON = 9;
		const int HTLEFT = 10;
		const int HTRIGHT = 11;
		const int HTTOP = 12;
		const int HTTOPLEFT = 13;
		const int HTTOPRIGHT = 14;
		const int HTBOTTOM = 15;
		const int HTBOTTOMLEFT = 16;
		const int HTBOTTOMRIGHT = 17;
		const int HTBORDER = 18;
		const int HTREDUCE = HTMINBUTTON;
		const int HTZOOM = HTMAXBUTTON;
		const int HTSIZEFIRST = HTLEFT;
		const int HTSIZELAST = HTBOTTOMRIGHT;

		enum WmSizeType
		{
			SIZE_MAXHIDE = 4,
			SIZE_MAXIMIZED = 2,
			SIZE_MAXSHOW = 3,
			SIZE_MINIMIZED = 1,
			SIZE_RESTORED = 0
		}

		static int LoWord(int dwValue)
		{
			return dwValue & 0xFFFF;
		}

		static int HiWord(int dwValue)
		{
			return (dwValue >> 16) & 0xFFFF;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct NCCALCSIZE_PARAMS
		{
			public PInvokeUtils.RECT rect0, rect1, rect2;
			public IntPtr lppos;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct MARGINS
		{
			public int leftWidth;
			public int rightWidth;
			public int topHeight;
			public int bottomHeight;
		}

		enum SetWindowPosWindow : int
		{
			HWND_BOTTOM = 1,
			HWND_NOTOPMOST = -2,
			HWND_TOP = 0,
			HWND_TOPMOST = -1,
		}

		[Flags]
		enum SetWindowPosFlags : uint
		{
			SWP_NOSIZE = 0x0001,
			SWP_NOMOVE = 0x0002,
			SWP_NOZORDER = 0x0004,
			SWP_NOREDRAW = 0x0008,
			SWP_NOACTIVATE = 0x0010,
			SWP_FRAMECHANGED = 0x0020,  /* The frame changed: send WM_NCCALCSIZE */
			SWP_SHOWWINDOW = 0x0040,
			SWP_HIDEWINDOW = 0x0080,
			SWP_NOCOPYBITS = 0x0100,
			SWP_NOOWNERZORDER = 0x0200,  /* Don't do owner Z ordering */
			SWP_NOSENDCHANGING = 0x0400,  /* Don't send WM_WINDOWPOSCHANGING */
			SWP_DRAWFRAME = SWP_FRAMECHANGED,
			SWP_NOREPOSITION = SWP_NOOWNERZORDER,
			SWP_DEFERERASE = 0x2000,
			SWP_ASYNCWINDOWPOS = 0x4000,
		}

		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		[DllImport("user32.dll")]
		static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("dwmapi.dll")]
		static extern int DwmDefWindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, out IntPtr result);

		[DllImport("dwmapi.dll")]
		static extern int DwmIsCompositionEnabled(out bool enabled);

		[DllImport("dwmapi.dll")]
		static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

		[DllImport("user32.dll")]
		static extern bool IsZoomed(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern bool GetWindowRect(IntPtr hwnd, out PInvokeUtils.RECT lpRect);

		[DllImport("user32.dll")]
		static extern bool ScreenToClient(IntPtr hWnd, ref PInvokeUtils.POINT lpPoint);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern uint RegisterWindowMessage(string lpString);

		[DllImport("user32.dll")]
		static extern IntPtr SendMessageW(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


		#region Windows 10 SetWindowCompositionAttribute
		internal enum AccentState
		{
			ACCENT_DISABLED = 0,
			ACCENT_ENABLE_GRADIENT = 1,
			ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
			ACCENT_ENABLE_BLURBEHIND = 3,
			ACCENT_INVALID_STATE = 4
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct AccentPolicy
		{
			public AccentState AccentState;
			public int AccentFlags;
			public int GradientColor;
			public int AnimationId;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WindowCompositionAttributeData
		{
			public WindowCompositionAttribute Attribute;
			public IntPtr Data;
			public int SizeOfData;
		}

		internal enum WindowCompositionAttribute
		{
			// ...
			WCA_ACCENT_POLICY = 19
			// ...
		}

		[DllImport("user32.dll")]
		internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
		#endregion
		#endregion
	}
}