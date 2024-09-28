namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    // Parameter for MySqlDbType.Geometry
    public class MySqlGeometryParameter : SqlMapper.ICustomQueryParameter
    {
        private readonly string? _value;

        private bool _nullable = true;

        public string? Value => _value;

        public MySqlGeometryParameter(string? value, bool nullable = true)
        {
            if (!Empty(value))
                _value = value;
            _nullable = nullable;
        }

        public void AddParameter(IDbCommand command, string name)
        {
            command.Parameters.Add(new MySqlParameter
            {
                ParameterName = name,
                MySqlDbType = MySqlDbType.Geometry,
                Value = _value != null ? _value : (_nullable ? DBNull.Value : new byte[0])
            });
        }
    }
} // End Partial class
