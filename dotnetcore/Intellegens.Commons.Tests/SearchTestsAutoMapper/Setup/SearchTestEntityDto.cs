using System;
using System.Collections.Generic;

namespace Intellegens.Commons.Tests.SearchTestsAutoMapper.Setup
{
    public class SearchTestEntityDto
    {
        public int Id { get; set; }
        public string TestingSessionId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public int Numeric { get; set; }
        public decimal Decimal { get; set; }
        public Guid Guid { get; set; }

        public int? SiblingId { get; set; }

        public SearchTestEntityDto? SiblingDto { get; set; }

        public ICollection<SearchTestChildEntityDto> ChildrenDtos { get; set; } = new List<SearchTestChildEntityDto>();
    }

    public class SearchTestChildEntityDto
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string TestingSessionId { get; set; }
        public SearchTestEntityDto ParentDto { get; set; }
    }
}