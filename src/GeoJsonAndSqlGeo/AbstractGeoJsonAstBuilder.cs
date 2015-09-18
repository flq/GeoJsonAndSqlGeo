using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using Irony.Ast;
using Irony.Parsing;

namespace GeoJsonAndSqlGeo
{
    internal abstract class AbstractGeoJsonAstBuilder: IGeoJsonAstBuilder
    {
        public abstract void ForGeometryCollection(NonTerminal geometryCollection, NonTerminal geometryDef);
        public abstract void ForGeometry(NonTerminal geometryDef);
        public abstract void ForMultiPolygon(NonTerminal multiPolygonDef, NonTerminal multiCoordSet);
        public abstract void ForPolygon(NonTerminal polygonDef, NonTerminal multiCoords);

        public abstract void ForCircleDefinition(NonTerminal circleDef, NonTerminal coordSet);

        public void ForGeoPosition(NonTerminal coord)
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


        public virtual void ForCoordinateSet(NonTerminal coordData, NonTerminal coord)
        {
            AstBuilder(coordData, (context, node) => { node.AstNode = GetAllAstNodesOf<GeographicPosition>(node, coord).ToList(); });
        }

        public virtual void ForMultipleCoordinateSets(NonTerminal multiCoords, NonTerminal coordData)
        {
            AstBuilder(multiCoords, (context, node) => { node.AstNode = GetAllAstNodesOf<List<GeographicPosition>>(node, coordData).ToList(); });
        }

        public virtual void ForPoint(NonTerminal pointDef, NonTerminal coord)
        {
            AstBuilder(pointDef, (context, node) => { node.AstNode = new Point(GetAllAstNodesOf<GeographicPosition>(node, coord).Single()); });
        }

        public virtual void ForMultiPoint(NonTerminal multiPointDef, NonTerminal multiCoords)
        {
            AstBuilder(multiPointDef, (context, node) =>
            {
                var points = GetAllAstNodesOf<List<List<GeographicPosition>>>(node, multiCoords)
                    .First()
                    .Select(l => l[0])
                    .Select(gp => new Point(gp))
                    .ToList();
                node.AstNode = new MultiPoint(points);
            });
        }

        public virtual void ForLineString(NonTerminal lineStringDef, NonTerminal coordData)
        {
            AstBuilder(lineStringDef, (context, node) => { node.AstNode = new LineString(GetAllAstNodesOf<List<GeographicPosition>>(node, coordData).First()); });
        }

        protected static void AstBuilder(BnfTerm token, AstNodeCreator creator)
        {
            token.AstConfig = new AstNodeConfig { NodeCreator = creator };
        }

        protected static IEnumerable<T> GetAllAstNodesOf<T>(ParseTreeNode node, BnfTerm term)
        {
            if (node.Term == term)
                yield return (T)node.AstNode;
            foreach (var n in node.ChildNodes)
            {
                if (n.Term == term)
                    yield return (T)n.AstNode;

                if (n.ChildNodes.Count == 0) continue;

                foreach (var result in n.ChildNodes.SelectMany(nInner => GetAllAstNodesOf<T>(nInner, term)))
                    yield return result;
            }
        }

        public virtual void ForMultiLine(NonTerminal multiLineStringDef, NonTerminal multiCoords)
        {
            AstBuilder(multiLineStringDef, (context, node) =>
            {
                var lineStrings = GetAllAstNodesOf<List<List<GeographicPosition>>>(node, multiCoords).First()
                    .Select(l => new LineString(l));
                node.AstNode = new MultiLineString(lineStrings.ToList());
            });
        }

        public virtual void ForRoot(NonTerminal root)
        {
            AstBuilder(root, (context, node) =>
            {
                node.AstNode = node.ChildNodes[0].AstNode;
            });
        }
    }
}