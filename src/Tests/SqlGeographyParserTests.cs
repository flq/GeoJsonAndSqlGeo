using System;
using System.Diagnostics;
using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
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
        [SetUp]
        public void SetUp()
        {
            GeoToSql.Reset();
            GeoToSql.Configure(cfg => cfg.SetReferenceSystem((int)SpatialReferenceSystem.WorldGeodetic1984));
        }

        [Test]
        public void test_parsing_with_negative_numbers()
        {
            var content = ResourceLoader.LoadSqlType("polygon_w_neg_nums");
            var geoJson = GeoToSql.Translate(content);
            geoJson.ShouldBeOfType<GeometryCollection>();
            ((GeometryCollection)geoJson).Geometries.Count.ShouldBe(3);
            ((GeometryCollection)geoJson).Geometries.ShouldAllBe(go => go is Polygon);
        }

        [Test]
        public void invalid_op_when_parsing_circle_ineocollection_style()
        {
            var content = ResourceLoader.LoadSqlType("ng_with_circle");
            Assert.Throws<InvalidOperationException>(() => GeoToSql.Translate(content)).Message.ShouldContain("circle-like structures");
        }

        [Test]
        public void test_getting_back_circle()
        {
            GeoToSql.Reset();
            GeoToSql.Configure(cfg =>
            {
                cfg.SetReferenceSystem(SpatialReferenceSystem.WorldGeodetic1984);
                cfg.SetSqlGeographyToGeoJsonConstructionStyle(GeoJsonConstructionStyle.AsFeatureCollection);
            });
            var content = ResourceLoader.LoadSqlType("ng_with_circle");
            var geoJson = GeoToSql.Translate(content);
            
            
            geoJson.ShouldBeOfType<FeatureCollection>();
            var newCollection = (FeatureCollection)geoJson;

            var json = GetObjectFromJson<FeatureCollection>("ng_with_circle");

            json.Features.Count.ShouldBe(newCollection.Features.Count);

            for (var i = 0; i < json.Features.Count; i++)
            {
                json.Features[i].Geometry.Type.ShouldBe(newCollection.Features[i].Geometry.Type);
            }

            var originalCircle = json.Features.First(f => f.Geometry.Type == GeoJSONObjectType.Point);
            var newCircle = newCollection.Features.First(f => f.Geometry.Type == GeoJSONObjectType.Point);

            var pOrig = (GeographicPosition)((Point) originalCircle.Geometry).Coordinates;
            var pNew = (GeographicPosition)((Point) newCircle.Geometry).Coordinates;
            Debug.WriteLine(pOrig);
            Debug.WriteLine(pNew);
            pOrig.Latitude.ShouldBe(pNew.Latitude, 0.00001);
            pOrig.Longitude.ShouldBe(pNew.Longitude, 0.00001);
            var radiusOrig = double.Parse(originalCircle.Properties["radius"].ToString());
            var radiusNew = double.Parse(originalCircle.Properties["radius"].ToString());
            Debug.WriteLine("radius orig: {0}, radius new: {1}", radiusOrig, radiusNew);
            radiusOrig.ShouldBe(radiusNew, 0.001);
        }
    }
}