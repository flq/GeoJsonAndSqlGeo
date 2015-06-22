using System;
using System.Runtime.Serialization;

namespace GeoJsonToSqlServer
{
    public class NoSpatialReferenceDefinedException : Exception
    {
        public NoSpatialReferenceDefinedException() : base("No spatial reference system has been set. Set one via GeoToSql.SetReferenceSystem(SpatialReferenceSystem)")
        {
            
        }
    }
}