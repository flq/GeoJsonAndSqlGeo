using GeoJSON.Net;
using GeoJSON.Net.Geometry;
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

        public static void Reset()
        {
            _spatialReferenceSystemId = null;
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