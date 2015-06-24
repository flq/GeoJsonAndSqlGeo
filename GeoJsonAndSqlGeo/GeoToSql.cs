using GeoJSON.Net;
using Irony.Parsing;
using Microsoft.SqlServer.Types;

namespace GeoJsonToSqlServer
{
    public static class GeoToSql
    {

        private static int? _spatialReferenceSystemId;

        public static int ReferenceId
        {
            get
            {
                AssertValidity();
                // ReSharper disable once PossibleInvalidOperationException
                return _spatialReferenceSystemId.Value;
            }
        }

        /// <summary>
        /// Use if you are using a spatial reference system known to Sql server but not represented in the <see cref="SpatialReferenceSystem"/> enum.
        /// </summary>
        public static void SetReferenceSystem(int spatialReferenceId)
        {
            _spatialReferenceSystemId = spatialReferenceId;
        }

        /// <summary>
        /// Initialize to a known spatial reference system
        /// </summary>
        public static void SetReferenceSystem(SpatialReferenceSystem spatialReference)
        {
            _spatialReferenceSystemId = (int) spatialReference;
        }

        public static SqlGeography Translate(GeoJSONObject geoJsonObject)
        {
            AssertValidity();
            var visitor = new GeoJsonToSqlGeographyObjectWalker();
            var walker = new GeoJsonObjectWalker(geoJsonObject);
            walker.CarryOut(visitor);
            return visitor.ConstructedGeography;
        }

        public static GeoJSONObject Translate(SqlGeography sqlGeography)
        {
            return Translate(sqlGeography.ToString());
        }

        public static GeoJSONObject Translate(string sqlGeographyRepresentation)
        {
            AssertValidity();
            var tree = ParseTree(sqlGeographyRepresentation);
            return (GeoJSONObject)tree.Root.AstNode;
        }

        public static void Reset()
        {
            _spatialReferenceSystemId = null;
        }

        internal static ParseTree ParseTree(string sqlGeographyRepresentation, bool throwOnError = true)
        {
            var grammar = new SqlGeographyGrammar();
            var p = new Parser(grammar);
            var tree = p.Parse(sqlGeographyRepresentation);
            if (tree.Status == ParseTreeStatus.Error && throwOnError)
                throw new SqlGeometryParseException(tree);
            return tree;
        }

        private static void AssertValidity()
        {
            if (!_spatialReferenceSystemId.HasValue)
                throw new NoSpatialReferenceDefinedException();            
        }
    }

    /// <summary>
    /// Represents some commonly used values 
    /// </summary>
    public enum SpatialReferenceSystem
    {
        WorldGeodetic1984 = 4326
    }
}