using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.SqlServer.Types;

namespace GeoJsonAndSqlGeo
{

    internal class GeoJsonToSqlGeographyObjectWalker : GeoJsonObjectVisitor
    {
        private readonly SqlGeographyBuilder _bld = new SqlGeographyBuilder();
        private readonly List<SqlGeography> _pendingCircles = new List<SqlGeography>();

        private readonly Lazy<SqlGeography> _constructedGeography;

        public GeoJsonToSqlGeographyObjectWalker()
        {
            _bld.SetSrid(GeoToSql.ReferenceId);
            _constructedGeography = new Lazy<SqlGeography>(() =>
            {
                if (_pendingCircles.Count == 0)
                    return _bld.ConstructedGeography;

                // Decompose the geo collection, add the pending circles, wrap as geometry
                // collection and reparse as geo collection.
                var geos = new List<SqlGeography>();
                var geoRoot = _bld.ConstructedGeography;
                for (var i = 1; i <= geoRoot.STNumGeometries(); i++)
                {
                    geos.Add(geoRoot.STGeometryN(i));
                }
                var sb = new StringBuilder();
                sb.Append("GEOMETRYCOLLECTION (");
                sb.Append(string.Join(",", geos.Concat(_pendingCircles)));
                sb.Append(")");
                Debug.WriteLine(sb.ToString());

                return SqlGeography.STGeomCollFromText(new SqlChars(new SqlString(sb.ToString())), GeoToSql.ReferenceId);
            });
        }

        public SqlGeography ConstructedGeography
        {
            get { return _constructedGeography.Value; }
        }

        public override void Visit(GeoWalkContext<GeometryCollection> gcl)
        {
            _bld.BeginGeography(OpenGisGeographyType.GeometryCollection);
            gcl.SetExitActivity(() => _bld.EndGeography());
        }

        public override void Visit(GeoWalkContext<FeatureCollection> fcl)
        {
            _bld.BeginGeography(OpenGisGeographyType.GeometryCollection);
            fcl.SetExitActivity(() => _bld.EndGeography());
        }

        public override void Visit(GeoWalkContext<MultiPolygon> mpl)
        {

            _bld.BeginGeography(OpenGisGeographyType.MultiPolygon);
            mpl.SetExitActivity(() => _bld.EndGeography());
        }

        public override void Visit(GeoWalkContext<Polygon> pl)
        {
            _bld.BeginGeography(OpenGisGeographyType.Polygon);
            pl.SetExitActivity(() => _bld.EndGeography());
        }

        public override void Visit(GeoWalkContext<LineString> ls)
        {
            // Line strings are used as items in polygons, but in an isolated context
            // they also neeed to be introduced with BeginGeography
            if (!ls.HasParentCorrespondingTo<Polygon>())
                _bld.BeginGeography(OpenGisGeographyType.LineString);
            AddAsBeginFigure(ls.Item.Coordinates[0]);
            foreach (var position in ls.Item.Coordinates.Skip(1))
            {
                AddAsLine(position);
            }
            _bld.EndFigure();
            if (!ls.HasParentCorrespondingTo<Polygon>())
                _bld.EndGeography();
        }

        public override void Visit(GeoWalkContext<MultiLineString> mls)
        {
            _bld.BeginGeography(OpenGisGeographyType.MultiLineString);
            mls.SetExitActivity(() => _bld.EndGeography());
        }

        public override void Visit(GeoWalkContext<Point> p)
        {
            if (p.HasParentCorrespondingTo<Feature>() && FeatureRepresentsCircle((Feature) p.Parent.Item))
                return;
            _bld.BeginGeography(OpenGisGeographyType.Point);
            PointFigure(p.Item);
            _bld.EndGeography();
        }

        public override void Visit(GeoWalkContext<MultiPoint> mp)
        {
            _bld.BeginGeography(OpenGisGeographyType.MultiPoint);
            mp.SetExitActivity(() => _bld.EndGeography());
        }

        public override void Visit(GeoWalkContext<Feature> f)
        {
            var feature = f.Item;
            if (FeatureRepresentsCircle(feature))
            {
                var coords = (GeographicPosition)((Point) feature.Geometry).Coordinates;
                var p = SqlGeography.Point(coords.Latitude, coords.Longitude, GeoToSql.ReferenceId);
                var radius = Convert.ToDouble(feature.Properties["radius"]);
                var circle = p.BufferWithCurves(radius);
                _pendingCircles.Add(circle);
            }
        }

        private void PointFigure(Point p)
        {
            AddAsBeginFigure(p.Coordinates);
            _bld.EndFigure();
        }

        private void AddAsBeginFigure(IPosition position)
        {
            var pos = (GeographicPosition) position;
            _bld.BeginFigure(pos.Latitude, pos.Longitude, pos.Altitude, null);
        }

        private void AddAsLine(IPosition position)
        {
            var pos = (GeographicPosition)position;
            _bld.AddLine(pos.Latitude, pos.Longitude, pos.Altitude, null);
        }

        private static bool FeatureRepresentsCircle(Feature feature)
        {
            return feature.Geometry.Type == GeoJSONObjectType.Point && feature.Properties.ContainsKey("radius");
        }
    }
}