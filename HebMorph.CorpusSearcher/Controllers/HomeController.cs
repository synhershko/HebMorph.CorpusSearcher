using System;
using System.Web.Mvc;
using HebMorph.CorpusSearcher.ViewModels;

namespace HebMorph.CorpusSearcher.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Query = new SearchQuery
			{
				AvailableIndexes = new SelectList(Core.Index.Instance.AvailableIndexes),
			};

			return View();
		}

		public ActionResult About()
		{
			ViewBag.Query = new SearchQuery
			{
				AvailableIndexes = new SelectList(Core.Index.Instance.AvailableIndexes),
			};

			return View();
		}

		public ActionResult Status()
		{
			ViewBag.Query = new SearchQuery
			{
				AvailableIndexes = new SelectList(Core.Index.Instance.AvailableIndexes),
			};

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

			int totalResults;
			var results = Core.Index.Instance.Search(q, out totalResults);

			ViewBag.TotalResults = totalResults;
			ViewBag.Query = q;
			ViewBag.CorpusName = corpusName;

			return View(results);
		}

		public ActionResult ViewDocument(string corpusName, int indexDocId)
		{
			var a = Request.AcceptTypes;

			ViewBag.Query = new SearchQuery
			{
				AvailableIndexes = new SelectList(Core.Index.Instance.AvailableIndexes),
			};

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
