using Irony.Ast;
using Irony.Parsing;

namespace GeoJsonAndSqlGeo
{
    internal class SqlGeographyGrammar : Grammar
    {
        public SqlGeographyGrammar(string sqlGeoToParse) : base(false)
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
            var CURVEPOLYGON = ToTerm("CURVEPOLYGON");
            var CIRCULARSTRING = ToTerm("CIRCULARSTRING");
            var number = new NumberLiteral("number", NumberOptions.AllowSign);
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
            var circleDef = new NonTerminal("circle") { Rule = CURVEPOLYGON + openBracket + CIRCULARSTRING + coordSet + closeBracket };
            
            var geometryDef = new NonTerminal("geometry")
            {
                Rule = pointDef | multiPointDef | lineStringDef | multiLineStringDef | polygonDef | multiPolygonDef | circleDef
            };

            var geometries = new NonTerminal("geometries");
            geometries.Rule = MakePlusRule(geometries, comma, geometryDef);

            var geometryCollection = new NonTerminal("geometryCollection") {Rule = GEOMETRYCOLLECTION + openBracket + geometries + closeBracket};


            // ROOT DEFINITION

            Root = new NonTerminal("root") {Rule = geometryCollection | geometryDef };

            // --------------

            NoopAstFor(POINT, LINESTRING, CURVEPOLYGON, CIRCULARSTRING,  number, comma, openBracket, closeBracket, 
                       coords, coordSet, coordSets, multiCoordSets, setOfMultiCoordSets, geometryDef, geometries);

            var astBuilder = GeoToSql.ConstructionStyle == GeoJsonConstructionStyle.AsGeometryCollection ?
                (IGeoJsonAstBuilder)new GeoJsonAstBuilderGeoCollectionStyle() :
                new GeoJsonAstBuilderFeatureCollectionStyle(sqlGeoToParse);

            astBuilder.ForGeoPosition(coord);
            astBuilder.ForCoordinateSet(coordSet, coord);
            astBuilder.ForMultipleCoordinateSets(multiCoordSet, coordSet);
            astBuilder.ForPoint(pointDef, coord);
            astBuilder.ForMultiPoint(multiPointDef, multiCoordSet);
            astBuilder.ForLineString(lineStringDef, coordSet);
            astBuilder.ForMultiLine(multiLineStringDef, multiCoordSet);
            astBuilder.ForPolygon(polygonDef, multiCoordSet);
            astBuilder.ForMultiPolygon(multiPolygonDef, multiCoordSet);
            astBuilder.ForCircleDefinition(circleDef, coordSet);
            astBuilder.ForGeometry(geometryDef);
            astBuilder.ForGeometryCollection(geometryCollection, geometryDef);
            astBuilder.ForRoot(Root);
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