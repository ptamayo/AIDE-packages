using Aide.Core.Data;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aide.Core.UnitTests.Data
{
    [TestFixture]
    public class EfRepositoryTests
    {
        private Mock<MockedDbContext> _context;

        [SetUp]
        public void Setup()
        {
            _context = new Mock<MockedDbContext>();
        }

        /// <summary>
        /// Setup the context.
        /// Need call this in scenarios that will query the database.
        /// IMPORTANT: The entity initialization must happen before calling this.
        /// </summary>
        private void SetupContext()
        {
            // For passing the DbSet from context to EfRepository
            _context.Setup(x => x.Set<testx>()).Returns(_context.Object.test);
        }

        #region Paginate

        [Test]
        public void Paginate_WhenValidIEnumerableAndPagingSettingsProvided_ThenReturnPagedResult()
        {
            #region Arrange

            var dtos = new List<Testx>
            {
                new Testx
                {
                    Id = 1,
                    StringProperty = "String Property 1"
                },
                new Testx
                {
                    Id = 2,
                    StringProperty = "String Property 2"
                },
                new Testx
                {
                    Id = 3,
                    StringProperty = "String Property 3"
                },
                new Testx
                {
                    Id = 4,
                    StringProperty = "String Property 4"
                },
                new Testx
                {
                    Id = 5,
                    StringProperty = "String Property 5"
                }
            };
            var pagingSettings = new PagingSettings
            {
                PageSize = 2,
                PageNumber = 2
            };

            #endregion

            #region Act

            var page = EfRepository<Testx>.Paginate(pagingSettings, dtos);

            #endregion

            #region Assert

            // Verify the resulting page is the one requested
            Assert.AreEqual(page.CurrentPage, pagingSettings.PageNumber);
            // Verify the size of the page is correct
            Assert.AreEqual(page.PageSize, pagingSettings.PageSize);
            // Verify the page results is not null
            Assert.IsNotNull(page.Results);
            // Verify the page results have items
            Assert.IsTrue(page.Results.Any());
            // Verify the number of page results is correct
            Assert.AreEqual(page.Results.Count(), pagingSettings.PageSize);
            // Verify the total number of pages is correct
            var expectedPageCount = Math.Ceiling(Convert.ToDecimal(dtos.Count) / pagingSettings.PageSize);
            Assert.AreEqual(page.PageCount, expectedPageCount);
            // Verify the total count of items for all pages is correct
            Assert.AreEqual(page.RowCount, dtos.Count);

            #endregion
        }

        [Test]
        public void Paginate_WhenValidIEnumerableAndPagingSettingsAndSortParamsProvided_ThenReturnPagedResultOrdered()
        {
            #region Arrange

            var dtos = new List<Testx>
            {
                new Testx
                {
                    Id = 1,
                    StringProperty = "String Property 1"
                },
                new Testx
                {
                    Id = 2,
                    StringProperty = "String Property 2"
                },
                new Testx
                {
                    Id = 3,
                    StringProperty = "String Property 3"
                },
                new Testx
                {
                    Id = 4,
                    StringProperty = "String Property 4"
                },
                new Testx
                {
                    Id = 5,
                    StringProperty = "String Property 5"
                }
            };
            var pagingSettings = new PagingSettings
            {
                PageSize = 5,
                PageNumber = 1,
                SortBy = new string[] { "StringProperty desc" }
            };

            #endregion

            #region Act

            var page = EfRepository<Testx>.Paginate(pagingSettings, dtos);

            #endregion

            #region Assert

            // Verify the resulting page is the one requested
            Assert.AreEqual(page.CurrentPage, pagingSettings.PageNumber);
            // Verify the size of the page is correct
            Assert.AreEqual(page.PageSize, pagingSettings.PageSize);
            // Verify the page results is not null
            Assert.IsNotNull(page.Results);
            // Verify the page results have items
            Assert.IsTrue(page.Results.Any());
            // Verify the number of page results is correct
            Assert.AreEqual(page.Results.Count(), pagingSettings.PageSize);
            // Verify the total number of pages is correct
            var expectedPageCount = Math.Ceiling(Convert.ToDecimal(dtos.Count) / pagingSettings.PageSize);
            Assert.AreEqual(page.PageCount, expectedPageCount);
            // Verify the total count of items for all pages is correct
            Assert.AreEqual(page.RowCount, dtos.Count);
            // Verify the first item in the result is correct
            Assert.IsTrue(page.Results.First().Id == dtos.Last().Id);
            // Verify the last item in the result is correct
            Assert.IsTrue(page.Results.Last().Id == dtos.First().Id);

            #endregion
        }

        [Test]
        public async Task Paginate_WhenValidIQueryableAndPagingSettings_ThenReturnPagedResult()
        {
            #region Arrange

            var entities = new List<testx>
            {
                new testx
                {
                    id = 1,
                    string_property = "String Property 1"
                },
                new testx
                {
                    id = 2,
                    string_property = "String Property 2"
                },
                new testx
                {
                    id = 3,
                    string_property = "String Property 3"
                },
                new testx
                {
                    id = 4,
                    string_property = "String Property 4"
                },
                new testx
                {
                    id = 5,
                    string_property = "String Property 5"
                }
            };
            var query = entities.AsQueryable().BuildMock();
            var pagingSettings = new PagingSettings
            {
                PageSize = 2,
                PageNumber = 2
            };

            // Initialize DbSet in context
            _context.Setup(x => x.test).ReturnsDbSet(entities);
            this.SetupContext();
            #endregion

            #region Act

            var page = await EfRepository<testx>.PaginateAsync(pagingSettings, query);

            #endregion

            #region Assert

            // Verify the resulting page is the one requested
            Assert.AreEqual(page.CurrentPage, pagingSettings.PageNumber);
            // Verify the size of the page is correct
            Assert.AreEqual(page.PageSize, pagingSettings.PageSize);
            // Verify the page results is not null
            Assert.IsNotNull(page.Results);
            // Verify the page results have items
            Assert.IsTrue(page.Results.Any());
            // Verify the number of page results is correct
            Assert.AreEqual(page.Results.Count(), pagingSettings.PageSize);
            // Verify the total number of pages is correct
            var expectedPageCount = Math.Ceiling(Convert.ToDecimal(entities.Count) / pagingSettings.PageSize);
            Assert.AreEqual(page.PageCount, expectedPageCount);
            // Verify the total count of items for all pages is correct
            Assert.AreEqual(page.RowCount, entities.Count);

            #endregion
        }

        [Test]
        public async Task Paginate_WhenValidIQueryableAndPagingSettingsAndAndSortParamsProvided_ThenReturnPagedResultOrdered()
        {
            #region Arrange

            var entities = new List<testx>
            {
                new testx
                {
                    id = 1,
                    string_property = "String Property 1"
                },
                new testx
                {
                    id = 2,
                    string_property = "String Property 2"
                },
                new testx
                {
                    id = 3,
                    string_property = "String Property 3"
                },
                new testx
                {
                    id = 4,
                    string_property = "String Property 4"
                },
                new testx
                {
                    id = 5,
                    string_property = "String Property 5"
                }
            };
            var query = entities.AsQueryable().BuildMock();
            var pagingSettings = new PagingSettings
            {
                PageSize = 5,
                PageNumber = 1,
                SortBy = new string[] { "string_property desc" }
            };

            // Initialize DbSet in context
            _context.Setup(x => x.test).ReturnsDbSet(entities);
            this.SetupContext();
            #endregion

            #region Act

            var page = await EfRepository<testx>.PaginateAsync(pagingSettings, query);

            #endregion

            #region Assert

            // Verify the resulting page is the one requested
            Assert.AreEqual(page.CurrentPage, pagingSettings.PageNumber);
            // Verify the size of the page is correct
            Assert.AreEqual(page.PageSize, pagingSettings.PageSize);
            // Verify the page results is not null
            Assert.IsNotNull(page.Results);
            // Verify the page results have items
            Assert.IsTrue(page.Results.Any());
            // Verify the number of page results is correct
            Assert.AreEqual(page.Results.Count(), pagingSettings.PageSize);
            // Verify the total number of pages is correct
            var expectedPageCount = Math.Ceiling(Convert.ToDecimal(entities.Count) / pagingSettings.PageSize);
            Assert.AreEqual(page.PageCount, expectedPageCount);
            // Verify the total count of items for all pages is correct
            Assert.AreEqual(page.RowCount, entities.Count);
            // Verify the first item in the result is correct
            Assert.IsTrue(page.Results.First().id == entities.Last().id);
            // Verify the last item in the result is correct
            Assert.IsTrue(page.Results.Last().id == entities.First().id);

            #endregion
        }

        #endregion

        #region ApplySort

        [Test]
        public void ApplySort_WhenValidIQueryableAndSortParamsProvided_ThenReturnSortedResult()
        {
            #region Arrange

            var orderParams = new string[] { "string_property desc" };
            var entities = new List<testx>
            {
                new testx
                {
                    id = 1,
                    string_property = "String Property 1"
                },
                new testx
                {
                    id = 2,
                    string_property = "String Property 2"
                },
                new testx
                {
                    id = 3,
                    string_property = "String Property 3"
                },
                new testx
                {
                    id = 4,
                    string_property = "String Property 4"
                },
                new testx
                {
                    id = 5,
                    string_property = "String Property 5"
                }
            };
            var query = entities.AsQueryable().BuildMock();

            // Initialize DbSet in context
            _context.Setup(x => x.test).ReturnsDbSet(entities);
            this.SetupContext();

            #endregion

            #region Act

            var queryOutput = EfRepository<testx>.ApplySort(query, orderParams);

            #endregion

            #region Assert

            var results = queryOutput.ToList();

            // Verify the page results is not null
            Assert.IsNotNull(results);
            // Verify the page results have items
            Assert.IsTrue(results.Any());
            // Verify the number of page results is correct
            Assert.AreEqual(entities.Count(), results.Count());
            // Verify the first item in the result is correct
            Assert.IsTrue(results.First().id == entities.Last().id);
            // Verify the last item in the result is correct
            Assert.IsTrue(results.Last().id == entities.First().id);

            #endregion
        }

        [Test]
        [TestCase("null")]
        [TestCase("empty")]
        public void ApplySort_WhenValidIQueryableAndNoSortParamsProvided_ThenReturnUnorderedResult(string inputParams)
        {
            #region Arrange

            string[] orderParams = null;
            switch (inputParams)
            {
                case "empty":
                    orderParams = new string[] { };
                    break;
            }

            var entities = new List<testx>
            {
                new testx
                {
                    id = 1,
                    string_property = "String Property 1"
                },
                new testx
                {
                    id = 2,
                    string_property = "String Property 2"
                },
                new testx
                {
                    id = 3,
                    string_property = "String Property 3"
                },
                new testx
                {
                    id = 4,
                    string_property = "String Property 4"
                },
                new testx
                {
                    id = 5,
                    string_property = "String Property 5"
                }
            };
            var query = entities.AsQueryable().BuildMock();

            // Initialize DbSet in context
            _context.Setup(x => x.test).ReturnsDbSet(entities);
            this.SetupContext();
            #endregion

            #region Act

            var queryOutput = EfRepository<testx>.ApplySort(query, orderParams);

            #endregion

            #region Assert

            var results = queryOutput.ToList();

            // Verify the page results is not null
            Assert.IsNotNull(results);
            // Verify the page results have items
            Assert.IsTrue(results.Any());
            // Verify the number of page results is correct
            Assert.AreEqual(entities.Count(), results.Count());
            // Verify the first item in the result is correct
            Assert.IsTrue(results[0].id == entities[0].id);
            // Verify the last item in the result is correct
            Assert.IsTrue(results[results.Count() - 1].id == entities[entities.Count() - 1].id);

            #endregion
        }

        #endregion

        #region Local Classes

        public class Testx
        {
            public int Id { get; set; }
            public string StringProperty { get; set; }
        }

        public class testx
        {
            public int id { get; set; }
            public string string_property { get; set; }
        }

        public partial class MockedDbContext : DbContext
        {
            public virtual DbSet<testx> test { get; set; }
        }

        #endregion
    }
}
