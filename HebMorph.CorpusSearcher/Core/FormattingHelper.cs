using HebMorph.CorpusReaders;

namespace HebMorph.CorpusSearcher.Core
{
	public static class FormattingHelpers
	{
		private static MarkdownSharp.Markdown MarkdownConverter
		{
			get { return _markdownConverter ?? (_markdownConverter = new MarkdownSharp.Markdown()); }
		}
		private static MarkdownSharp.Markdown _markdownConverter;

		public static string AsHtml(this CorpusDocument doc)
		{
			switch (doc.Format)
			{
				case CorpusDocument.ContentFormat.Markdown:
					return MarkdownConverter.Transform(doc.Content);
				case CorpusDocument.ContentFormat.WikiMarkup:
					string redirectToTopic;
					var htmlContent = ScrewTurn.Wiki.Formatter.Format(doc.Title, doc.Content, new {Name = doc.Title, TopicId = doc.Id},
					                                                  true, false, out redirectToTopic);

					// we currently do not support the notion of redirects
					if (htmlContent.StartsWith("Redirected to") || htmlContent.StartsWith("<ol><li>הפניה"))
						return string.Empty;

					// make up for dumb <br> handling by the formatter
					while (htmlContent.IndexOf("<br /><br />") > 0)
						htmlContent = htmlContent.Replace("<br /><br />", "<br />");

					return htmlContent.Trim();
			}
			return doc.Content; // either a fallback or it is already HTML
		}
	}
}