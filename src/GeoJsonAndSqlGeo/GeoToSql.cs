using System;
using GeoJSON.Net;
using Irony.Parsing;
using Microsoft.SqlServer.Types;

namespace GeoJsonAndSqlGeo
{
    public static class GeoToSql
    {

        private static int? _spatialReferenceSystemId;
        private static GeoJsonConstructionStyle? _constructionStyle;

        public static int ReferenceId
        {
            get
            {
                AssertValidity();
                // ReSharper disable once PossibleInvalidOperationException
                return _spatialReferenceSystemId.Value;
            }
        }

        public static GeoJsonConstructionStyle ConstructionStyle
        {
            get
            {
                AssertValidity();
                // ReSharper disable once PossibleInvalidOperationException
                return _constructionStyle.HasValue ? _constructionStyle.Value : GeoJsonConstructionStyle.AsGeometryCollection;
            }
        }

        /// <summary>
        /// Please call this before using any of the Translate methods.
        /// It is mandatory to set up at least the spatial reference system.
        /// </summary>
        public static void Configure(Action<IConfig> config)
        {
            if (config == null) throw new ArgumentNullException("config");
            var cfg = new Config();
            config(cfg);

        }

        /// <summary>
        /// Translate a GEOJSONObject to the corresponding SqlGeography. Please note that without any additional configuration,
        /// a FeatureCollection will turn into a GeometryCollection and the geometry of each feature will be added to that collection.
        /// </summary>
        public static SqlGeography Translate(GeoJSONObject geoJsonObject)
        {
            AssertValidity();
            var visitor = new GeoJsonToSqlGeographyObjectWalker();
            var walker = new GeoJsonObjectWalker(geoJsonObject);
            walker.CarryOut(visitor);
            return visitor.ConstructedGeography;
        }

        private static void SetReferenceSystem(int spatialReferenceId)
        {
            _spatialReferenceSystemId = spatialReferenceId;
        }

        // ReSharper disable once UnusedMember.Global - Part of API
        public static GeoJSONObject Translate(SqlGeography sqlGeography)
        {
            return Translate(sqlGeography.ToString());
        }

        public static void Reset()
        {
            _spatialReferenceSystemId = null;
            _constructionStyle = null;
        }

        public static GeoJSONObject Translate(string sqlGeographyRepresentation)
        {
            AssertValidity();
            var tree = ParseTree(sqlGeographyRepresentation);
            return (GeoJSONObject)tree.Root.AstNode;
        }


        private static void SetReferenceSystem(SpatialReferenceSystem spatialReference)
        {
            _spatialReferenceSystemId = (int)spatialReference;
        }

        private static void SetConstructionStyle(GeoJsonConstructionStyle constructionStyle)
        {
            _constructionStyle = constructionStyle;
        }

        private static ParseTree ParseTree(string sqlGeographyRepresentation, bool throwOnError = true)
        {
            var grammar = new SqlGeographyGrammar(sqlGeographyRepresentation);
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

        private class Config : IConfig
        {
            void IConfig.SetReferenceSystem(int id)
            {
                SetReferenceSystem(id);
            }

            void IConfig.SetReferenceSystem(SpatialReferenceSystem id)
            {
                SetReferenceSystem(id);
            }

            void IConfig.SetSqlGeographyToGeoJsonConstructionStyle(GeoJsonConstructionStyle constructionStyle)
            {
                SetConstructionStyle(constructionStyle);
            }
        }
    }

    public interface IConfig
    {
        /// <summary>
        /// Use if you are using a spatial reference system known to Sql server but not represented in the <see cref="SpatialReferenceSystem"/> enum.
        /// </summary>
        void SetReferenceSystem(int id);
        
        /// <summary>
        /// Initialize to a known spatial reference system
        /// </summary>
        void SetReferenceSystem(SpatialReferenceSystem id);

        /// <summary>
        /// Set how the output of the SqlGeography -> GeoJson Translate method should look like.
        /// Default is <see cref="GeoJsonConstructionStyle.AsGeometryCollection"/>
        /// </summary>
        void SetSqlGeographyToGeoJsonConstructionStyle(GeoJsonConstructionStyle constructionStyle);
    }

    /// <summary>
    /// Represents some commonly used values 
    /// </summary>
    public enum SpatialReferenceSystem
    {
        WorldGeodetic1984 = 4326
    }

    /// <summary>
    /// The output that you want from the <see cref="GeoToSql.Translate(SqlGeography)"/> method.
    /// </summary>
    public enum GeoJsonConstructionStyle
    {
        /// <summary>
        /// Returns the sql geometry collection as geojeon collection. Please note that this method is incompatible if you
        /// stored via the supported method of Feature + Point + radius property.
        /// </summary>
        AsGeometryCollection,
        /// <summary>
        /// Returns each top-level item of the geometrycollection as a feature. This will support output of a geojson "circle"
        /// </summary>
        AsFeatureCollection
    }
}