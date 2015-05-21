using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ComputerBeacon.Json;

namespace AZPreviewE
{
	public static class Settings
	{
		private static string	m_path;
		private static frmMain	m_form;

		public static void Load(frmMain frm)
		{
			Settings.m_form = frm;
			Settings.m_path = Path.Combine(Program.ExePath, "settings");

			try
			{
				JsonObject jo = new JsonObject(File.ReadAllText(Settings.m_path, Encoding.UTF8));

				Get<int>	(jo, "Left",		e => Settings.m_form.Left = e);
				Get<int>	(jo, "Top",			e => Settings.m_form.Top = e);
				Get<int>	(jo, "Width",		e => Settings.m_form.Width = e);
				Get<int>	(jo, "Height",		e => Settings.m_form.Height = e);
				Get<bool>	(jo, "TopMost",		e => Settings.m_form.TopMost = e);
				Get<string>	(jo, "SavePath",	e => Settings.m_form.sfd.InitialDirectory = e);
			}
			catch
			{
			}
		}

		public static void Save()
		{
			new Task(
				() =>
				File.WriteAllText(
					Settings.m_path, 
					new JsonObject(
						new
						{
							Left		= Settings.m_form.Left,
							Top			= Settings.m_form.Top,
							Width		= Settings.m_form.Width,
							Height		= Settings.m_form.Height,
							TopMost		= Settings.m_form.TopMost,
							SavePath	= Settings.m_form.sfd.InitialDirectory
						}
						).ToString(true)
					, Encoding.UTF8)).Start();
		}

		private static void Get<T>(JsonObject json, string value, Action<T> res)
		{
			object obj;
			if (json.TryGetValue(value, out obj)) res.Invoke((T)obj);
		}
	}
}
