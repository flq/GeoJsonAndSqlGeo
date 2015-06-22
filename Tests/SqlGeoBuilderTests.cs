
using System.Data.SqlTypes;
using System.Diagnostics;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using GeoJsonToSqlServer;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Tests.Examples;

namespace Tests
{
    [TestFixture]
    public class SqlGeoBuilderTests : TestingContext
    {
        [Test]
        [TestCase("point")]
        [TestCase("multipoint")]
        [TestCase("linestring")]
        [TestCase("multilinestring")]
        [TestCase("polygon")]
        [TestCase("multipolygon")]
        [TestCase("geometrycollection")]
        public void checking_string_representations_for_sqlgeography(string name)
        {
            var geography = SqlGeographyFromJsonFile(name);
            Debug.WriteLine(geography.ToString());
            AssertIsEqualToRepresentationInFile(geography, name);
        }

        [Test]
        [TestCase("polygon")]
        public void checking_geo_json_representations_of_sqlgeography(string fileName)
        {
            var content = ResourceLoader.LoadSqlType(fileName);
            var geography = SqlGeography.Parse(new SqlString(content));
            Debug.WriteLine(geography);
        }

        private SqlGeography SqlGeographyFromJsonFile(string jsonFile)
        {
            var json = GetObjectFromJson<IGeometryObject>(jsonFile);
            return GeoToSql.Translate((GeoJSONObject) json);
        }
    }
}