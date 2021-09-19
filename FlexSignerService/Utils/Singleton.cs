using System;

namespace FlexSignerService
{
    public class GenericSingleton<T> where T : class, new()
    {
        private static T instance;

        private static readonly object _lockingObject = new object();

        public static T GetInstance()
        {
            lock (_lockingObject)
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }

        public static T GetInstance(params dynamic[] param)
        {
            if (instance == null)
            {
                lock (_lockingObject)
                {
                    if (instance == null)
                    {
                        instance = (T)Activator.CreateInstance(typeof(T), param);
                    }
                }
            }
            return instance;
        }
    }
}