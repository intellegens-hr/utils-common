using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.Tests.SearchTests.Setup
{
    public class SearchTestEntity
    {
        [Key]
        public int Id { get; set; }

        public string TestingSessionId { get; set; }

        public string Text { get; set; }
        public DateTime Date { get; set; }
        public int Numeric { get; set; }
        public decimal Decimal { get; set; }
        public Guid Guid { get; set; }

        [InverseProperty("Parent")]
        public ICollection<SearchTestChildEntity> Children { get;set;}
    }

    public class SearchTestChildEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(SearchTestEntity))]
        public int ParentId { get; set; }

        public string TestingSessionId { get; set; }

        public virtual SearchTestEntity Parent { get; set; }
    }
}