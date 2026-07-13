using System.IO;

namespace dependencyInjection.Logging
{
	internal interface IChatLogger
	{
		void Log(string entry);
	}

	internal class FileChatLogger : IChatLogger
	{
		private readonly string path;

		public FileChatLogger(string path)
		{
			this.path = path;
		}

		public void Log(string entry)
		{
			try
			{
				File.AppendAllText(path, $"{DateTime.Now:HH:mm:ss} {entry}{Environment.NewLine}");
			}
			catch
			{
			}
		}
	}
}
