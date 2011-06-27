using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using HebMorph.CorpusSearcher.Helpers;
using HebMorph.CorpusSearcher.ViewModels;
using Lucene.Net.QueryParsers;

namespace HebMorph.CorpusSearcher.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Query = new SearchQuery();

			return View();
		}

		public ActionResult Status()
		{
			ViewBag.Query = new SearchQuery();

			var curStatus = Core.Index.Instance.IndexingStatus;
			ViewBag.IndexingStatus = curStatus != null ? curStatus.ToString() : "Idle";

			return View();
		}

		public ActionResult Search(string query, string corpusName, int? page, string searchType)
		{
			var pageNumber = page ?? 1;
			if (pageNumber <= 0)
				pageNumber = 1;

			Core.Index.SearchType type;
			if (!Enum.TryParse(searchType, true, out type))
				type = Core.Index.SearchType.Morphologic;

			ViewBag.CurrentIndexName = corpusName;
			var q = new SearchQuery
			        	{
			        		IndexName = corpusName,
			        		Query = query,
							CurrentPage = pageNumber,
							SearchType = type,
			        	};

			int totalResults = 0;
			IEnumerable<SearchResult> results = null;
			try
			{
				results = Core.Index.Instance.Search(q, out totalResults);
			}
			catch (ParseException ex)
			{
				ViewBag.ErrorMessage = "Error: " + ex.Message;
			}

			ViewBag.TotalResults = totalResults;
			ViewBag.Query = q;
			ViewBag.CorpusName = corpusName;

			return View(results ?? new List<SearchResult>());
		}

		public ActionResult ViewDocument(string corpusName, int indexDocId)
		{
			var doc = Core.Index.Instance.GetDocument(corpusName, indexDocId);
			return View(doc.ToViewDocument());
		}

		public ActionResult MoreLikeThis(string corpusName, int indexDocId)
		{
			var docs = Core.Index.Instance.GetMoreLikeThis(corpusName, indexDocId, 10);
			return View(docs.Select(x => x.ToViewDocument()));
		}
	}
}
