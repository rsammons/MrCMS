using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Util;
using MrCMS.Indexing.Utils;
using MrCMS.Search;
using MrCMS.Search.Models;
using MrCMS.Web.Areas.Admin.Models.Search;

namespace MrCMS.Web.Areas.Admin.Services
{
    public interface IUniversalSearchIndexSearcher
    {
        List<UniversalSearchItemQuickSearch> QuickSearch(string term);
    }

    public class UniversalSearchIndexSearcher : IUniversalSearchIndexSearcher
    {
        private readonly IUniversalSearchIndexManager _universalSearchIndexManager;
        private readonly ISearchConverter _searchConverter;

        public UniversalSearchIndexSearcher(IUniversalSearchIndexManager universalSearchIndexManager, ISearchConverter searchConverter)
        {
            _universalSearchIndexManager = universalSearchIndexManager;
            _searchConverter = searchConverter;
        }

        public List<UniversalSearchItemQuickSearch> QuickSearch(string term)
        {
            using (var searcher = _universalSearchIndexManager.GetSearcher())
            {
                var analyser = new StandardAnalyzer(Version.LUCENE_30);
                var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                    new[] {UniversalSearchFieldNames.SearchTerms}, analyser);
                var topDocs = searcher.Search(term.SafeGetSearchQuery(parser, analyser), 10);

                var universalSearchItems =
                    topDocs.ScoreDocs.Select(doc => _searchConverter.Convert(searcher.Doc(doc.Doc))).ToList();

                return universalSearchItems.Select(item => new UniversalSearchItemQuickSearch(item)).ToList();
            }
        }
    }

    public class UniversalSearchItemQuickSearch 
    {
        private readonly UniversalSearchItem _item;

        public UniversalSearchItemQuickSearch(UniversalSearchItem item)
        {
            _item = item;
        }

        public string id { get { return _item.SearchGuid.ToString(); } }
        public string value { get { return _item.DisplayName; } }
        public string url { get { return _item.EditUrl; } }
    }
}