using GeoJsonToSqlServer;
using NUnit.Framework;

namespace Tests
{
    //[TestFixture]
    public class SqlGeographyParserTests
    {
      
        //[Test]
        public void test_multipoint_parsing()
        {
            const string content = "((100 0, 100 1), (100 2))";
            var tree = GeoToSql.ParseTree(content, throwOnError: false);
        }
    }
}