using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Converters;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoJsonAndSqlGeo;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using Tests.Examples;

namespace Tests
{
    [TestFixture]
    public class BaselineTests
    {
        [Test]
        public void attempted_conversions_require_setting_spatial_reference_system()
        {
            var p = new Point(new GeographicPosition(23.0, 23.0));
            Assert.Throws<NoSpatialReferenceDefinedException>(()=> GeoToSql.Translate(p));    
        }

        [Test]
        [TestCaseSource(typeof(ResourceLoader), "KnownGeometryJsonFiles")]
        public void all_json_loads_fine(string fileName)
        {
            var json = ResourceLoader.LoadJson(fileName);
            json.ShouldNotBe(null);
            json.Length.ShouldBeGreaterThan(10);
        }


        [Test]
        [TestCaseSource(typeof(ResourceLoader), "KnownGeometryJsonFiles")]
        public void GeoJson_deserialization_works(string fileName)
        {
            var obj = TestingContext.GetObjectFromJson<IGeometryObject>(fileName);
            obj.ShouldNotBe(null);
            Debug.WriteLine(obj.GetType().Name);
        }

        [Test]
        public void GeoJson_deserialization_works_feature()
        {
            var obj = TestingContext.GetObjectFromJson<FeatureCollection>("ng_earthquakes");
            obj.ShouldNotBe(null);
            Debug.WriteLine(obj.GetType().Name);
        }
    }
}