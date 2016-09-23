using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Utility
{
    public class SearchEndpoints
    {
        public const string AliexpressSearchUrl = @"http://www.aliexpress.com/wholesale?SortType=price_asc&isUnitPrice=y&g=y&";

        //http://www.dhgate.com/wholesale/search.do?searchkey=40mm+12v&searchSource=minOrderGo&uuid=0709114821&stype=up&sinfo=price&ftype=price&finfo=2%2C4&minorder=100&shipcountry=us&shipcompanies=s4o-sj9-dhl-sd4-sbi-ups-tnt-sao-su4-s8p-s72-sed&isadvanced=0&advancedno=&luceneQuery=&seotype=&thirdattr=&_flush=-1635146333
        public const string DHGateSearchUrl = @"http://www.dhgate.com/wholesale/search.do?searchSource=sort&uuid=1428766536&stype=up&sinfo=price&shipcountry=us&isadvanced=0&advancedno=&luceneQuery=&seotype=&thirdattr=&_flush=-724731264&";
    }
}
