using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using GeoJsonToSqlServer;
using Irony.Parsing;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
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
        [TestCase("point", typeof(Point))]
        [TestCase("multipoint", typeof(MultiPoint))]
        [TestCase("linestring", typeof(LineString))]
        [TestCase("multilinestring", typeof(MultiLineString))]
        [TestCase("polygon", typeof(Polygon))]
        [TestCase("multipolygon", typeof(MultiPolygon))]
        [TestCase("geometrycollection", typeof(GeometryCollection))]
        public void checking_geo_json_representations_of_sqlgeography(string fileName, Type expectedType)
        {
            var content = ResourceLoader.LoadSqlType(fileName);
            var obj = GeoToSql.Translate(content);
            obj.ShouldBeAssignableTo(expectedType);
            AssertIsEqualToGeoJsonFile(obj, fileName, expectedType);
        }

        private void AssertIsEqualToGeoJsonFile(GeoJSONObject geoJsonObject, string fileName, Type expectedType)
        {
            var serialized = JsonConvert.SerializeObject(geoJsonObject);
            Debug.WriteLine(serialized);
            var json = JsonConvert.SerializeObject(GetObjectFromJson(fileName, expectedType));
            serialized.ShouldBe(json);
        }

        private SqlGeography SqlGeographyFromJsonFile(string jsonFile)
        {
            var json = GetObjectFromJson<IGeometryObject>(jsonFile);
            return GeoToSql.Translate((GeoJSONObject) json);
        }
    }
}