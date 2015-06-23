using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Irony.Ast;
using Irony.Parsing;

namespace GeoJsonToSqlServer
{
    internal static class AstBuilderForGeoJsonTypes
    {
        public static void AstBuilderForRoot(NonTerminal root)
        {
            AstBuilder(root, (context, node) =>
            {
                node.AstNode = node.ChildNodes[0].AstNode;
            });
        }

        public static void AstBuilderForGeometryCollection(NonTerminal geometryCollection, NonTerminal geometryDef)
        {
            AstBuilder(geometryCollection, (context, node) =>
            {
                var geometries = GetAllAstNodesOf<GeoJSONObject>(node, geometryDef);
                node.AstNode = new GeometryCollection(geometries.OfType<IGeometryObject>().ToList());
            });
        }

        public static void AstBuilderForGeometry(NonTerminal geometryDef)
        {
            AstBuilder(geometryDef, (context, node) =>
            {
                node.AstNode = node.ChildNodes.Count == 1
                    ? node.ChildNodes[0].AstNode
                    : new GeometryCollection(node.ChildNodes.Select(ptn => ptn.AstNode).OfType<IGeometryObject>().ToList());
            });
        }

        public static void AstBuilderForMultiPolygon(NonTerminal multiPolygonDef, NonTerminal multiCoordSet)
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

        public static void AstBuilderForPolygon(NonTerminal polygonDef, NonTerminal multiCoords)
        {
            AstBuilder(polygonDef, (context, node) =>
            {
                var lineStrings = GetAllAstNodesOf<List<List<GeographicPosition>>>(node, multiCoords)
                    .First()
                    .Select(l => new LineString(l));
                node.AstNode = new Polygon(lineStrings.ToList());
            });
        }

        public static void AstBuilderForMultiLine(NonTerminal multiLineStringDef, NonTerminal multiCoords)
        {
            AstBuilder(multiLineStringDef, (context, node) =>
            {
                var lineStrings = GetAllAstNodesOf<List<List<GeographicPosition>>>(node, multiCoords).First()
                    .Select(l => new LineString(l));
                node.AstNode = new MultiLineString(lineStrings.ToList());
            });
        }

        public static void AstBuilderForLineString(NonTerminal lineStringDef, NonTerminal coordData)
        {
            AstBuilder(lineStringDef, (context, node) => { node.AstNode = new LineString(GetAllAstNodesOf<List<GeographicPosition>>(node, coordData).First()); });
        }

        public static void AstBuilderForMultiPoint(NonTerminal multiPointDef, NonTerminal multiCoords)
        {
            AstBuilder(multiPointDef, (context, node) =>
            {
                var points = node.GetAllAstNodesOf<List<List<GeographicPosition>>>(multiCoords)
                    .First()
                    .Select(l => l[0])
                    .Select(gp => new Point(gp))
                    .ToList();
                node.AstNode = new MultiPoint(points);
            });
        }

        public static void AstBuilderForPoint(NonTerminal pointDef, NonTerminal coord)
        {
            AstBuilder(pointDef, (context, node) => { node.AstNode = new Point(node.GetAllAstNodesOf<GeographicPosition>(coord).Single()); });
        }

        public static void AstBuilderForMultipleCoordinateSets(NonTerminal multiCoords, NonTerminal coordData)
        {
            AstBuilder(multiCoords, (context, node) => { node.AstNode = node.GetAllAstNodesOf<List<GeographicPosition>>(coordData).ToList(); });
        }

        public static void AstBuilderForCoordinateSet(NonTerminal coordData, NonTerminal coord)
        {
            AstBuilder(coordData, (context, node) => { node.AstNode = node.GetAllAstNodesOf<GeographicPosition>(coord).ToList(); });
        }

        public static void AstBuilderForGeoPosition(NonTerminal coord)
        {
            AstBuilder(coord, (context, node) =>
            {
                var numbers = Enumerable.Range(0, 2)
                    .Select(idx => node.ChildNodes[idx].Token.Value)
                    .Select(Convert.ToDouble)
                    .ToArray();
                node.AstNode = new GeographicPosition(numbers[1], numbers[0]);
            });
        }

        public static void AstBuilder(BnfTerm token, AstNodeCreator creator)
        {
            token.AstConfig = new AstNodeConfig { NodeCreator = creator };
        }

        private static IEnumerable<T> GetAllAstNodesOf<T>(this ParseTreeNode node, BnfTerm term)
        {
            if (node.Term == term)
                yield return (T) node.AstNode;
            foreach (var n in node.ChildNodes)
            {
                if (n.Term == term)
                    yield return (T)n.AstNode;
                
                if (n.ChildNodes.Count == 0) continue;
                
                foreach (var result in n.ChildNodes.SelectMany(nInner => GetAllAstNodesOf<T>(nInner, term)))
                    yield return result;
            }
        }
    }
}