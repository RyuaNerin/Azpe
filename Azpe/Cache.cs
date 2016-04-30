using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ComputerBeacon.Json;

namespace Azpe
{
	internal static class Cache
	{
		private struct CacheInfo
		{
			public CacheInfo(string url)
			{
				string filename;
				do
				{
					filename = Path.GetFileName(Path.GetRandomFileName()).Replace(".", "");
				} while (Cache.m_cache.Exists(e => e.Name == filename));

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

		private static string m_path;
		private static string m_dir;
		private static List<CacheInfo> m_cache = new List<CacheInfo>();

		public static string CachePath
		{
			get
			{
				return Cache.m_dir;
			}
		}

		public static void Init()
		{
			Cache.m_dir	 = Path.Combine(Program.ExePath, "cache");
			Cache.m_path = Path.Combine(Program.ExePath, "cache.data");

			if (File.Exists(Cache.m_path))
			{
				var jo = new JsonObject(File.ReadAllText(Cache.m_path, Encoding.UTF8));

				if ((int)jo["ver"] == 1)
				{
					lock (Cache.m_cache)
					{
						try
						{
							jo = jo["data"] as JsonObject;

							foreach (var key in jo.Keys)
								Cache.m_cache.Add(new CacheInfo(key, Convert.ToString(jo[key])));
						}
						catch
						{
							Cache.m_cache.Clear();
						}
					}
				}
			}

            Cache.ClearCache();
		}
		private static void ClearCache()
		{
			var expires = DateTime.UtcNow.AddDays(-7);

			if (Directory.Exists(Cache.m_dir))
			{
				lock (Cache.m_cache)
				{
					foreach (var file in Directory.GetFiles(Cache.m_dir))
					{
						string name = Path.GetFileName(file);

						if (File.GetLastAccessTimeUtc(file) <= expires ||
							!Cache.m_cache.Exists(e => e.Name == name) ||
							new FileInfo(file).Length == 0)
						{
							try
							{
								File.Delete(file);
							}
							catch
							{ }

							Cache.Remove(null, Path.GetFileName(file), false);
						}
					}

					int i = 0;
					while (i < Cache.m_cache.Count)
						if (!File.Exists(Path.Combine(Cache.m_dir, Cache.m_cache[i].Name)))
							Cache.m_cache.RemoveAt(i);
						else
							i++;

					Cache.Save();
				}
			}
		}

		private static void Save()
		{
			var jo = new JsonObject();
			var joData = new JsonObject();

			jo.Add("ver",  1);
			jo.Add("data", joData);

			lock (Cache.m_cache)
			{
				for (int i = 0; i < Cache.m_cache.Count; ++i)
				{
					var obj = Cache.m_cache[i];
					joData.Add(obj.Url, obj.Name);
				}
			}

			File.WriteAllText(Cache.m_path, jo.ToString(true), Encoding.UTF8);
		}

		public static string GetCachePath(string url, out string cacheName)
		{
			lock (Cache.m_cache)
			{
				int i = 0;
				while (i < Cache.m_cache.Count)
				{
					if (Cache.m_cache[i].Url == url)
					{
						var cachePath = Path.Combine(Cache.CachePath, Cache.m_cache[i].Name);

						if (File.Exists(cachePath) && new FileInfo(cachePath).Length > 0)
						{
							File.SetLastAccessTimeUtc(cachePath, DateTime.UtcNow);
                            cacheName = Cache.m_cache[i].Name;
							return cachePath;
						}
						else
						{
							try
							{
								File.Delete(cachePath);
							}
							catch
							{ }

							Cache.m_cache.RemoveAt(i);
						};
					}
					else
						++i;
				}

				var info = new CacheInfo(url);
				Cache.m_cache.Add(info);
				Cache.Save();
                cacheName = info.Name;
				return Path.Combine(Cache.CachePath, info.Name);
			}
		}

        public static void SetNewCachePath(string before, string after)
        {
			lock (Cache.m_cache)
			{
				int i = 0;
				while (i < Cache.m_cache.Count)
				{
					if (Cache.m_cache[i].Name == before)
					{
                        var st = Cache.m_cache[i];
                        st.Name = after;
                        Cache.m_cache[i] = st;
                        break;
                    }
                    else
                        ++i;
                }

                Cache.Save();
            }
        }

		public static void Remove(string url, string name, bool save = true)
		{
			lock (Cache.m_cache)
			{
				int i = 0;
				while (i < Cache.m_cache.Count)
					if ((!string.IsNullOrEmpty(url)  && Cache.m_cache[i].Url == url) &&
						(!string.IsNullOrEmpty(name) && Cache.m_cache[i].Name == name))
					{
						try
						{
							File.Delete(Path.Combine(Cache.m_dir, Cache.m_cache[i].Name));
						}
						catch
						{ }

						Cache.m_cache.RemoveAt(i);
					}
					else
						i++;

				if (save)
					Cache.Save();
			}
		}
	}
}
