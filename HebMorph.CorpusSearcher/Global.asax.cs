using System.Web.Mvc;
using System.Web.Routing;
using Raven.Client.Document;

namespace HebMorph.CorpusSearcher
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static DocumentStore RavenDocStore { get; private set; }

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"ContentPages", // Route name
				"Pages/{pageName}", // URL with parameters
				new { controller = "Pages", action = "Index", pageName = UrlParameter.Optional }
				);

			routes.MapRoute(
				"Default", // Route name
				"{action}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);

			routes.MapRoute(
				"LongDefault", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);

			// Open doc-store, but make this optional
			try
			{
				RavenDocStore = new DocumentStore
									{
										ConnectionStringName = "RavenConnStr"
									};
				RavenDocStore.Initialize();
			}
			catch (System.ArgumentException)
			{
				RavenDocStore = null;
			}
		}

		protected void Application_End()
		{
			Core.Index.Instance.CleanUp();
			RavenDocStore.Dispose();
		}
	}
}