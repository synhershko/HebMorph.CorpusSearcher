using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace HebMorph.CorpusSearcher.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

		public ActionResult ExecuteIndexing(string corpusName, string specsString)
		{
			if (!Settings.AllowIndexing || string.IsNullOrEmpty(specsString) || string.IsNullOrEmpty(corpusName))
				return RedirectToAction("Status", "Home");

			var corpusReader = Core.Index.GetCorpusReader(specsString);
			if (corpusReader != null)
				ThreadPool.QueueUserWorkItem(state => Core.Index.Instance.BeginIndexing(corpusReader, corpusName));

			return RedirectToAction("Status", "Home");
		}
    }
}
