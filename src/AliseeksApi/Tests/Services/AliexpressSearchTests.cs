using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AliseeksApi.Services;
using AliseeksApi.Storage.Cache;
using AliseeksApi.Storage.Postgres.Search;
using AliseeksApi.Utility;
using AliseeksApi.Models.Search;
using Moq;

namespace AliseeksApi.Tests.Services
{
    public class AliexpressSearchTests
    {
        AliexpressService service;
        Mock<IApplicationCache> moqCache;
        Mock<ISearchPostgres> moqDb;

        //Different Aliexpress Searches that are tested
        public static IEnumerable<object[]> SearchCriterias
        {
            get
            {
                return new List<object[]>
                {
                    new object[] { new SearchCriteria() { SearchText = "40mm 12V" } },
                    new object[] { new SearchCriteria() { SearchText = "PTFE", Page = 2 } },
                    new object[] { new SearchCriteria() { SearchText = "PTFE", Page = 2, PriceFrom=2 } },
                    new object[] { new SearchCriteria() {  SearchText = "wire"} }
                };
            }
        }

        public AliexpressSearchTests()
        {
            var httpService = new HttpService();
            moqCache = new Mock<IApplicationCache>();

            //Do not let AliexpressService find cached search
            moqCache.Setup(x => x.Exists(It.IsAny<string>())).ReturnsAsync(false);

            moqDb = new Mock<ISearchPostgres>();
            service = new AliexpressService(httpService, moqCache.Object, moqDb.Object);
        }

        [Theory]
        [MemberData(nameof(SearchCriterias))]
        public async Task CanAliexpressSearch(SearchCriteria criteria)
        {
            moqCache.ResetCalls();

            var results = await service.SearchItems(criteria);
            Assert.True(results.Count() != 0, $"Did not return any results for search criteria {criteria.SearchText}");
            moqCache.Verify(x => x.StoreString(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task CanCacheAliexpressSearch()
        {
            var criteria = new SearchCriteria() { SearchText = "40mm 12V" };

            moqCache.ResetCalls();

            await service.CacheItems(criteria);
            moqCache.Verify(x => x.StoreString(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }
    }
}
