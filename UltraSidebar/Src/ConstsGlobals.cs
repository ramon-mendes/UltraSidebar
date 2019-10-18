using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Ion;

namespace UltraSidebar
{
	public static partial class Consts
	{
		public static readonly DateTime RELEASE_DATE = new DateTime(2019, 5, 30);
		public const int VersionInt = 0x00010000;
		public const string Version = "1.00";

		public const string AppName = "Sticky Notes";
		//public const EProduct ProductID = EProduct.STICKY_NOTES;

		public static readonly string DirUserData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/MISoftware/" + AppName;
		public static readonly string APP_EXE = Process.GetCurrentProcess().MainModule.FileName;

		static Consts()
		{
			Directory.CreateDirectory(DirUserData);
		}
	}
}