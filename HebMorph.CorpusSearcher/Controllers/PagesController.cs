using System.Web.Mvc;

namespace HebMorph.CorpusSearcher.Controllers
{
    public class PagesController : Controller
    {
        public ActionResult Index(string pageName)
        {
        	var doc = Helpers.ContentPagesHelper.GetContentPage(pageName);
			if (doc == null)
				return new HttpNotFoundResult("The requested page does not exist");

            return View(doc);
        }
    }
}
