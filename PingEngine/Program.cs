using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Qlik.Engine;

namespace PingEngine
{
	class Program
	{
		private static readonly StreamWriter LogFile = new StreamWriter("Logfile.txt");
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				PrintUsage();
			}

			var url = args[0];
			Log("Connecting to " + url);
			var certs = LoadCertificateFromStore();
			while (true)
			{
				var d0 = DateTime.Now;
				Exception e = null;
				using (var location = Location.FromUri(url))
				{
					location.AsDirectConnection(Environment.UserDomainName, Environment.UserName, true, false, certs);
					try
					{
						using (var hub = location.Hub())
						{
							hub.EngineVersion();
						}
					}
					catch (Exception exception)
					{
						e = exception;
					}
				}

				var d1 = DateTime.Now;
				var dt = d1 - d0;
				if (e == null)
				{
					Log(d1 + " - Connection successfully established. t=" + dt);
				}
				else
				{
					Log(d1 + " - Connection failed: \"" + e.Message.Split(' ').FirstOrDefault() + "\". dt=" + dt);
				}

				Thread.Sleep(5000);
			}
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage:   PingEngine.exe <url>:<port>");
			Console.WriteLine("Example: PingEngine.exe https://my.server.url:4747");
			Environment.Exit(1);
		}

		public static X509Certificate2Collection LoadCertificateFromStore()
		{
			var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadOnly);
			var certificates = store.Certificates.Cast<X509Certificate2>().Where(c => c.FriendlyName == "QlikClient").ToArray();
			store.Close();
			if (certificates.Any())
			{
				Log("Successfully loaded client certificate from store.");
				return new X509Certificate2Collection(certificates);
			}

			Log("Failed to load client certificate from store.");
			Environment.Exit(1);
			return null;
		}

		static void Log(string msg)
		{
			LogFile.WriteLine(msg);
			LogFile.Flush();
			Console.WriteLine(msg);
		}
	}
}
