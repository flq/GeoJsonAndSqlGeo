using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoJsonAndSqlGeo;
using NUnit.Framework;
using Shouldly;

namespace Tests
{
    [TestFixture]
    public class GeoJsonWalkerTests : TestingContext
    {
        [Test]
        public void walking_point()
        {
            var walker = new GeoJsonObjectWalker(GetObjectFromJson<Point>("point"));
            walker.CarryOut(GeoVisitor);
            GeoVisitor.VisitedObjects.Count.ShouldBe(1);
            GeoVisitor.AssertType<Point>(0);
        }

        [Test]
        public void walking_complex_geo()
        {
            var walker = new GeoJsonObjectWalker(GetObjectFromJson<GeometryCollection>("geometrycollection"));
            walker.CarryOut(GeoVisitor);
            GeoVisitor.VisitedObjects.Count.ShouldBe(11);
            GeoVisitor.AssertType<Point>(1);
            GeoVisitor.AssertType<LineString>(4); // gcl@0 -> polygon@3 -> linestring
            GeoVisitor.AssertType<MultiPolygon>(5); // gcl@0 -> polygon@5 (-> 1xpolygon + 1xlinestring + 1xpolygon + 2xlinestrings)
        }

        [Test]
        public void walking_feature_collection()
        {
            var walker = new GeoJsonObjectWalker(GetObjectFromJson<FeatureCollection>("ng_earthquakes"));
            walker.CarryOut(GeoVisitor);
            GeoVisitor.VisitedObjects.Count.ShouldBe(369); // 1 feature Collection with 184 Features, each with a point = 1 + 184 *2
            GeoVisitor.AssertType<Feature>(181);
            GeoVisitor.AssertType<Point>(172);
            GeoVisitor.AssertStackDepth(172, 2);
        }
    }
}