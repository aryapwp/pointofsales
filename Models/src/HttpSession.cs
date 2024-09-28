namespace ASPNETMaker2024.Models;

// Partial class
public partial class PointofSales {
    /// <summary>
    /// Session class
    /// </summary>
    public class HttpSession
    {
        // Serializer settings
        public static JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

        // Keys
        public IEnumerable<string>? Keys => _session?.Keys;

        // Session ID
        public string SessionId => _session?.Id ?? "";

        // Get session
        private ISession? _session => UseSession ? HttpContext?.Session : null; // No session for external use of API

        // Remove
        public HttpSession Remove(string key)
        {
            _session?.Remove(key);
            return this;
        }

        // Clear
        public void Clear() => _session?.Clear();

        // Get value as bytes
        public byte[]? GetBytes(string key) => _session?.Get(key);

        // Set value as bytes
        public HttpSession SetBytes(string key, byte[] value)
        {
            _session?.Set(key, value);
            return this;
        }

        // Get value as string
        public string GetString(string key) => _session?.GetString(key) ?? "";

        // Set value as bytes
        public HttpSession SetString(string key, string value)
        {
            _session?.SetString(key, value);
            return this;
        }

        // Try get value as string
        public bool TryGetValue(string name, [NotNullWhen(true)] out string? value)
        {
            value = _session?.GetString(name);
            return value != null;
        }

        // Try get value as object
        public bool TryGetValue(string name, [NotNullWhen(true)] out object? value)
        {
            value = GetValue(name);
            return value != null;
        }

        // Get value as byte[]?
        public byte[]? Get(string key) => _session?.Get(key);

        // Get value as T
        public T? Get<T>(string key) => ChangeType<T>(_session?.GetString(key));

        // Get value as int32
        public int GetInt(string key) => _session?.GetInt32(key) ?? 0;

        // Get value as int32
        public int? GetInt32(string key) => _session?.GetInt32(key);

        // Set value as int32
        public HttpSession SetInt(string key, int value)
        {
            _session?.SetInt32(key, value);
            return this;
        }

        // Serialize and set
        public HttpSession SetValue(string key, object? value)
        {
            SetValue(key, value, SerializerSettings);
            return this;
        }

        // Serialize and set
        public HttpSession SetValue(string key, object? value, JsonSerializerSettings settings) =>
            value == null ? Remove(key) : SetString(key, JsonConvert.SerializeObject(value, settings));

        // Get as deserialized object (TypeNameHandling.All)
        public object? GetValue(string key) => GetValue(key, SerializerSettings);

        // Get as deserialized object
        public object? GetValue(string key, JsonSerializerSettings settings)
        {
            try {
                var data = _session?.GetString(key);
                if (data != null)
                    return JsonConvert.DeserializeObject(data, settings);
            } catch {}
            return null;
        }

        // Get as deserialized type T (TypeNameHandling.All)
        public T? GetValue<T>(string key) => GetValue<T>(key, SerializerSettings);

        // Get as deserialized type T
        public T? GetValue<T>(string key, JsonSerializerSettings settings)
        {
            try {
                var data = _session?.GetString(key);
                if (data != null)
                    return JsonConvert.DeserializeObject<T>(data, settings);
            } catch {}
            return default(T);
        }

        // Get value as bool
        public bool GetBool(string key) => ConvertToBool(GetValue(key));

        // Set value as bool
        public HttpSession SetBool(string key, bool value) => SetValue(key, value);

        // Get/Set as object (string)
        public object? this[string name]
        {
            get => _session?.GetString(name);
            set => _session?.SetString(name, ConvertToString(value));
        }

        // Commit
        public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
            _session?.CommitAsync(cancellationToken) ?? Task.CompletedTask;

        // Commit
        public void Commit() => CommitAsync().GetAwaiter().GetResult();
    }
} // End Partial class
