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
		
		private struct CacheInfo
		{
			public CacheInfo(string url)
			{
				string filename;
				do
				{
					filename = Path.GetRandomFileName();
				} while (Settings.m_cache.Exists(e => e.Name == filename));

				this.Url	= url;
				this.Name	= filename;
			}
			public CacheInfo(string url, string filename)
			{
				this.Url	= url;
				this.Name	= filename;
			}
			public string Url;
			public string Name;
		}

		private static List<CacheInfo>	m_cache	= new List<CacheInfo>();
		private static Random			m_rnd	= new Random(DateTime.UtcNow.Millisecond);

		public static DateTime NewChecked { get; set; }

		public static string getCacheFileName(string url)
		{
			lock (Settings.m_cache)
			{
				for (int i = 0; i < Settings.m_cache.Count; ++i)
					if (Settings.m_cache[i].Url == url)
						return Settings.m_cache[i].Name;

				var info = new CacheInfo(url);
				Settings.m_cache.Add(info);
				Settings.Save();
				return info.Name;
			}
		}

		public static void DelCacheFile(string url, string name, bool save = true)
		{
			lock (Settings.m_cache)
			{
				int i = 0;
				while (i < Settings.m_cache.Count)
					if ((!string.IsNullOrEmpty(url)  && Settings.m_cache[i].Url == url) &&
						(!string.IsNullOrEmpty(name) && Settings.m_cache[i].Name == name))
						Settings.m_cache.RemoveAt(i);
					else
						i++;

				if (save)
					Settings.Save();
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
				Get<JsonObject>	(jo, "Cache",		e => { foreach (var st in e) Settings.m_cache.Add(new CacheInfo(Convert.ToString(st.Value), st.Key)); });
				Get<JsonObject>	(jo, "Cache2",		e => { foreach (var st in e) Settings.m_cache.Add(new CacheInfo(st.Key, Convert.ToString(st.Value))); });
			}
			catch
			{
			}

			Settings.ClearCache();
		}

		private static void Get<T>(JsonObject json, string value, Action<T> res)
		{
			object obj;
			if (json.TryGetValue(value, out obj)) res.Invoke((T)obj);
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
						string name = Path.GetFileName(file);

						if (File.GetLastAccessTimeUtc(file) <= expires ||
							!Settings.m_cache.Exists(e => e.Name == name) ||
							new FileInfo(file).Length == 0)
						{
							try
							{
								File.Delete(file);
							}
							catch
							{
							}
							Settings.DelCacheFile(null, Path.GetFileName(file), false);
						}
					}

					int i = 0;
					while (i < Settings.m_cache.Count)
						if (!File.Exists(Path.Combine(Program.CacheDir, Settings.m_cache[i].Name)))
							Settings.m_cache.RemoveAt(i);
						else
							i++;

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

					jo.Add("Left", Settings.m_form.Left);
					jo.Add("Top", Settings.m_form.Top);
					jo.Add("Width", Settings.m_form.Width);
					jo.Add("Height", Settings.m_form.Height);
					jo.Add("TopMost", Settings.m_form.TopMost);
					jo.Add("SavePath", Settings.m_form.sfd.InitialDirectory);
					jo.Add("UpChecked", Settings.NewChecked.ToString("yyyy-MM-dd HH:mm:ss"));
					jo.Add("Cache2", joCache);

					lock (Settings.m_cache)
						Settings.m_cache.ForEach(e => joCache.Add(e.Url, e.Name));

					File.WriteAllText(Settings.m_path, jo.ToString(true), Encoding.UTF8);
				}
				).Start();
		}
	}
}
