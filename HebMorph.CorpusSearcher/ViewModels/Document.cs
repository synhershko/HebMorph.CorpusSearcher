using System.Web.Mvc;

namespace HebMorph.CorpusSearcher.ViewModels
{
	public class Document
	{
		public string Id { get; set; }
		public MvcHtmlString Title { get; set; }
		public MvcHtmlString Content { get; set; }
	}
}