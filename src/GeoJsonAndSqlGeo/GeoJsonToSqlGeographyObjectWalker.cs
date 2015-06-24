using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.SqlServer.Types;

namespace GeoJsonAndSqlGeo
{

    internal class GeoJsonToSqlGeographyObjectWalker : GeoJsonObjectVisitor
    {
        private readonly SqlGeographyBuilder _bld = new SqlGeographyBuilder();

        public GeoJsonToSqlGeographyObjectWalker()
        {
            _bld.SetSrid(GeoToSql.ReferenceId);
        }

        public SqlGeography ConstructedGeography
        {
            get { return _bld.ConstructedGeography; }
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
            _bld.BeginGeography(OpenGisGeographyType.Point);
            PointFigure(p.Item);
            _bld.EndGeography();
        }

        public override void Visit(GeoWalkContext<MultiPoint> mp)
        {
            _bld.BeginGeography(OpenGisGeographyType.MultiPoint);
            mp.SetExitActivity(() => _bld.EndGeography());
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
    }
}