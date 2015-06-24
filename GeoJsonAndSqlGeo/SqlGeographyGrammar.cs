using System.Linq;
using GeoJSON.Net.Geometry;
using Irony.Ast;
using Irony.Parsing;

namespace GeoJsonToSqlServer
{
    internal class SqlGeographyGrammar : Grammar
    {
        public SqlGeographyGrammar() : base(false)
        {
            LanguageFlags |= LanguageFlags.CreateAst;
            
            // ReSharper disable InconsistentNaming
            var POINT = ToTerm("POINT");
            var MULTIPOINT = ToTerm("MULTIPOINT");
            var LINESTRING = ToTerm("LINESTRING");
            var MULTILINESTRING = ToTerm("MULTILINESTRING");
            var POLYGON = ToTerm("POLYGON");
            var MULTIPOLYGON = ToTerm("MULTIPOLYGON");
            var GEOMETRYCOLLECTION = ToTerm("GEOMETRYCOLLECTION");
            var number = new NumberLiteral("number");
            var comma = ToTerm(", ");
            var openBracket = ToTerm("(");
            var closeBracket = ToTerm(")");
            
            var coord = new NonTerminal("coordPair") { Rule = number + number };
            var coords = new NonTerminal("coords"); coords.Rule = MakePlusRule(coords, comma, coord);
            var coordSet = new NonTerminal("coordSet") { Rule = openBracket + coords + closeBracket };
            var coordSets = new NonTerminal("coordSets"); coordSets.Rule = MakePlusRule(coordSets, comma, coordSet);
            var multiCoordSet = new NonTerminal("multiCoords") { Rule = openBracket + coordSets + closeBracket};
            var multiCoordSets = new NonTerminal("multiCoordSets"); multiCoordSets.Rule = MakePlusRule(multiCoordSets, comma, multiCoordSet);
            var setOfMultiCoordSets = new NonTerminal("setOfMultiCoordSets") {Rule = openBracket + multiCoordSets + closeBracket};

            var pointDef = new NonTerminal("point") {Rule = POINT + coordSet };
            var multiPointDef = new NonTerminal("multipoint") {Rule = MULTIPOINT + multiCoordSet };
            var lineStringDef = new NonTerminal("lineString") { Rule = LINESTRING + coordSet };
            var multiLineStringDef = new NonTerminal("multilineString") { Rule = MULTILINESTRING + multiCoordSet };
            var polygonDef = new NonTerminal("polygon") { Rule = POLYGON + multiCoordSet };
            var multiPolygonDef = new NonTerminal("multipolygon") { Rule = MULTIPOLYGON + setOfMultiCoordSets };
            
            var geometryDef = new NonTerminal("geometry")
            {
                Rule = pointDef | multiPointDef | lineStringDef | multiLineStringDef | polygonDef | multiPolygonDef
            };

            var geometries = new NonTerminal("geometries");
            geometries.Rule = MakePlusRule(geometries, comma, geometryDef);

            var geometryCollection = new NonTerminal("geometryCollection") {Rule = GEOMETRYCOLLECTION + openBracket + geometries + closeBracket};


            // ROOT DEFINITION

            Root = new NonTerminal("root") {Rule = geometryCollection | geometryDef };

            // --------------

            NoopAstFor(POINT, LINESTRING, number, comma, openBracket, closeBracket, 
                       coords, coordSet, coordSets, multiCoordSets, setOfMultiCoordSets, geometryDef, geometries);
            
            AstBuilderForGeoJsonTypes.AstBuilderForGeoPosition(coord);
            AstBuilderForGeoJsonTypes.AstBuilderForCoordinateSet(coordSet, coord);
            AstBuilderForGeoJsonTypes.AstBuilderForMultipleCoordinateSets(multiCoordSet, coordSet);
            AstBuilderForGeoJsonTypes.AstBuilderForPoint(pointDef, coord);
            AstBuilderForGeoJsonTypes.AstBuilderForMultiPoint(multiPointDef, multiCoordSet);
            AstBuilderForGeoJsonTypes.AstBuilderForLineString(lineStringDef, coordSet);
            AstBuilderForGeoJsonTypes.AstBuilderForMultiLine(multiLineStringDef, multiCoordSet);
            AstBuilderForGeoJsonTypes.AstBuilderForPolygon(polygonDef, multiCoordSet);
            AstBuilderForGeoJsonTypes.AstBuilderForMultiPolygon(multiPolygonDef, multiCoordSet);
            AstBuilderForGeoJsonTypes.AstBuilderForGeometry(geometryDef);
            AstBuilderForGeoJsonTypes.AstBuilderForGeometryCollection(geometryCollection, geometryDef);
            AstBuilderForGeoJsonTypes.AstBuilderForRoot(Root);
        }

        

        private static void NoopAstFor(params BnfTerm[] nonTerminals)
        {
            foreach (var nt in nonTerminals)
            {
                var nt1 = nt;
                nt.AstConfig = new AstNodeConfig { NodeCreator = (context, node) => { node.AstNode = nt1.Name; } };
            }
        }
    }
}