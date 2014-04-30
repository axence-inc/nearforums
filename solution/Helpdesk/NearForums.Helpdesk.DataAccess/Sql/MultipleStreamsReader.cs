using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NearForums.Helpdesk.DataAccess.Sql
{
    public class MultipleStreamsReader<TSource>
    {
        public MultipleStreamsReader(Expression<Func<TSource, Stream>> streamSelector, TSource[] sources)
        {
            this.streamSelector = streamSelector.Compile();
            this.sourcesEnumerator = sources.GetEnumerator();
        }

        public StringBuilder ReadToEnd()
        {
            StringBuilder sb = new StringBuilder();

            while (this.sourcesEnumerator.MoveNext())
            {
                object currentObject = this.sourcesEnumerator.Current;
                TSource current = (TSource)currentObject;

                using (StreamReader sr = new StreamReader(streamSelector(current)))
                {
                    sb.Append(
                        sr.ReadToEnd());
                }
            }

            return sb;
        }

        private Func<TSource, Stream> streamSelector;
        private System.Collections.IEnumerator sourcesEnumerator;
    }
}
