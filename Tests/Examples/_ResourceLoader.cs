using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tests.Examples
{
    public class ResourceLoader
    {
        public static string LoadJson(string fileName)
        {
            using (var s = typeof (ResourceLoader).Assembly.GetManifestResourceStream(typeof (ResourceLoader), fileName + ".json"))
            using (var sr = new StreamReader(s))
                return sr.ReadToEnd();
        }

        public static string LoadSqlType(string fileName)
        {
            using (var s = typeof(ResourceLoader).Assembly.GetManifestResourceStream(typeof(ResourceLoader), fileName + ".sqltype"))
            using (var sr = new StreamReader(s))
                return sr.ReadToEnd();
        }

        /// <summary>
        /// Files are taken from http://geojson.org/geojson-spec.html#appendix-a-geometry-examples
        /// Used in tests as test case source.
        /// multilinestring: Coordinates of a MultiLineString are an array of LineString coordinate arrays
        /// multipolygon: Coordinates of a MultiPolygon are an array of Polygon coordinate arrays
        /// polygon: Coordinates of a Polygon are an array of LinearRing coordinate arrays. 
        /// The first element in the array represents the exterior ring. Any subsequent elements represent interior rings (or holes).
        /// geometrycollection: Each element in the geometries array of a GeometryCollection is one of the geometry objects described above
        /// </summary>
        public static IEnumerable<object[]> KnownGeometryJsonFiles()
        {
            Func<string, object[]> f = s => new[] { s };

            return KnownGeometryFiles().Select(f);

        }

        private static IEnumerable<string> KnownGeoJsonFiles()
        {
            return typeof(ResourceLoader).Assembly.GetManifestResourceNames()
                .Where(n => n.EndsWith("json"))
                .Select(Path.GetFileNameWithoutExtension)
                .Select(s => s.Replace(typeof(ResourceLoader).Namespace + ".", ""));
        }

        private static IEnumerable<string> KnownGeometryFiles()
        {
            const string notGeometry = "ng_";
            return KnownGeoJsonFiles().Where(s => !s.StartsWith(notGeometry));

        }
    }
}