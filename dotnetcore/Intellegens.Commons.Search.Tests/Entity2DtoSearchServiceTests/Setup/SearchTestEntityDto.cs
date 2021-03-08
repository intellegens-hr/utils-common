using Intellegens.Commons.Search.FullTextSearch;
using System;
using System.Collections.Generic;

namespace Intellegens.Commons.Search.Tests.Entity2DtoSearchServiceTests.Setup
{
    public class SearchTestEntityDto
    {
        public int Id { get; set; }

        [FullTextSearch]
        public string TestingSessionId { get; set; }

        [FullTextSearch]
        public string Text { get; set; }

        public DateTime Date { get; set; }
        public int Numeric { get; set; }
        public decimal Decimal { get; set; }
        public Guid Guid { get; set; }

        public int? SiblingId { get; set; }

        [FullTextSearch("Text")]
        public SearchTestEntityDto? SiblingDto { get; set; }

        [FullTextSearch("Text")]
        public ICollection<SearchTestChildEntityDto> ChildrenDtos { get; set; } = new List<SearchTestChildEntityDto>();
    }

    public class SearchTestChildEntityDto
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string TestingSessionId { get; set; }

        [FullTextSearch("Text")]
        public string Text { get; set; }

        public SearchTestEntityDto ParentDto { get; set; }
    }
}