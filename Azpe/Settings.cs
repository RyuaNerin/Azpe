using System.ComponentModel;
using System.IO;
using System.Text;
using ComputerBeacon.Json;

namespace Azpe
{
	internal static class Settings
	{
		private static string m_path;

		public static void Init()
		{
			Settings.m_path = Path.Combine(Program.ExePath, "settings");

			if (File.Exists(Settings.m_path))
			{
				try
				{
					var jo = new JsonObject(File.ReadAllText(Settings.m_path));
					if ((int)jo["ver"] == 1)
					{
						jo = jo["data"] as JsonObject;

						Settings.Load(jo);
					}
				}
				catch
				{ }
			}
		}

		private static void Load(JsonObject jo)
		{
			if (jo == null) return;

			Settings.Left		= (   int)jo["Left"];
			Settings.Top		= (   int)jo["Top"];
			Settings.Width		= (   int)jo["Width"];
			Settings.Height		= (   int)jo["Height"];
			Settings.SavePath	= (string)jo["SavePath"];
		}

		public static void Save()
		{
			var jo = new JsonObject();
			var joData = new JsonObject();

			jo.Add("ver",	1);
			jo.Add("data",	joData);

			joData.Add("Left",		Settings.Left);
			joData.Add("Top",		Settings.Top);
			joData.Add("Width",		Settings.Width);
			joData.Add("Height",	Settings.Height);
			joData.Add("SavePath",	Settings.SavePath);

			File.WriteAllText(Settings.m_path, jo.ToString(true), Encoding.UTF8);
		}

		[DefaultValue(-1)]
		public static int		Left		{ get; set; }

		[DefaultValue(-1)]
		public static int		Top			{ get; set; }

		[DefaultValue(-1)]
		public static int		Width		{ get; set; }

		[DefaultValue(-1)]
		public static int		Height		{ get; set; }

		[DefaultValue(null)]
		public static string	SavePath	{ get; set; }
	}
}
