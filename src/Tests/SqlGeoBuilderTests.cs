using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoJsonAndSqlGeo;
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
        public void special_case_with_circle()
        {
/*            var b = new SqlGeographyBuilder();
            b.SetSrid(4326);
            b.BeginGeography(OpenGisGeographyType.Point);
            b.BeginFigure(47.38905261221537, 6.8939208984375);
            b.EndFigure();
            b.EndGeography();
            var geo = b.ConstructedGeography.BufferWithCurves(9000.0);
            var envCenter = geo.EnvelopeCenter();
            var dou = geo.STStartPoint().STDistance(envCenter);
            Debug.WriteLine(geo.ToString());
            Debug.WriteLine(envCenter.ToString());
            Debug.WriteLine(dou.ToString());*/
            
            var json = GetObjectFromJson<FeatureCollection>("ng_with_circle");
            var geography = GeoToSql.Translate(json);
            AssertIsEqualToRepresentationInFile(geography, "ng_with_circle");
        }

        [Test]
        public void feature_collection_becomes_geometrycollection()
        {
            var json = GetObjectFromJson<FeatureCollection>("ng_earthquakes");
            var geometry = GeoToSql.Translate(json);
            Debug.WriteLine(geometry.ToString());
            AssertIsEqualToRepresentationInFile(geometry, "ng_earthquakes");
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