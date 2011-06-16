using System.Web.Mvc;
using HebMorph.CorpusSearcher.Core;
using Lucene.Net.QueryParsers;

namespace HebMorph.CorpusSearcher.ViewModels
{
	public class SearchQuery
	{
		public SearchQuery()
		{
			SearchType = Index.SearchType.Morphologic;
		}

		public string Query { get; set; }
		public string IndexName { get; set; }
		public QueryParser.Operator DefaultOperator { get; set; }
		public Index.SearchType SearchType { get; set; }

		public int CurrentPage { get; set; }

		public SelectList AvailableIndexes { get; set; }

		public string GetSearchUrl()
		{
			return string.Format("/Search?corpusName={0}&query={1}&searchType={2}&page=", IndexName, Query, SearchType);
		}
	}
}