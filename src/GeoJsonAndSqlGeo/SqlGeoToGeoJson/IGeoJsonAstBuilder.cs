using Irony.Parsing;

namespace GeoJsonAndSqlGeo
{
    internal interface IGeoJsonAstBuilder
    {
        void ForRoot(NonTerminal root);
        void ForGeometryCollection(NonTerminal geometryCollection, NonTerminal geometryDef);
        void ForGeometry(NonTerminal geometryDef);
        void ForMultiPolygon(NonTerminal multiPolygonDef, NonTerminal multiCoordSet);
        void ForPolygon(NonTerminal polygonDef, NonTerminal multiCoords);
        void ForMultiLine(NonTerminal multiLineStringDef, NonTerminal multiCoords);
        void ForLineString(NonTerminal lineStringDef, NonTerminal coordData);
        void ForMultiPoint(NonTerminal multiPointDef, NonTerminal multiCoords);
        void ForPoint(NonTerminal pointDef, NonTerminal coord);
        void ForMultipleCoordinateSets(NonTerminal multiCoords, NonTerminal coordData);
        void ForCoordinateSet(NonTerminal coordData, NonTerminal coord);
        void ForGeoPosition(NonTerminal coord);
        void ForCircleDefinition(NonTerminal circleDef, NonTerminal coordSet);
    }
}