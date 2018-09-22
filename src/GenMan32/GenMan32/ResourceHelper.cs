namespace GenMan45
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class ResourceHelper
    {
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr BeginUpdateResource(string fileName, bool deleteExistingResource);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr FindResource(IntPtr hModule, int lpName, int lpType);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr FindResource(IntPtr hModule, int lpName, string lpType);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr FindResource(IntPtr hModule, string lpName, int lpType);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern void FreeLibrary(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string strLibrary);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern bool UpdateResource(IntPtr hUpdate, int lpType, int lpName, int wLanguage, byte[] data, int cbData);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern bool UpdateResource(IntPtr hUpdate, int lpType, string lpName, int wLanguage, byte[] data, int cbData);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern bool UpdateResource(IntPtr hUpdate, string lpType, int lpName, int wLanguage, byte[] data, int cbData);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern bool UpdateResource(IntPtr hUpdate, string lpType, string lpName, int wLanguage, byte[] data, int cbData);
        internal static void UpdateResourceInFile(string strAssemblyName, string strResourceFileName, int idType, int idRes)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr hUpdate = IntPtr.Zero;
            bool flag = false;
            bool flag2 = false;
            if (((strAssemblyName == null) || (strAssemblyName == string.Empty)) || (idRes < 1))
            {
                throw new ApplicationException("Invalid argument passed in UpdateResourceInFile!");
            }
            if ((strResourceFileName == null) || (strResourceFileName == string.Empty))
            {
                flag = true;
            }
            zero = LoadLibrary(strAssemblyName);
            if (zero == IntPtr.Zero)
            {
                throw new ApplicationException($"Cannot load {strAssemblyName}!");
            }
            if (FindResource(zero, idRes, idType) != IntPtr.Zero)
            {
                flag2 = true;
            }
            FreeLibrary(zero);
            if (flag2 && !flag)
            {
                throw new ApplicationException($"Resource already exists in {strAssemblyName}");
            }
            if (flag && !flag2)
            {
                throw new ApplicationException($"Resource does not exist in {strAssemblyName}");
            }
            FileStream stream = null;
            int count = 0;
            byte[] buffer = null;
            if (!flag)
            {
                try
                {
                    stream = File.OpenRead(strResourceFileName);
                    count = (int) stream.Length;
                    buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                    stream.Close();
                    stream = null;
                }
                catch (Exception)
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    throw new ApplicationException($"Error reading {strResourceFileName}");
                }
            }
            hUpdate = BeginUpdateResource(strAssemblyName, false);
            if (hUpdate == IntPtr.Zero)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            bool flag3 = true;
            if (!UpdateResource(hUpdate, idType, idRes, 0, buffer, count))
            {
                flag3 = false;
            }
            if (!EndUpdateResource(hUpdate, false))
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            if (!flag3)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
        }

        internal static void UpdateResourceInFile(string strAssemblyName, string strResourceFileName, int idType, string idRes)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr hUpdate = IntPtr.Zero;
            bool flag = false;
            bool flag2 = false;
            if (((strAssemblyName == null) || (strAssemblyName == string.Empty)) || (((idRes == null) || (idRes == string.Empty)) || (idType < 1)))
            {
                throw new ApplicationException("Invalid argument passed in UpdateResourceInFile!");
            }
            if ((strResourceFileName == null) || (strResourceFileName == string.Empty))
            {
                flag = true;
            }
            zero = LoadLibrary(strAssemblyName);
            if (zero == IntPtr.Zero)
            {
                throw new ApplicationException($"Cannot load {strAssemblyName}!");
            }
            if (FindResource(zero, idRes, idType) != IntPtr.Zero)
            {
                flag2 = true;
            }
            FreeLibrary(zero);
            if (flag2 && !flag)
            {
                throw new ApplicationException($"Resource already exists in {strAssemblyName}");
            }
            if (flag && !flag2)
            {
                throw new ApplicationException($"Resource does not exist in {strAssemblyName}");
            }
            FileStream stream = null;
            int count = 0;
            byte[] buffer = null;
            if (!flag)
            {
                try
                {
                    stream = File.OpenRead(strResourceFileName);
                    count = (int) stream.Length;
                    buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                    stream.Close();
                    stream = null;
                }
                catch (Exception)
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    throw new ApplicationException($"Error reading {strResourceFileName}");
                }
            }
            hUpdate = BeginUpdateResource(strAssemblyName, false);
            if (hUpdate == IntPtr.Zero)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            bool flag3 = true;
            if (!UpdateResource(hUpdate, idType, idRes, 0, buffer, count))
            {
                flag3 = false;
            }
            if (!EndUpdateResource(hUpdate, false))
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            if (!flag3)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
        }

        internal static void UpdateResourceInFile(string strAssemblyName, string strResourceFileName, string idType, int idRes)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr hUpdate = IntPtr.Zero;
            bool flag = false;
            bool flag2 = false;
            if (((strAssemblyName == null) || (strAssemblyName == string.Empty)) || (((idType == null) || (idType == string.Empty)) || (idRes < 1)))
            {
                throw new ApplicationException("Invalid argument passed in UpdateResourceInFile!");
            }
            if ((strResourceFileName == null) || (strResourceFileName == string.Empty))
            {
                flag = true;
            }
            zero = LoadLibrary(strAssemblyName);
            if (zero == IntPtr.Zero)
            {
                throw new ApplicationException($"Cannot load {strAssemblyName}!");
            }
            if (FindResource(zero, idRes, idType) != IntPtr.Zero)
            {
                flag2 = true;
            }
            FreeLibrary(zero);
            if (flag2 && !flag)
            {
                throw new ApplicationException($"Resource already exists in {strAssemblyName}");
            }
            if (flag && !flag2)
            {
                throw new ApplicationException($"Resource does not exist in {strAssemblyName}");
            }
            FileStream stream = null;
            int count = 0;
            byte[] buffer = null;
            if (!flag)
            {
                try
                {
                    stream = File.OpenRead(strResourceFileName);
                    count = (int) stream.Length;
                    buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                    stream.Close();
                    stream = null;
                }
                catch (Exception)
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    throw new ApplicationException($"Error reading {strResourceFileName}");
                }
            }
            hUpdate = BeginUpdateResource(strAssemblyName, false);
            if (hUpdate == IntPtr.Zero)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            bool flag3 = true;
            if (!UpdateResource(hUpdate, idType, idRes, 0, buffer, count))
            {
                flag3 = false;
            }
            if (!EndUpdateResource(hUpdate, false))
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            if (!flag3)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
        }

        internal static void UpdateResourceInFile(string strAssemblyName, string strResourceFileName, string idType, string idRes)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr hUpdate = IntPtr.Zero;
            bool flag = false;
            bool flag2 = false;
            if ((((strAssemblyName == null) || (strAssemblyName == string.Empty)) || ((idType == null) || (idType == string.Empty))) || ((idRes == null) || (idRes == string.Empty)))
            {
                throw new ApplicationException("Invalid argument passed in UpdateResourceInFile!");
            }
            if ((strResourceFileName == null) || (strResourceFileName == string.Empty))
            {
                flag = true;
            }
            zero = LoadLibrary(strAssemblyName);
            if (zero == IntPtr.Zero)
            {
                throw new ApplicationException($"Cannot load {strAssemblyName}!");
            }
            if (FindResource(zero, idRes, idType) != IntPtr.Zero)
            {
                flag2 = true;
            }
            FreeLibrary(zero);
            if (flag2 && !flag)
            {
                throw new ApplicationException($"Resource already exists in {strAssemblyName}");
            }
            if (flag && !flag2)
            {
                throw new ApplicationException($"Resource does not exist in {strAssemblyName}");
            }
            FileStream stream = null;
            int count = 0;
            byte[] buffer = null;
            if (!flag)
            {
                try
                {
                    stream = File.OpenRead(strResourceFileName);
                    count = (int) stream.Length;
                    buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                    stream.Close();
                    stream = null;
                }
                catch (Exception)
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    throw new ApplicationException($"Error reading {strResourceFileName}");
                }
            }
            hUpdate = BeginUpdateResource(strAssemblyName, false);
            if (hUpdate == IntPtr.Zero)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            bool flag3 = true;
            if (!UpdateResource(hUpdate, idType, idRes, 0, buffer, count))
            {
                flag3 = false;
            }
            if (!EndUpdateResource(hUpdate, false))
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
            if (!flag3)
            {
                throw new ApplicationException($"Error updating resource for {strAssemblyName}");
            }
        }
    }
}

