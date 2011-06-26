using System.IO;
using System.Web;
using System.Web.Configuration;

namespace HebMorph.CorpusSearcher
{
	public static class Settings
	{
		private static string BasePath
		{
			get
			{
				if (baseDataPath == null)
				{
					var webConfig = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
					var docsPathSetting = webConfig.AppSettings.Settings["DataPath"];
					if (docsPathSetting != null)
						baseDataPath = Path.GetFullPath(docsPathSetting.Value);
					else
						return HttpContext.Current.Server.MapPath("~/App_Data"); // don't cache this
				}
				return baseDataPath;
			}
		}

		public static bool AllowIndexing
		{
			get
			{
				var webConfig = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
				var docsPathSetting = webConfig.AppSettings.Settings["AllowIndexing"];
				var ret = false;
				if (docsPathSetting != null)
					ret = bool.TryParse(docsPathSetting.Value, out ret);
				return ret;
			}
		}

		public static string HSepllPath
		{
			get
			{
				return hspellPath ?? (hspellPath = Path.Combine(BasePath, "hspell-data-files"));
			}
		}

		public static string IndexesPath
		{
			get { return indexesPath ?? (indexesPath = Path.Combine(BasePath, "Indexes")); }
		}

		public static string PagesPath
		{
			get { return pagesPath ?? (pagesPath = Path.Combine(BasePath, "Pages")); }
		}

		public static string GoogleAnalyticsAccount
		{
			get
			{
				if (gaAccount == null)
				{
					gaAccount = string.Empty;
					var webConfig = WebConfigurationManager.OpenWebConfiguration("~/Web.config");
					var gaAccountSetting = webConfig.AppSettings.Settings["GAAccount"];
					if (gaAccountSetting != null)
						gaAccount = HttpContext.Current.Server.MapPath(gaAccountSetting.Value);
				}
				return gaAccount;
			}
		}

		private static string baseDataPath;
		private static string hspellPath;
		private static string indexesPath;
		private static string pagesPath;

		private static string gaAccount;
	}
}