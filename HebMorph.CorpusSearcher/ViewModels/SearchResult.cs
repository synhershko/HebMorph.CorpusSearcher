using System.Web.Mvc;

namespace HebMorph.CorpusSearcher.ViewModels
{
	public class SearchResult
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public int LuceneDocId { get; set; }
		public float Score { get; set; }
		public MvcHtmlString Fragment { get; set; }
	}
}