#if WINDOWS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UltraSidebar.Native
{
	class GlobalHotkeys : IDisposable
	{
		[DllImport("user32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32", SetLastError = true)]
		public static extern int UnregisterHotKey(IntPtr hwnd, int id);
		[DllImport("kernel32", SetLastError = true)]
		public static extern short GlobalAddAtom(string lpString);
		[DllImport("kernel32", SetLastError = true)]
		public static extern short GlobalDeleteAtom(short nAtom);

		public const int MOD_ALT = 1;
		public const int MOD_CONTROL = 2;
		public const int MOD_SHIFT = 4;
		public const int MOD_WIN = 8;
		public const int MOD_NOREPEAT = 0x4000;

		public const int WM_HOTKEY = 0x312;

		private static int _hotkey_count = 0;

		/// <summary>Handle of window</summary>
		public IntPtr Handle;

		/// <summary>The ID for the hotkey</summary>
		public short HotkeyID { get; private set; }

		/// <summary>Register the hotkey</summary>
		public void RegisterGlobalHotKey(int hotkey, int modifiers, IntPtr handle)
		{
			UnregisterGlobalHotKey();

			try
			{
				Handle = handle;

				// use the GlobalAddAtom API to get a unique ID (as suggested by MSDN)
				string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + GetType().FullName + _hotkey_count;
				HotkeyID = GlobalAddAtom(atomName);
				HotkeyID--;
				if(HotkeyID == 0)
					throw new Exception("Unable to generate unique hotkey ID. Error: " + Marshal.GetLastWin32Error().ToString());

				// register the hotkey, throw if any error
				if(!RegisterHotKey(Handle, HotkeyID, (uint)modifiers, (uint)hotkey))
				{
					var msg = new Win32Exception(Marshal.GetLastWin32Error()).Message;
					throw new Exception("Unable to register hotkey. Error: " + msg);
				}

				_hotkey_count++;
			}
			catch(Exception ex)
			{
				Debug.Assert(false);

				// clean up if hotkey registration failed
				Dispose();
				Console.WriteLine(ex);
			}
		}

		/// <summary>Unregister the hotkey</summary>
		public void UnregisterGlobalHotKey()
		{
			if(HotkeyID != 0)
			{
				UnregisterHotKey(Handle, HotkeyID);
				// clean up the atom list
				GlobalDeleteAtom(HotkeyID);
				HotkeyID = 0;
			}
		}

		public void Dispose()
		{
			UnregisterGlobalHotKey();
		}
	}
}
#endif
