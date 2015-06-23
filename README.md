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
