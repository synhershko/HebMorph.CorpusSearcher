using System;
using System.Collections.Generic;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search.Vectorhighlight;
using WeightedFragInfo = Lucene.Net.Search.Vectorhighlight.FieldFragList.WeightedFragInfo;

namespace HebMorph.CorpusSearcher.Core
{
	public class CustomFragmentsBuilder : BaseFragmentsBuilder
	{
		public string ContentFieldName { get; protected set; }

		/// <summary>
		/// a constructor.
		/// </summary>
		public CustomFragmentsBuilder()
		{
		}

		public CustomFragmentsBuilder(string contentFieldName)
		{
			ContentFieldName = contentFieldName;
		}

		/// <summary>
		/// a constructor.
		/// </summary>
		/// <param name="preTags">array of pre-tags for markup terms</param>
		/// <param name="postTags">array of post-tags for markup terms</param>
		public CustomFragmentsBuilder(String[] preTags, String[] postTags)
			: base(preTags, postTags)
		{
		}

		public CustomFragmentsBuilder(string contentFieldName, String[] preTags, String[] postTags)
			: base(preTags, postTags)
		{
			ContentFieldName = contentFieldName;
		}

		/// <summary>
		/// do nothing. return the source list.
		/// </summary>
		public override List<WeightedFragInfo> GetWeightedFragInfoList(List<WeightedFragInfo> src)
		{
			return src;
		}

		protected override Field[] GetFields(IndexReader reader, int docId, string fieldName)
		{
			var field = ContentFieldName ?? fieldName;
			var doc = reader.Document(docId, new MapFieldSelector(new[] {field}));
			return doc.GetFields(field); // according to Document class javadoc, this never returns null
		}
	}
}