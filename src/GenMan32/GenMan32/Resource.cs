namespace GenMan45
{
    using System;
    using System.Reflection;
    using System.Resources;

    internal class Resource
    {
        private static ResourceManager _resmgr;

        internal static string FormatString(string key) => 
            GetString(key);

        internal static string FormatString(string key, object a1) => 
            string.Format(GetString(key), a1);

        internal static string FormatString(string key, object[] a) => 
            string.Format(GetString(key), a);

        internal static string FormatString(string key, object a1, object a2) => 
            string.Format(GetString(key), a1, a2);

        internal static string FormatString(string key, object a1, object a2, object a3) => 
            string.Format(GetString(key), a1, a2, a3);

        internal static string GetString(string key)
        {
            InitResourceManager();
            string str = _resmgr.GetString(key, null);
            if (str == null)
            {
                throw new ApplicationException("FATAL: Resource string for '" + key + "' is null");
            }
            return str;
        }

        private static void InitResourceManager()
        {
            if (_resmgr == null)
            {
                _resmgr = new ResourceManager("GenMan32", Assembly.GetAssembly(typeof(global::GenMan45.GenMan32)));
            }
        }
    }
}

