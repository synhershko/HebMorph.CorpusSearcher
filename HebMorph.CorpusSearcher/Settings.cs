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
						baseDataPath = HttpContext.Current.Server.MapPath(docsPathSetting.Value);
					else
						return HttpContext.Current.Server.MapPath("~/App_Data"); // don't cache this
				}
				return baseDataPath;
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
			get { return indexesPath ?? (indexesPath = Path.Combine(BasePath, "Pages")); }
		}

		private static string baseDataPath;
		private static string hspellPath;
		private static string indexesPath;
		private static string pagesPath;
	}
}