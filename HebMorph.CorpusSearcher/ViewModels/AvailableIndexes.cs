using System.Collections.Generic;
using System.Web.Mvc;

namespace HebMorph.CorpusSearcher.ViewModels
{
	public class AvailableIndexes
	{
		public string CurrentIndexName { get; set; }
		public IEnumerable<SelectListItem> IndexNames
		{
			get { return new SelectList(Core.Index.Instance.AvailableIndexes); }
		}
	}
}