namespace GenMan45
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class Win32ManifestGenerator : MarshalByRefObject
    {
        private static AssemblyResolver s_Resolver;

        private void AsmCreateWin32ManifestFile(Stream s, Assembly asm, bool bGenerateTypeLib, string strReferenceFiles)
        {
            string str = "<assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\">";
            string str2 = "</assembly>";
            this.WriteUTFChars(s, str + Environment.NewLine);
            this.WriteAsmIDElement(s, asm, 4);
            if (strReferenceFiles != null)
            {
                char[] separator = new char[] { '?' };
                foreach (string str3 in strReferenceFiles.Split(separator))
                {
                    if (str3 != string.Empty)
                    {
                        string assemblyIdentity = null;
                        try
                        {
                            assemblyIdentity = this.GetAssemblyIdentity(str3);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(Resource.FormatString("Msg_AsmIdentityError", str3, exception.Message));
                        }
                        if (assemblyIdentity != null)
                        {
                            char[] chArray2 = new char[] { '\r', '\n' };
                            string[] strArray2 = assemblyIdentity.Split(chArray2);
                            this.WriteUTFChars(s, "<dependency>" + Environment.NewLine, 4);
                            this.WriteUTFChars(s, "<dependentAssembly>" + Environment.NewLine, 8);
                            for (int j = 0; j < strArray2.Length; j++)
                            {
                                if (strArray2[j] != string.Empty)
                                {
                                    int offset = 8;
                                    if (j == 0)
                                    {
                                        offset = 12;
                                    }
                                    this.WriteUTFChars(s, strArray2[j] + Environment.NewLine, offset);
                                }
                            }
                            this.WriteUTFChars(s, "</dependentAssembly>" + Environment.NewLine, 8);
                            this.WriteUTFChars(s, "</dependency>" + Environment.NewLine, 4);
                        }
                    }
                }
            }
            RegistrationServices regServices = new RegistrationServices();
            string imageRuntimeVersion = asm.ImageRuntimeVersion;
            Module[] modules = asm.GetModules();
            foreach (Module module in modules)
            {
                this.WriteTypes(s, module, asm, imageRuntimeVersion, regServices, bGenerateTypeLib, 4);
            }
            for (int i = 0; i < modules.Length; i++)
            {
                if ((i == 0) && bGenerateTypeLib)
                {
                    this.WriteFileElement(s, modules[i], asm, 4);
                }
                else
                {
                    this.WriteFileElement(s, modules[i], 4);
                }
            }
            this.WriteUTFChars(s, str2);
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr FindResource(IntPtr hInst, int idType, int idRes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern void FreeLibrary(IntPtr ptr);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern void FreeResource(IntPtr hGlobal);
        internal void GenerateWin32ManifestFile(string strAssemblyManifestFileName, string strAssemblyName, bool bGenerateTypeLib, string strReferenceFiles, string strAsmPath)
        {
            Stream s = null;
            string str = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            s_Resolver = new AssemblyResolver(Path.GetDirectoryName(strAssemblyName), strAsmPath);
            Assembly asm = null;
            asm = Assembly.ReflectionOnlyLoadFrom(strAssemblyName);
            string directoryName = Path.GetDirectoryName(strAssemblyManifestFileName);
            if ((directoryName != "") && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(strAssemblyManifestFileName));
            }
            s = File.Create(strAssemblyManifestFileName);
            try
            {
                this.WriteUTFChars(s, str + Environment.NewLine);
                this.AsmCreateWin32ManifestFile(s, asm, bGenerateTypeLib, strReferenceFiles);
            }
            catch (Exception exception)
            {
                s.Close();
                File.Delete(strAssemblyManifestFileName);
                throw exception;
            }
            s.Close();
        }

        private string GetAssemblyIdentity(string fileName)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr hRes = IntPtr.Zero;
            IntPtr hGlobal = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            string str = null;
            try
            {
                zero = LoadLibrary(fileName);
                if (zero == IntPtr.Zero)
                {
                    throw new ApplicationException(Resource.FormatString("Err_LoadLibraryFailed", fileName));
                }
                hRes = FindResource(zero, 1, 0x18);
                if (hRes == IntPtr.Zero)
                {
                    throw new ApplicationException(Resource.FormatString("Err_NoWin32Manifest", fileName));
                }
                hGlobal = LoadResource(zero, hRes);
                if (hGlobal == IntPtr.Zero)
                {
                    throw new ApplicationException(Resource.FormatString("Err_LoadWin32ManifestFailed", fileName));
                }
                ptr = LockResource(hGlobal);
                if (SizeofResource(zero, hRes) == 0)
                {
                    throw new ApplicationException(Resource.FormatString("Err_EmptyWin32Manifest", fileName));
                }
                string str2 = Marshal.PtrToStringAnsi(ptr);
                int index = str2.IndexOf("<assemblyIdentity", 0);
                int num3 = str2.IndexOf("/>", index);
                str = str2.Substring(index, (num3 + 2) - index);
            }
            finally
            {
                if (hGlobal != IntPtr.Zero)
                {
                    FreeResource(hGlobal);
                }
                if (zero != IntPtr.Zero)
                {
                    FreeLibrary(zero);
                }
            }
            return str;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string strLibrary);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr LoadResource(IntPtr hInst, IntPtr hRes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr LockResource(IntPtr hGlobal);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern int SizeofResource(IntPtr hInst, IntPtr hRes);
        private void WriteAsmIDElement(Stream s, Assembly assembly, int offset)
        {
            AssemblyName name = assembly.GetName();
            string str = name.Version.ToString();
            string str2 = name.Name;
            byte[] publicKeyToken = name.GetPublicKeyToken();
            string str3 = name.CultureInfo.ToString();
            ProcessorArchitecture processorArchitecture = name.ProcessorArchitecture;
            this.WriteUTFChars(s, "<assemblyIdentity" + Environment.NewLine, offset);
            this.WriteUTFChars(s, "    name=\"" + str2 + "\"" + Environment.NewLine, offset);
            this.WriteUTFChars(s, "    version=\"" + str + "\"", offset);
            if ((publicKeyToken != null) && (publicKeyToken.Length != 0))
            {
                this.WriteUTFChars(s, Environment.NewLine);
                this.WriteUTFChars(s, "    publicKeyToken=\"", offset);
                this.WriteUTFChars(s, publicKeyToken);
                this.WriteUTFChars(s, "\"");
            }
            if (processorArchitecture != ProcessorArchitecture.None)
            {
                this.WriteUTFChars(s, Environment.NewLine);
                this.WriteUTFChars(s, "    processorArchitecture=\"", offset);
                this.WriteUTFChars(s, processorArchitecture.ToString());
                this.WriteUTFChars(s, "\"");
            }
            if (str3 == "")
            {
                this.WriteUTFChars(s, " />" + Environment.NewLine);
            }
            else
            {
                this.WriteUTFChars(s, Environment.NewLine);
                this.WriteUTFChars(s, "    language=\"" + str3 + "\" />" + Environment.NewLine, offset);
            }
        }

        private void WriteFileElement(Stream s, Module m, int offset)
        {
            this.WriteUTFChars(s, "<file ", offset);
            this.WriteUTFChars(s, "name=\"" + m.Name + "\">" + Environment.NewLine);
            this.WriteUTFChars(s, "</file>" + Environment.NewLine, offset);
        }

        private void WriteFileElement(Stream s, Module m, Assembly asm, int offset)
        {
            this.WriteUTFChars(s, "<file ", offset);
            this.WriteUTFChars(s, "name=\"" + m.Name + "\">" + Environment.NewLine);
            Version version = asm.GetName().Version;
            string directoryName = Path.GetDirectoryName(asm.Location);
            string str2 = version.Major.ToString() + "." + version.Minor.ToString();
            string str3 = "{" + Marshal.GetTypeLibGuidForAssembly(asm).ToString().ToUpper() + "}";
            this.WriteUTFChars(s, "<typelib" + Environment.NewLine, offset + 4);
            this.WriteUTFChars(s, "tlbid=\"" + str3 + "\"" + Environment.NewLine, offset + 8);
            this.WriteUTFChars(s, "version=\"" + str2 + "\"" + Environment.NewLine, offset + 8);
            this.WriteUTFChars(s, "helpdir=\"" + directoryName + "\" />" + Environment.NewLine, offset + 8);
            this.WriteUTFChars(s, "</file>" + Environment.NewLine, offset);
        }

        private void WriteTypes(Stream s, Module m, Assembly asm, string strRuntimeVersion, RegistrationServices regServices, bool bGenerateTypeLib, int offset)
        {
            string fullName = null;
            string str2 = "{" + Marshal.GetTypeLibGuidForAssembly(asm).ToString().ToUpper() + "}";
            foreach (Type type in m.GetTypes())
            {
                if (regServices.TypeRequiresRegistration(type))
                {
                    string str3 = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper() + "}";
                    fullName = type.FullName;
                    if (regServices.TypeRepresentsComType(type) || type.IsValueType)
                    {
                        this.WriteUTFChars(s, "<clrSurrogate" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "    clsid=\"" + str3 + "\"" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "    name=\"" + fullName + "\">" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "</clrSurrogate>" + Environment.NewLine, offset);
                    }
                    else
                    {
                        string str4 = Marshal.GenerateProgIdForType(type);
                        this.WriteUTFChars(s, "<clrClass" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "    clsid=\"" + str3 + "\"" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "    progid=\"" + str4 + "\"" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "    threadingModel=\"Both\"" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "    name=\"" + fullName + "\"" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "    runtimeVersion=\"" + strRuntimeVersion + "\">" + Environment.NewLine, offset);
                        this.WriteUTFChars(s, "</clrClass>" + Environment.NewLine, offset);
                    }
                }
                else if ((bGenerateTypeLib && type.IsInterface) && (type.IsPublic && !type.IsImport))
                {
                    string str5 = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper() + "}";
                    this.WriteUTFChars(s, "<comInterfaceExternalProxyStub" + Environment.NewLine, offset);
                    this.WriteUTFChars(s, "iid=\"" + str5 + "\"" + Environment.NewLine, offset + 4);
                    this.WriteUTFChars(s, "name=\"" + type.Name + "\"" + Environment.NewLine, offset + 4);
                    this.WriteUTFChars(s, string.Concat(new object[] { "numMethods=\"", type.GetMethods().Length, "\"", Environment.NewLine }), offset + 4);
                    this.WriteUTFChars(s, "proxyStubClsid32=\"{00020424-0000-0000-C000-000000000046}\"" + Environment.NewLine, offset + 4);
                    this.WriteUTFChars(s, "tlbid=\"" + str2 + "\" />" + Environment.NewLine, offset + 4);
                }
            }
        }

        private void WriteUTFChars(Stream s, byte[] bytes)
        {
            foreach (byte num in bytes)
            {
                this.WriteUTFChars(s, num.ToString("x2"));
            }
        }

        private void WriteUTFChars(Stream s, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            s.Write(bytes, 0, bytes.Length);
        }

        private void WriteUTFChars(Stream s, string value, int offset)
        {
            for (int i = 0; i < offset; i++)
            {
                this.WriteUTFChars(s, " ");
            }
            this.WriteUTFChars(s, value);
        }
    }
}

