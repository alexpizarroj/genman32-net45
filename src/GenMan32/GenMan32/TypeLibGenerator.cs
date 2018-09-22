namespace GenMan45
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class TypeLibGenerator : MarshalByRefObject
    {
        internal static ITypeLib GenerateTypeLib(Assembly asm, ref string strTlbName)
        {
            ITypeLibConverter converter = new TypeLibConverter();
            ExporterCallback notifySink = new ExporterCallback();
            ITypeLib lib = (ITypeLib) converter.ConvertAssemblyToTypeLib(asm, strTlbName, TypeLibExporterFlags.None, notifySink);
            ((ICreateITypeLib) lib).SaveAllChanges();
            return lib;
        }

        internal ITypeLib GenerateTypeLib(string strAssemblyName, ref string strTlbName)
        {
            ITypeLib embeddedTlb = GetEmbeddedTlb(strAssemblyName);
            if (embeddedTlb != null)
            {
                strTlbName = null;
                return embeddedTlb;
            }
            Assembly asm = Assembly.ReflectionOnlyLoadFrom(strAssemblyName);
            if (asm.GetCustomAttributes(typeof(ImportedFromTypeLibAttribute), true).Length > 0)
            {
                throw new ApplicationException(Resource.FormatString("Err_ImportedFromTypeLib", strAssemblyName));
            }
            if (strTlbName == null)
            {
                string directoryName = Path.GetDirectoryName(asm.Location);
                strTlbName = Path.Combine(directoryName, asm.GetName().Name) + ".tlb.tmp";
            }
            return GenerateTypeLib(asm, ref strTlbName);
        }

        internal static ITypeLib GetEmbeddedTlb(string strFileName)
        {
            ITypeLib typeLib = null;
            try
            {
                LoadTypeLibEx(strFileName, REGKIND.REGKIND_NONE, out typeLib);
            }
            catch (Exception)
            {
            }
            return typeLib;
        }

        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
        private static extern void LoadTypeLibEx(string strTypeLibName, REGKIND regKind, out ITypeLib TypeLib);
    }
}

