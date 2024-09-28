namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {

    public class GeometryTypeMapper<TGeometry> : SqlMapper.TypeHandler<TGeometry>
        where TGeometry : NetTopologySuite.Geometries.Geometry
    {
        public override void SetValue(IDbDataParameter parameter, TGeometry? value) {
            if (parameter is MySqlParameter mysqlParameter) {
                mysqlParameter.MySqlDbType = MySqlDbType.Geometry;
                mysqlParameter.Value = BitConverter.GetBytes(value.SRID).Concat(value.AsBinary()).ToArray();
                return;
            }
            throw new ArgumentException();
        }

        public override TGeometry? Parse(object value) {
            if (value is TGeometry geometry) {
                return geometry;
            }
            if (value is byte[] bytes) { // MySQL geometry
                var myGeom = new MySqlGeometry(MySqlDbType.Geometry, bytes);
                var geom = new NetTopologySuite.IO.WKBReader().Read(bytes.Skip(4).ToArray()); // The first 4 bytes is SRID
                geom.SRID = myGeom.SRID ?? 0;
                return (TGeometry)geom;
            }
            throw new ArgumentException();
        }
    }

    public class GeometryTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.Geometry>
    {
    }

    public class PointTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.Point>
    {
    }

    public class PolygonTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.Polygon>
    {
    }

    public class LineStringTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.LineString>
    {
    }

    public class GeometryCollectionTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.GeometryCollection>
    {
    }

    public class MultiPointTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.MultiPoint>
    {
    }

    public class MultiPolygonTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.MultiPolygon>
    {
    }

    public class MultiLineStringTypeMapper : GeometryTypeMapper<NetTopologySuite.Geometries.MultiLineString>
    {
    }
} // End Partial class
