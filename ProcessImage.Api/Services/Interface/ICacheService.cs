namespace ProcessImage.Services.Interface
{
    public interface ICacheService
    {
        T Get<T>(string key);
        bool TryGet<T>(string key, out T value);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        void Update<T>(string key, T value, TimeSpan? expiration = null);
        void Remove(string key);
        void RemoveByPattern(string pattern);
        void Clear();
        bool Exists(string key);
    }
}
