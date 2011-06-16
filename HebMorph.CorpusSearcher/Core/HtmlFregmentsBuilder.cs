using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search.Vectorhighlight;
using WeightedFragInfo = Lucene.Net.Search.Vectorhighlight.FieldFragList.WeightedFragInfo;

namespace HebMorph.CorpusSearcher.Core
{
	public class HtmlFragmentsBuilder : BaseFragmentsBuilder
	{
		/// <summary>
        /// a constructor.
        /// </summary>
        public HtmlFragmentsBuilder() : base()
        {
        }

        /// <summary>
        /// a constructor.
        /// </summary>
        /// <param name="preTags">array of pre-tags for markup terms</param>
        /// <param name="postTags">array of post-tags for markup terms</param>
		public HtmlFragmentsBuilder(String[] preTags, String[] postTags)
            : base(preTags, postTags)
        {

        }

		/// <summary>
		/// do nothing. return the source list.
		/// </summary>
		public override List<WeightedFragInfo> GetWeightedFragInfoList(List<WeightedFragInfo> src)
		{
			return src;
		}

		protected override String GetFragmentSource(StringBuilder buffer, int[] index, Field[] values, int startOffset, int endOffset)
		{
			string fieldText;
			while (buffer.Length < endOffset && index[0] < values.Length)
			{
				fieldText = GetFilteredFieldText(values[index[0]]);
				if (index[0] > 0 && values[index[0]].IsTokenized() && fieldText.Length > 0)
					buffer.Append(' ');
				buffer.Append(fieldText);
				++(index[0]);
			}
			var eo = buffer.Length < endOffset ? buffer.Length : endOffset;
			return buffer.ToString().Substring(startOffset, eo - startOffset);
		}

		protected string GetFilteredFieldText(Field field)
		{
			var theStream = new MemoryStream(Encoding.UTF8.GetBytes(field.StringValue()));
            var reader = CharReader.Get(new StreamReader(theStream));
			reader = new HTMLStripCharFilter(reader);

			int r;
			var sb = new StringBuilder();
			while ((r = reader.Read()) != -1)
			{
				sb.Append((char) r);
			}
			return sb.ToString();
		}
	}
}