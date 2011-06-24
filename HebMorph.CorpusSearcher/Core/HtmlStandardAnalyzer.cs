using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace HebMorph.CorpusSearcher.Core
{
	public class HtmlStandardAnalyzer : StandardAnalyzer
	{
		public HtmlStandardAnalyzer()
			: base(Lucene.Net.Util.Version.LUCENE_29)
		{
		}

		public override Lucene.Net.Analysis.TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
		{
			var htmlCharFilter = new HTMLStripCharFilter(CharReader.Get(reader));
			return base.TokenStream(fieldName, htmlCharFilter);
		}
	}
}