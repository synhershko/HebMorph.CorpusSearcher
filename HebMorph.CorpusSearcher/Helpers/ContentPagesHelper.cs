using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HebMorph.CorpusSearcher.ViewModels;

namespace HebMorph.CorpusSearcher.Helpers
{
	public static class ContentPagesHelper
	{
		public static Document GetContentPage(string pageId)
		{
			pageId = pageId.Replace(Path.PathSeparator, ' ')
				.Replace(Path.VolumeSeparatorChar, ' ')
				.Replace(Path.AltDirectorySeparatorChar, ' ')
				.Replace(Path.DirectorySeparatorChar, ' ');

			var path = Path.Combine(Settings.PagesPath, pageId + ".markdown");
			if (!File.Exists(path))
				return null;

			var contents = File.ReadAllText(path);
			var markdown = new MarkdownSharp.Markdown();
			return new Document
					{
						Content = MvcHtmlString.Create(markdown.Transform(contents)),
						Id = pageId,
						Title = MvcHtmlString.Create(pageId),
					};
		}
	}
}