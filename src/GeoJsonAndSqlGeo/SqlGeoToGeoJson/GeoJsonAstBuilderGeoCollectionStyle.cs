using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Irony.Parsing;

namespace GeoJsonAndSqlGeo
{
    internal class GeoJsonAstBuilderGeoCollectionStyle : AbstractGeoJsonAstBuilder
    {
        public override void ForGeometryCollection(NonTerminal geometryCollection, NonTerminal geometryDef)
        {
            AstBuilder(geometryCollection, (context, node) =>
            {
                var geometries = GetAllAstNodesOf<GeoJSONObject>(node, geometryDef);
                node.AstNode = new GeometryCollection(geometries.OfType<IGeometryObject>().ToList());
            });
        }

        public override void ForGeometry(NonTerminal geometryDef)
        {
            AstBuilder(geometryDef, (context, node) =>
            {
                node.AstNode = node.ChildNodes.Count == 1
                    ? node.ChildNodes[0].AstNode
                    : new GeometryCollection(node.ChildNodes.Select(ptn => ptn.AstNode).OfType<IGeometryObject>().ToList());
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

                node.AstNode = new MultiPolygon(polygons.ToList());
            });
        }

        public override void ForPolygon(NonTerminal polygonDef, NonTerminal multiCoords)
        {
            AstBuilder(polygonDef, (context, node) =>
            {
                var lineStrings = GetAllAstNodesOf<List<List<GeographicPosition>>>(node, multiCoords)
                    .First()
                    .Select(l => new LineString(l));
                node.AstNode = new Polygon(lineStrings.ToList());
            });
        }

        public override void ForCircleDefinition(NonTerminal circleDef, NonTerminal coordSet)
        {
            AstBuilder(circleDef, (context, node) =>
            {
                throw new InvalidOperationException("SqlGeometryParser cannot handle circle-like structures if the output style is set to GeometryCollection");
            });
        }
    }
}