using System;
using System.Collections.Generic;
using GeoJSON.Net;
using GeoJSON.Net.Converters;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoJsonAndSqlGeo;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using Tests.Examples;

namespace Tests
{
    public class TestingContext
    {
        [TestFixtureSetUp]
        public void Given()
        {
            GeoToSql.Configure(cfg =>
            {
                cfg.SetReferenceSystem((int) SpatialReferenceSystem.WorldGeodetic1984);
                cfg.SetSqlGeographyToGeoJsonConstructionStyle(ConstructionStyle);
            });
            AdditionalSetup();
        }

        protected virtual void AdditionalSetup()
        {
        }

        protected virtual GeoJsonConstructionStyle ConstructionStyle
        {
            get
            {
                return GeoJsonConstructionStyle.AsGeometryCollection;
            }
        }

        [SetUp]
        public void GivenOnEachTest()
        {
            GeoVisitor = new TestGeoVisitor();
        }

        public TestGeoVisitor GeoVisitor { get; private set; }

        [TestFixtureTearDown]
        public void End()
        {
            GeoToSql.Reset();
        }

        public static T GetObjectFromJson<T>(string fileName)
        {
            var json = ResourceLoader.LoadJson(fileName);
            var obj = JsonConvert.DeserializeObject<T>(json, new GeometryConverter());
            return obj;
        }

        protected static object GetObjectFromJson(string fileName, Type resultingType)
        {
            var json = ResourceLoader.LoadJson(fileName);
            var obj = JsonConvert.DeserializeObject(json, resultingType, new GeometryConverter());
            return obj;
        }

        protected static void AssertIsEqualToRepresentationInFile(SqlGeography sqlGeo, string fileName)
        {
            var content = ResourceLoader.LoadSqlType(fileName);
            sqlGeo.ToString().ShouldBe(content);
        }
    }

    public class TestGeoVisitor : IGeoJsonObjectVisitor
    {
        private readonly List<IGeoWalkContext> _visitedObjects = new List<IGeoWalkContext>();

        public IReadOnlyList<IGeoWalkContext> VisitedObjects { get { return _visitedObjects; } }


        public T AssertType<T>(int index) where T : GeoJSONObject
        {
            _visitedObjects[index].ShouldBeAssignableTo<GeoWalkContext<T>>();
            return (T) _visitedObjects[index].Item;
        }

        public int AssertStackDepth(int index, int depth)
        {
            _visitedObjects[index].CurrentDepth.ShouldBe(depth);
            return _visitedObjects[index].CurrentDepth;
        }

        void IGeoJsonObjectVisitor.Visit(GeoWalkContext<Point> p)
        {
            _visitedObjects.Add(p);
        }

        void IGeoJsonObjectVisitor.Visit(GeoWalkContext<MultiPoint> mp)
        {
            _visitedObjects.Add(mp);
        }

        void IGeoJsonObjectVisitor.Visit(GeoWalkContext<LineString> ls)
        {
            _visitedObjects.Add(ls);
        }

        void IGeoJsonObjectVisitor.Visit(GeoWalkContext<MultiLineString> mls)
        {
            _visitedObjects.Add(mls);
        }

        void IGeoJsonObjectVisitor.Visit(GeoWalkContext<Polygon> pl)
        {
            _visitedObjects.Add(pl);
        }

        void IGeoJsonObjectVisitor.Visit(GeoWalkContext<MultiPolygon> mpl)
        {
            _visitedObjects.Add(mpl);
        }

        void IGeoJsonObjectVisitor.Visit(GeoWalkContext<GeometryCollection> gcl)
        {
            _visitedObjects.Add(gcl);
        }

        public void Visit(GeoWalkContext<Feature> f)
        {
            _visitedObjects.Add(f);
        }

        public void Visit(GeoWalkContext<FeatureCollection> fcl)
        {
            _visitedObjects.Add(fcl);
        }
    }
}