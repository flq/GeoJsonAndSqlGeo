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

    public class ReadingBackCircle : TestingContext
    {
        private FeatureCollection newFeatureCollection;
        private FeatureCollection loadedGeoJson;

        protected override GeoJsonConstructionStyle ConstructionStyle
        {
            get { return GeoJsonConstructionStyle.AsFeatureCollection; }
        }

        protected override void AdditionalSetup()
        {
            var content = ResourceLoader.LoadSqlType("ng_with_circle");
            var geoJson = GeoToSql.Translate(content);
            geoJson.ShouldBeOfType<FeatureCollection>();
            
            newFeatureCollection = (FeatureCollection)geoJson;
            loadedGeoJson = GetObjectFromJson<FeatureCollection>("ng_with_circle");
        }

        [Test]
        public void same_feature_count()
        {
            loadedGeoJson.Features.Count.ShouldBe(newFeatureCollection.Features.Count);
        }

        [Test]
        public void same_geometry_type()
        {
            for (var i = 0; i < loadedGeoJson.Features.Count; i++)
            {
                loadedGeoJson.Features[i].Geometry.Type.ShouldBe(newFeatureCollection.Features[i].Geometry.Type);
            }
        }

        [Test]
        public void check_point_and_radius_values()
        {
            var originalCircle = loadedGeoJson.Features.First(f => f.Geometry.Type == GeoJSONObjectType.Point);
            var newCircle = newFeatureCollection.Features.First(f => f.Geometry.Type == GeoJSONObjectType.Point);
            var pOrig = (GeographicPosition)((Point)originalCircle.Geometry).Coordinates;
            var pNew = (GeographicPosition)((Point)newCircle.Geometry).Coordinates;
            var radiusOrig = double.Parse(originalCircle.Properties["radius"].ToString());
            var radiusNew = double.Parse(originalCircle.Properties["radius"].ToString());

            pOrig.Latitude.ShouldBe(pNew.Latitude, 0.00001);
            pOrig.Longitude.ShouldBe(pNew.Longitude, 0.00001);
            
            radiusOrig.ShouldBe(radiusNew, 0.001);
        }


    }

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

        [Test]
        public void invalid_op_when_parsing_circle_in_collection_style()
        {
            var content = ResourceLoader.LoadSqlType("ng_with_circle");
            Assert.Throws<InvalidOperationException>(() => GeoToSql.Translate(content)).Message.ShouldContain("circle-like structures");
        }
    }
}