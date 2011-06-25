using System;
using System.Collections.Generic;
using System.Web.Mvc;
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
			        		AvailableIndexes = new SelectList(Core.Index.Instance.AvailableIndexes, corpusName),
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
			ViewBag.Query = new SearchQuery();

			var doc = Core.Index.Instance.GetDocument(corpusName, indexDocId);

			return View(new Document
			            	{
			            		Content = MvcHtmlString.Create(doc.Content),
			            		Id = doc.Id,
			            		Title = MvcHtmlString.Create(doc.Title)
			            	});
		}
	}
}
