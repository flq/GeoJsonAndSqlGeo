using System.Linq;
using GeoJSON.Net.Geometry;
using GeoJsonAndSqlGeo;
using NUnit.Framework;
using Shouldly;
using Tests.Examples;

namespace Tests
{
    [TestFixture]
    public class SqlGeographyParserTests : TestingContext
    {
      
        [Test]
        public void test_parsing_with_negative_numbers()
        {
            var content = ResourceLoader.LoadSqlType("polygon_w_neg_nums");
            var geoJson = GeoToSql.Translate(content);
            geoJson.ShouldBeOfType<GeometryCollection>();
            ((GeometryCollection)geoJson).Geometries.Count.ShouldBe(3);
            ((GeometryCollection)geoJson).Geometries.ShouldAllBe(go => go is Polygon);
        }
    }
}