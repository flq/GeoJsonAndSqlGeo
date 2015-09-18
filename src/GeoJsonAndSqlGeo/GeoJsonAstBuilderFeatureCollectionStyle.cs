using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Irony.Parsing;
using Microsoft.SqlServer.Types;

namespace GeoJsonAndSqlGeo
{
    internal class GeoJsonAstBuilderFeatureCollectionStyle : AbstractGeoJsonAstBuilder
    {
        private readonly string _sqlGeoToParse;

        public GeoJsonAstBuilderFeatureCollectionStyle(string sqlGeoToParse)
        {
            _sqlGeoToParse = sqlGeoToParse;
        }

        public override void ForGeometryCollection(NonTerminal geometryCollection, NonTerminal geometryDef)
        {
            AstBuilder(geometryCollection, (context, node) =>
            {
                var geometries = GetAllAstNodesOf<Feature>(node, geometryDef).ToList();
                node.AstNode = new FeatureCollection(geometries);
            });
        }

        public override void ForGeometry(NonTerminal geometryDef)
        {
            AstBuilder(geometryDef, (context, node) =>
            {
                node.AstNode = node.ChildNodes.Count == 1
                    ? node.ChildNodes[0].AstNode
                    : new Feature(new GeometryCollection(node.ChildNodes.Select(ptn => ptn.AstNode).OfType<IGeometryObject>().ToList()));
            });
        }

        public override void ForMultiPolygon(NonTerminal multiPolygonDef, NonTerminal multiCoordSet)
        {
            AstBuilder(multiPolygonDef, (context, node) =>
            {
                var polygons =
                    from manyLineStrings in GetAllAstNodesOf<List<List<GeographicPosition>>>(node, multiCoordSet)
                    let container =
                        (from lineStringContent in manyLineStrings
                         select new LineString(lineStringContent)).ToList()
                    select new Polygon(container);

                node.AstNode = new Feature(new MultiPolygon(polygons.ToList()));
            });
        }

        public override void ForPolygon(NonTerminal polygonDef, NonTerminal multiCoords)
        {
            AstBuilder(polygonDef, (context, node) =>
            {
                var lineStrings = GetAllAstNodesOf<List<List<GeographicPosition>>>(node, multiCoords)
                    .First()
                    .Select(l => new LineString(l));
                
                node.AstNode = new Feature(new Polygon(lineStrings.ToList()));
            });
        }

        public override void ForCircleDefinition(NonTerminal circleDef, NonTerminal coordSet)
        {
            AstBuilder(circleDef, (context, node) =>
            {
                var geoDef = _sqlGeoToParse.Substring(node.Span.Location.Position, node.Span.Length);
                var geo = SqlGeography.Parse(new SqlString(geoDef));
                var p = geo.EnvelopeCenter();
                var lat = p.Lat.Value;
                var longit = p.Long.Value;
                var radius = Math.Round(geo.STStartPoint().STDistance(p).Value);    
                node.AstNode = new Feature(new Point(new GeographicPosition(lat, longit)), new Dictionary<string, object> { { "radius", radius } });
            });
        }
    }
}