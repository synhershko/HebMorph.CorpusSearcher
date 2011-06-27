using System.Web.Mvc;
using HebMorph.CorpusSearcher.ViewModels;

namespace HebMorph.CorpusSearcher.Helpers
{
	public static class DocumentHelpers
	{
		public static Document ToViewDocument(this CorpusReaders.CorpusDocument doc)
		{
			return new Document
				{
					Content = MvcHtmlString.Create(doc.Content),
					Id = doc.Id,
					Title = MvcHtmlString.Create(doc.Title)
				};
		}
	}
}