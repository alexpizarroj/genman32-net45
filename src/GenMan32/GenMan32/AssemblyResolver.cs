namespace GenMan45
{
    using System;
    using System.IO;
    using System.Reflection;

    internal class AssemblyResolver
    {
        private string m_sourceAsmDir;
        private string[] m_lstPaths;

        public AssemblyResolver(string sourceAsmDir, string asmpaths)
        {
            this.m_sourceAsmDir = sourceAsmDir;
            if (!string.IsNullOrEmpty(asmpaths))
            {
                this.m_lstPaths = asmpaths.Split(new char[] { ';' });
            }
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(this.ResolveAssembly);
        }

        public Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            string str;
            AssemblyName name = new AssemblyName(args.Name);
            if (!string.IsNullOrEmpty(this.m_sourceAsmDir))
            {
                str = this.m_sourceAsmDir + @"\" + name.Name + ".dll";
                if (File.Exists(str))
                {
                    return Assembly.ReflectionOnlyLoadFrom(str);
                }
                str = this.m_sourceAsmDir + @"\" + name.Name + ".exe";
                if (File.Exists(str))
                {
                    return Assembly.ReflectionOnlyLoadFrom(str);
                }
            }
            if (this.m_lstPaths == null)
            {
                return Assembly.ReflectionOnlyLoad(args.Name);
            }
            foreach (string str2 in this.m_lstPaths)
            {
                str = str2 + @"\" + name.Name + ".dll";
                if (File.Exists(str))
                {
                    return Assembly.ReflectionOnlyLoadFrom(str);
                }
                str = str2 + @"\" + name.Name + ".exe";
                if (File.Exists(str))
                {
                    return Assembly.ReflectionOnlyLoadFrom(str);
                }
            }
            return null;
        }
    }
}

