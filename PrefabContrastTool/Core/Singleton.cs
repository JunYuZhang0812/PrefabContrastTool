public class Singleton<T> where T : new()
{
    /// <summary>
    /// 对象的实例
    /// </summary>
    protected static readonly T _Instance = new T();
    /// <summary>
    /// 获取对象的实例
    /// </summary>
    public static T Instance
    {
        get
        {
            return _Instance;
        }
    }
}
