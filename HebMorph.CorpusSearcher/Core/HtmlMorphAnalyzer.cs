using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Hebrew;

namespace HebMorph.CorpusSearcher.Core
{
	public class HtmlMorphAnalyzer : MorphAnalyzer
	{
		public HtmlMorphAnalyzer(MorphAnalyzer other) : base(other)
		{
		}

		public HtmlMorphAnalyzer(StreamLemmatizer hml) : base(hml)
		{
		}

		public HtmlMorphAnalyzer(string HSpellDataFilesPath) : base(HSpellDataFilesPath)
		{
		}

		public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
		{
			var filter = new HTMLStripCharFilter(CharReader.Get(reader));
			return base.TokenStream(fieldName, filter);
		}

		public override TokenStream ReusableTokenStream(string fieldName, System.IO.TextReader reader)
		{
			var filter = new HTMLStripCharFilter(CharReader.Get(reader));
			return base.TokenStream(fieldName, filter);
		}
	}
}