using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using HebMorph.CorpusReaders;
using HebMorph.CorpusSearcher.ViewModels;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Hebrew;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.QueryParsers.Hebrew;
using Lucene.Net.Search;
using Lucene.Net.Search.Vectorhighlight;
using Lucene.Net.Store;
using Newtonsoft.Json.Linq;
using Directory = System.IO.Directory;
using Document = Lucene.Net.Documents.Document;
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;

namespace HebMorph.CorpusSearcher.Core
{
	public class Index
	{
		protected Index()
		{
			PageSize = 15;
			IndexesStoragePath = HttpContext.Current.Server.MapPath("~/App_Data/Indexes");
			HSpellDataFilesPath = HttpContext.Current.Server.MapPath("~/App_Data/hspell-data-files");
			UpdateIndexesList();
		}

		public enum SearchType
		{
			Morphologic,
			AsTyped,
			Simple,
		}

		public static readonly Index Instance = new Index();

		public string IndexesStoragePath { get; protected set; }
		public string HSpellDataFilesPath { get; protected set; }

		protected string[] indexSubFolders;

		public string[] AvailableIndexes { get; set; }

		public void UpdateIndexesList()
		{
			indexSubFolders = Directory.GetDirectories(IndexesStoragePath);
			for (var i=0; i < indexSubFolders.Length;i++)
			{
				var s = indexSubFolders[i];
				indexSubFolders[i] = s.Substring(s.LastIndexOf('\\') + 1);
			}
			AvailableIndexes = indexSubFolders;
		}

		protected MorphAnalyzer MorphAnalyzer
		{
			get
			{
				if (morphAnalyzer == null)
				{
					morphAnalyzer = new MorphAnalyzer(HSpellDataFilesPath);
					var lemmaFilter = new HebMorph.LemmaFilters.ChainedLemmaFilter();
					lemmaFilter.Filters.AddLast(new HebMorph.LemmaFilters.BasicLemmaFilter());
					morphAnalyzer.lemmaFilter = lemmaFilter;
				}
				return morphAnalyzer;
			}
		}
		protected MorphAnalyzer morphAnalyzer;

		protected Analyzer GetAnalyzer(SearchType searchType)
		{
			switch (searchType)
			{
				case SearchType.Morphologic:
					return MorphAnalyzer;
				case SearchType.AsTyped:
					return CreateHebrewSimpleAnalyzer();
				case SearchType.Simple:
					return new SimpleAnalyzer();
			}
			return null;
		}

		public void CleanUp()
		{
			foreach (var searcher in indexSearchers.Values)
			{
				searcher.Close();
			}
			indexSearchers.Clear();
		}

		#region Indexing
		public void BeginIndexing(ICorpusReader corpusReader, string corpusName)
		{
			if (IndexingStatus != null)
				return; // there's already an indexing process running

			// Init
			IndexingStatus = new IndexingProgressInfo
			{
				IndexName = corpusName,
				Percentage = 0,
				Status = "Launching",
				IsRunning = true,
			};

			var indexingAnalyzer = new HtmlMorphAnalyzer(MorphAnalyzer);
			indexingAnalyzer.alwaysSaveMarkedOriginal = true; // to allow for non-morphologic searches too

			var indexPath = Path.Combine(IndexesStoragePath, corpusName);
			var writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(indexPath)), indexingAnalyzer, true,
										 IndexWriter.MaxFieldLength.UNLIMITED);
			writer.SetUseCompoundFile(false);

			corpusReader.OnDocument += corpusDoc =>
			{
				var content = corpusDoc.AsHtml();

				// skip blank documents, they are worthless to us (even though they have a title we could index)
				if (string.IsNullOrEmpty(content))
					return;

				var doc = new Document();
				doc.Add(new Field("Id", corpusDoc.Id, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
				var titleField = new Field("Title", corpusDoc.Title, Field.Store.COMPRESS, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
				titleField.SetBoost(3.0f);
				doc.Add(titleField);
				doc.Add(new Field("Content", content, Field.Store.COMPRESS, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
				writer.AddDocument(doc);
			};

			corpusReader.OnProgress += (percentage, status, isRunning) =>
			{
				IndexingStatus = new IndexingProgressInfo
				{
					IndexName = corpusName,
					Percentage = percentage,
					Status = status,
					IsRunning = isRunning,
				};
			};

			// Execute corpus reading, which will trigger indexing for each document found
			corpusReader.Read();

			// Wrap up, optimize and cleanup
			IndexingStatus = new IndexingProgressInfo
			{
				IndexName = corpusName,
				Percentage = 0,
				Status = "Optimizing index",
				IsRunning = true,
			};

			writer.SetUseCompoundFile(true);
			writer.Optimize();
			writer.Close();

			UpdateIndexesList();

			IndexingStatus = null;
		}

		public class IndexingProgressInfo
		{
			public string IndexName { get; set; }
			public byte Percentage { get; set; }
			public bool IsRunning { get; set; }
			public string Status { get; set; }

			public override string ToString()
			{
				if (IsRunning)
					return string.Format("Indexing corpus {0}, {1} (%{2} done)", IndexName, Status, Percentage);
				
				return string.Format("Finished indexing corpus {0}", IndexName);
			}
		}

		public IndexingProgressInfo IndexingStatus { get; protected set; }

		public static ICorpusReader GetCorpusReader(string specsString)
		{
			var configObject = JObject.Parse(specsString);
			if (configObject["type"].Value<string>() == "wikipedia")
			{
				var reader = new CorpusReaders.Wikipedia.WikiDumpReader(configObject["dumpPath"].Value<string>());
				return reader;
			}
			return null;
		}
		#endregion

		#region Searching
		public IndexSearcher GetSearcher(string indexName)
		{
			IndexSearcher ret;
			if (indexSearchers.TryGetValue(indexName, out ret) && ret != null)
				return ret;

			var indexPath = Path.Combine(IndexesStoragePath, indexName);
			if (!Directory.Exists(indexPath))
				return null;

			ret = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(indexPath)), true);
			ret.SetSimilarity(new BinaryCoordSimilarity()); // to boost docs containing all terms, to overcome issues
																// concerning many terms in index produced by HebMorph

			if (!indexSearchers.TryAdd(indexName, ret))
			{
				ret.Close();
				return null;
			}

			return ret;
		}

		protected readonly ConcurrentDictionary<string, IndexSearcher> indexSearchers =
			new ConcurrentDictionary<string, IndexSearcher>();

		public static Query ParseIntoDMQ(string query, string[] fields, Analyzer analyzer)
		{
			var dmQuery = new DisjunctionMaxQuery(0.0f);
			foreach (var t in fields)
			{
				QueryParser qp = new HebrewQueryParser(Lucene.Net.Util.Version.LUCENE_29, t, analyzer);
				var q = qp.Parse(query);
				if (q != null && (!(q is BooleanQuery) || ((BooleanQuery)q).GetClauses().Length > 0))
					dmQuery.Add(q);
			}
			return dmQuery;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected static Lucene.Net.Analysis.Hebrew.SimpleAnalyzer CreateHebrewSimpleAnalyzer()
		{
			var an = new Lucene.Net.Analysis.Hebrew.SimpleAnalyzer();
			an.RegisterSuffix(HebrewTokenizer.TokenTypeSignature(HebrewTokenizer.TOKEN_TYPES.Hebrew), "$");
			an.RegisterSuffix(HebrewTokenizer.TokenTypeSignature(HebrewTokenizer.TOKEN_TYPES.Acronym), "$");
			an.RegisterSuffix(HebrewTokenizer.TokenTypeSignature(HebrewTokenizer.TOKEN_TYPES.Construct), "$");
			return an;
		}

		private static readonly string[] searchFields = new [] {"Title", "Content"};
		private static readonly BooleanClause.Occur[] searchFlags = new[] {BooleanClause.Occur.SHOULD, BooleanClause.Occur.SHOULD};

		public byte PageSize { get; set; }

		public IEnumerable<SearchResult> Search(SearchQuery searchQuery, out int totalHitCount)
		{
			var searcher = GetSearcher(searchQuery.IndexName);
			if (searcher == null)
				throw new ArgumentException("Index not found: " + searchQuery.IndexName);

			// Init
			var fvh = new FastVectorHighlighter(FastVectorHighlighter.DEFAULT_PHRASE_HIGHLIGHT,
												FastVectorHighlighter.DEFAULT_FIELD_MATCH,
												new SimpleFragListBuilder(), new HtmlFragmentsBuilder());

			var query = HebrewMultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_29, searchQuery.Query,
			                                              searchFields, searchFlags,
			                                              GetAnalyzer(searchQuery.SearchType));

			// Perform actual search
			var tsdc = TopScoreDocCollector.create(PageSize * searchQuery.CurrentPage, true);
			searcher.Search(query, tsdc);
			totalHitCount = tsdc.GetTotalHits();
			var hits = tsdc.TopDocs().scoreDocs;

			var ret = new List<SearchResult>(PageSize);

			int i;
			for (i = (searchQuery.CurrentPage - 1) * PageSize; i < hits.Length; ++i)
			{
				var d = searcher.Doc(hits[i].doc);
				var fq = fvh.GetFieldQuery(query);
				var fragment = fvh.GetBestFragment(fq, searcher.GetIndexReader(), hits[i].doc, "Content", 300);

				ret.Add(new SearchResult
				        	{
				        		Id = d.Get("Id"),
				        		Title = d.Get("Title"),
				        		Score = hits[i].score,
				        		LuceneDocId = hits[i].doc,
								Fragment = MvcHtmlString.Create(fragment),
				        	});
			}
			return ret;
		}
		#endregion

		public CorpusDocument GetDocument(string indexName, int indexDocumentId)
		{
			var searcher = GetSearcher(indexName);
			if (searcher == null)
				throw new ArgumentException("Index not found: " + indexName);

			var doc = searcher.Doc(indexDocumentId);
			var ret = new CorpusDocument
			          	{
			          		Id = doc.Get("Id"),
			          		Title = doc.Get("Title")
			          	};
			ret.SetContent(doc.Get("Content"), CorpusDocument.ContentFormat.Html);
			return ret;
		}
	}
}