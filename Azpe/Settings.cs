using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComputerBeacon.Json;

namespace Azpe
{
	public static class Settings
	{
		private static string	m_path;
		private static FrmMain	m_form;
		
		private static Dictionary<string, string> m_cache = new Dictionary<string,string>();
		private static Random m_rnd = new Random(DateTime.UtcNow.Millisecond);

		public static DateTime NewChecked { get; set; }

		public static string getCacheFileName(string url)
		{
			lock (Settings.m_cache)
			{
				if (m_cache.ContainsValue(url))
				{
					return Settings.m_cache.First(e => e.Value == url).Key;
				}
				else
				{
					string key;
					do
					{
						key = Path.GetRandomFileName();
					} while (Settings.m_cache.ContainsKey(key));
					
					Settings.m_cache.Add(key, url);
					Settings.Save();

					return key;
				}
			}
		}

		public static void Load(FrmMain frm)
		{
			Settings.m_form = frm;
			Settings.m_path = Path.Combine(Program.ExePath, "settings");

			try
			{
				var jo = new JsonObject(File.ReadAllText(Settings.m_path, Encoding.UTF8));

				Get<int>		(jo, "Left",		e => Settings.m_form.Left = e);
				Get<int>		(jo, "Top",			e => Settings.m_form.Top = e);
				Get<int>		(jo, "Width",		e => Settings.m_form.Width = e);
				Get<int>		(jo, "Height",		e => Settings.m_form.Height = e);
				Get<bool>		(jo, "TopMost",		e => Settings.m_form.TopMost = e);
				Get<string>		(jo, "SavePath",	e => Settings.m_form.sfd.InitialDirectory = e);
				Get<string>		(jo, "UpChecked",	e => Settings.NewChecked = DateTime.Parse(e));
				Get<JsonObject>	(jo, "Cache",		e => { foreach (var st in e) Settings.m_cache.Add(st.Key, Convert.ToString(st.Value)); });
			}
			catch
			{
			}

			Settings.ClearCache();
		}
		
		private static void ClearCache()
		{
			var expires = DateTime.UtcNow.AddDays(-7);

			if (Directory.Exists(Program.CacheDir))
			{
				lock (Settings.m_cache)
				{
					foreach (var file in Directory.GetFiles(Program.CacheDir))
					{
						try
						{
							if (File.GetLastAccessTimeUtc(file) <= expires)
								File.Delete(file);
						}
						catch
						{
						}
					}
					
					var fs		= Directory.GetFiles(Program.CacheDir);
					var fsname	= new string[fs.Length];
					var keys	= Settings.m_cache.Keys.ToArray();

					int i = 0;
					for (i = 0; i < fs.Length; ++i)
						fsname[i] = Path.GetFileName(fs[i]);
					
					for (i = 0; i < keys.Length; ++i)
						if (!fsname.Contains(keys[i]))
							Settings.m_cache.Remove(keys[i]);
					
					for (i = 0; i < fs.Length; ++i)
					{
						if (!Settings.m_cache.ContainsKey(fsname[i]))
						{
							try
							{
								File.Delete(fs[i]);
							}
							catch
							{ }
						}
					}

					Settings.Save();
				}
			}
		}

		public static void Save()
		{
			new Task(
				() =>
				{
					var jo = new JsonObject();
					var joCache = new JsonObject();
					
					jo.Add("Left",		Settings.m_form.Left);
					jo.Add("Top",		Settings.m_form.Top);
					jo.Add("Width",		Settings.m_form.Width);
					jo.Add("Height",	Settings.m_form.Height);
					jo.Add("TopMost",	Settings.m_form.TopMost);
					jo.Add("SavePath",	Settings.m_form.sfd.InitialDirectory);
					jo.Add("UpChecked",	Settings.NewChecked.ToString("yyyy-MM-dd HH:mm:ss"));
					jo.Add("Cache",		joCache);

					lock (Settings.m_cache)
					{
						foreach (var st in Settings.m_cache)
							joCache.Add(st.Key, st.Value);
					}

					File.WriteAllText(Settings.m_path, jo.ToString(true), Encoding.UTF8);
				}
				).Start();
		}

		private static void Get<T>(JsonObject json, string value, Action<T> res)
		{
			object obj;
			if (json.TryGetValue(value, out obj)) res.Invoke((T)obj);
		}

	}
}
