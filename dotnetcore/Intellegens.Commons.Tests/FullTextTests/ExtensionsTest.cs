using Intellegens.Commons.Search.FullTextSearch;
using System.Collections.Generic;
using Xunit;

namespace Intellegens.Commons.Tests.FullTextTests
{
    public class ExtensionsTest
    {
        public class DemoClass001
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public DemoClass001 Test { get; set; }
        }

        public class DemoClass002
        {
            [FullTextSearch]
            public string Name { get; set; }

            public string Title { get; set; }

            [FullTextSearch("Title")]
            public DemoClass002Child Child { get; set; }
        }

        public class DemoClass002Child
        {
            public string Name { get; set; }
            public string Title { get; set; }
        }

        public class DemoClass003
        {
            [FullTextSearch]
            public string Name { get; set; }

            public string Title { get; set; }

            [FullTextSearch]
            public DemoClass003Child Child { get; set; }
        }

        public class DemoClass003Child
        {
            [FullTextSearch]
            public string Name { get; set; }

            public string Title { get; set; }
        }

        public class DemoClass004
        {
            public int Id { get; set; }

            [FullTextSearch]
            public DemoClass003 Demo3_1 { get; set; }

            [FullTextSearch("Title")]
            public DemoClass003 Demo3_2 { get; set; }
        }

        public class DemoClass005
        {
            public int Id { get; set; }

            [FullTextSearch("Child.Title")]
            public DemoClass003 Demo3_1 { get; set; }

            [FullTextSearch("Title")]
            public List<DemoClass003> Demo3_2 { get; set; }
        }

        [Fact]
        public void Full_text_search_ext_should_return_only_string_props_by_default()
        {
            var paths = FullTextSearchExtensions.GetFullTextSearchPaths<DemoClass001>();
            Assert.Equal(2, paths.Count);
            Assert.Contains("Name", paths);
            Assert.Contains("Title", paths);
        }

        [Fact]
        public void Full_text_search_ext_should_return_respect_specified_attribute_paths_1()
        {
            var paths = FullTextSearchExtensions.GetFullTextSearchPaths<DemoClass002>();
            Assert.Equal(2, paths.Count);
            Assert.Contains("Name", paths);
            Assert.Contains("Child.Title", paths);
        }

        [Fact]
        public void Full_text_search_ext_should_return_respect_specified_attribute_paths_2()
        {
            var paths = FullTextSearchExtensions.GetFullTextSearchPaths<DemoClass003>();
            Assert.Equal(2, paths.Count);
            Assert.Contains("Name", paths);
            Assert.Contains("Child.Name", paths);
        }

        [Fact]
        public void Full_text_search_ext_should_return_respect_specified_attribute_paths_3()
        {
            var paths = FullTextSearchExtensions.GetFullTextSearchPaths<DemoClass004>();
            Assert.Equal(3, paths.Count);
            Assert.Contains("Demo3_1.Name", paths);
            Assert.Contains("Demo3_1.Child.Name", paths);
            Assert.Contains("Demo3_2.Title", paths);
        }

        [Fact]
        public void Full_text_search_ext_should_return_respect_specified_attribute_paths_4()
        {
            var paths = FullTextSearchExtensions.GetFullTextSearchPaths<DemoClass005>();
            Assert.Equal(2, paths.Count);
            Assert.Contains("Demo3_1.Child.Title", paths);
            Assert.Contains("Demo3_2.Title", paths);
        }
    }
}