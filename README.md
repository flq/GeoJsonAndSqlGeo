# GeoJsonAndSqlGeo
Library to get SqlGeography from GeoJson and back

You can use this to e.g. define zones or similar things in GoogleMaps, export the data as __GeoJson__, traslate it to a 
__SqlGeography__ type in order to store it in a __SqlServer DB__ e.g. with Dapper.

This library has the following dependencies:
- Paket (for nuget things)
- GeoJSON.Net
- Irony

Entry point is the `GeoJsonSql` class. Call `Translate`to get back and forth from a deserialized GeoJson tree to a 
SqlGeography instance. Call Configure to set a few necessary things. In fact the only mandatory thing you need to set 
is the underlying coordinate system of the SqlGeography.

Note that this library supports handling circles. GeoJson does not "natively" support circles. 
It is supported by passing a Feature with a point as geometry and a property with name "radius", providing the radius
as a number that fits to the Spatial representation system that you are using (e.g. Meter in World Geodetic).

In this mode, you need to set the library to output a `FeatureCollection` when translating fom SqlGeography to GeoJson
through the config.

```csharp
GeoToSql.Configure(cfg =>
{
    cfg.SetReferenceSystem(SpatialReferenceSystem.WorldGeodetic1984);
    cfg.SetSqlGeographyToGeoJsonConstructionStyle(GeoJsonConstructionStyle.AsFeatureCollection);
});
```

The default is `GeoJsonConstructionStyle.AsGeometryCollection`. If in this mode a circle-like structure is encountered,
an `InvalidOperationException` will be thrown.

Useful Links:

- [https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlgeographybuilder.aspx](SqlGeographyBuilder)
- [http://blogs.msdn.com/b/davidlean/archive/2008/10/30/sql-2008-spatial-samples-part-n-5-of-n-sql-builder-api.aspx](Overview of Methods to make your own Spatial Objects using the Builder API.)
