namespace GenMan45
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Text;

    internal class GenMan32
    {
        private const int SuccessReturnCode = 0;
        private const int ErrorReturnCode = 100;
        private const int MAX_PATH = 260;
        internal static GenMan32Options s_Options;

        private static void AddManifestToAssembly(string strManifestFileName, string strAssemblyName)
        {
            global::GenMan45.ResourceHelper.UpdateResourceInFile(strAssemblyName, strManifestFileName, 0x18, 1);
        }

        private static void EmbedTypeLibInAssembly(string strTlbName, string strAssemblyName)
        {
            try
            {
                global::GenMan45.ResourceHelper.UpdateResourceInFile(strAssemblyName, strTlbName, "TYPELIB", 1);
                if (!s_Options.m_bSilentMode)
                {
                    Console.WriteLine(Resource.FormatString("Msg_TlbEmbedded", strAssemblyName));
                }
            }
            finally
            {
                File.Delete(strTlbName);
            }
        }

        private static string GenerateTypeLibUsingHelper(string strAssemblyName)
        {
            string directoryName = Path.GetDirectoryName(strAssemblyName);
            string strTlbName = null;
            AppDomainSetup info = new AppDomainSetup {
                ApplicationBase = directoryName
            };
            AppDomain domain = AppDomain.CreateDomain("GenMan32", null, info);
            if (domain == null)
            {
                throw new ApplicationException(Resource.FormatString("Err_CannotCreateAppDomain"));
            }
            try
            {
                ObjectHandle handle = domain.CreateInstanceFrom(Assembly.GetExecutingAssembly().CodeBase, typeof(TypeLibGenerator).FullName);
                if (handle == null)
                {
                    throw new ApplicationException(Resource.FormatString("Err_CannotCreateRemoteTypeLibGenerator"));
                }
                ((TypeLibGenerator) handle.Unwrap()).GenerateTypeLib(strAssemblyName, ref strTlbName);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
            if ((strTlbName != null) && !s_Options.m_bSilentMode)
            {
                Console.WriteLine(Resource.FormatString("Msg_TlbGenerated", strAssemblyName));
            }
            return strTlbName;
        }

        private static void GenerateWin32ManifestFileUsingHelper(string strAssemblyManifestFileName, string strAssemblyName, bool bGenerateTypeLib, string strReferenceFiles, string strAsmPath)
        {
            string directoryName = Path.GetDirectoryName(strAssemblyName);
            AppDomainSetup info = new AppDomainSetup {
                ApplicationBase = directoryName
            };
            AppDomain domain = AppDomain.CreateDomain("GenMan32", null, info);
            if (domain == null)
            {
                throw new ApplicationException(Resource.FormatString("Err_CannotCreateAppDomain"));
            }
            try
            {
                ObjectHandle handle = domain.CreateInstanceFrom(Assembly.GetExecutingAssembly().CodeBase, typeof(Win32ManifestGenerator).FullName);
                if (handle == null)
                {
                    throw new ApplicationException(Resource.FormatString("Err_CannotCreateRemoteWin32ManGen"));
                }
                ((Win32ManifestGenerator) handle.Unwrap()).GenerateWin32ManifestFile(strAssemblyManifestFileName, strAssemblyName, bGenerateTypeLib, strReferenceFiles, strAsmPath);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        private static int Main(string[] args)
        {
            int returnCode = 0;
            if (!ParseArguments(args, ref s_Options, ref returnCode))
            {
                return returnCode;
            }
            if (!s_Options.m_bSilentMode)
            {
                PrintLogo();
            }
            return Run();
        }

        private static bool ParseArguments(string[] args, ref GenMan32Options Options, ref int ReturnCode)
        {
            CommandLine line;
            Option option;
            StringBuilder builder = null;
            Options = new GenMan32Options();
            try
            {
                line = new CommandLine(args, new string[] { "add", "+out", "remove", "replace", "typelib", "*asmpath", "*reference", "*manifest", "nologo", "silent", "?", "help" });
            }
            catch (ApplicationException exception)
            {
                PrintLogo();
                WriteErrorMsg(null, exception);
                ReturnCode = 100;
                return false;
            }
            if ((line.NumArgs + line.NumOpts) < 1)
            {
                PrintUsage();
                ReturnCode = 100;
                return false;
            }
            Options.m_strAssemblyName = line.GetNextArg();
            while ((option = line.GetNextOption()) != null)
            {
                if (option.Name.Equals("add"))
                {
                    Options.m_bAddManifest = true;
                }
                else
                {
                    if (option.Name.Equals("out"))
                    {
                        Options.m_strOutputFileName = option.Value;
                        continue;
                    }
                    if (option.Name.Equals("manifest"))
                    {
                        Options.m_strInputManifestFile = option.Value;
                        continue;
                    }
                    if (option.Name.Equals("nologo"))
                    {
                        Options.m_bNoLogo = true;
                        continue;
                    }
                    if (option.Name.Equals("silent"))
                    {
                        Options.m_bSilentMode = true;
                        continue;
                    }
                    if (option.Name.Equals("remove"))
                    {
                        Options.m_bRemoveManifest = true;
                        continue;
                    }
                    if (option.Name.Equals("replace"))
                    {
                        Options.m_bReplaceManifest = true;
                        continue;
                    }
                    if (option.Name.Equals("typelib"))
                    {
                        Options.m_bGenerateTypeLib = true;
                        continue;
                    }
                    if (option.Name.Equals("asmpath"))
                    {
                        Options.m_strAsmPath = option.Value;
                        continue;
                    }
                    if (option.Name.Equals("reference"))
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(option.Value);
                        }
                        else
                        {
                            builder.Append("?");
                            builder.Append(option.Value);
                        }
                        Options.m_strReferenceFiles = builder.ToString();
                        continue;
                    }
                    if (option.Name.Equals("?") || option.Name.Equals("help"))
                    {
                        PrintUsage();
                        ReturnCode = 0;
                        return false;
                    }
                    PrintLogo();
                    WriteErrorMsg(Resource.FormatString("Err_InvalidOption"));
                    ReturnCode = 100;
                    return false;
                }
            }
            if (Options.m_strAssemblyName == null)
            {
                PrintLogo();
                WriteErrorMsg(Resource.FormatString("Err_NoInputFile"));
                ReturnCode = 100;
                return false;
            }
            if ((Options.m_strInputManifestFile != null) && (Options.m_strOutputFileName != null))
            {
                PrintLogo();
                WriteErrorMsg(Resource.FormatString("Err_InvalidManAndOutCombination"));
                ReturnCode = 100;
                return false;
            }
            if (((Options.m_bAddManifest && (Options.m_bRemoveManifest || Options.m_bReplaceManifest)) || (Options.m_bRemoveManifest && (Options.m_bAddManifest || Options.m_bReplaceManifest))) || (Options.m_bReplaceManifest && (Options.m_bAddManifest || Options.m_bRemoveManifest)))
            {
                PrintLogo();
                WriteErrorMsg(Resource.FormatString("Err_InvalidAddRemReplaceCombination"));
                ReturnCode = 100;
                return false;
            }
            if (Options.m_bRemoveManifest && (Options.m_strInputManifestFile != null))
            {
                PrintLogo();
                WriteErrorMsg(Resource.FormatString("Err_InvalidRemManCombination"));
                ReturnCode = 100;
                return false;
            }
            if (Options.m_bReplaceManifest && (Options.m_strInputManifestFile == null))
            {
                PrintLogo();
                WriteErrorMsg(Resource.FormatString("Err_ReplaceRequiresNewMan"));
                ReturnCode = 100;
                return false;
            }
            if (Options.m_bGenerateTypeLib && !Options.m_bAddManifest)
            {
                PrintLogo();
                WriteErrorMsg(Resource.FormatString("Err_TypeLibRequiresAddMan"));
                ReturnCode = 100;
                return false;
            }
            return true;
        }

        private static void PrintLogo()
        {
            if (!s_Options.m_bNoLogo)
            {
                Console.WriteLine(Resource.FormatString("Msg_Copyright", "2.0.60120.0"));
            }
        }

        private static void PrintUsage()
        {
            PrintLogo();
            Console.WriteLine(Resource.FormatString("Msg_Usage"));
        }

        private static void RemoveManifestFromAssembly(string strAssemblyName)
        {
            global::GenMan45.ResourceHelper.UpdateResourceInFile(strAssemblyName, null, 0x18, 1);
        }

        private static void ReplaceManifestInAssembly(string strManifestFileName, string strAssemblyName)
        {
            global::GenMan45.ResourceHelper.UpdateResourceInFile(strAssemblyName, null, 0x18, 1);
            global::GenMan45.ResourceHelper.UpdateResourceInFile(strAssemblyName, strManifestFileName, 0x18, 1);
        }

        private static void ReportResult()
        {
            if (!s_Options.m_bSilentMode)
            {
                if (s_Options.m_bAddManifest)
                {
                    Console.WriteLine(Resource.FormatString("Msg_ManifestAdded", s_Options.m_strAssemblyName));
                }
                else if (s_Options.m_bRemoveManifest)
                {
                    Console.WriteLine(Resource.FormatString("Msg_ManifestRemoved", s_Options.m_strAssemblyName));
                }
                else if (s_Options.m_bReplaceManifest)
                {
                    Console.WriteLine(Resource.FormatString("Msg_ManifestReplaced", s_Options.m_strAssemblyName));
                }
                else
                {
                    Console.WriteLine(Resource.FormatString("Msg_ManifestCreated", s_Options.m_strOutputFileName));
                }
            }
        }

        public static int Run()
        {
            int num = 0;
            try
            {
                string path = null;
                string fullPath = Path.GetFullPath(s_Options.m_strAssemblyName);
                if (!File.Exists(fullPath))
                {
                    StringBuilder buffer = new StringBuilder(0x105);
                    if (SearchPath(null, s_Options.m_strAssemblyName, null, buffer.Capacity + 1, buffer, null) == 0)
                    {
                        throw new ApplicationException(Resource.FormatString("Err_InputFileNotFound", s_Options.m_strAssemblyName));
                    }
                    s_Options.m_strAssemblyName = buffer.ToString();
                }
                else
                {
                    s_Options.m_strAssemblyName = fullPath;
                }
                if (s_Options.m_strOutputFileName != null)
                {
                    s_Options.m_strOutputFileName = new FileInfo(s_Options.m_strOutputFileName).FullName;
                    path = s_Options.m_strOutputFileName;
                }
                else
                {
                    path = new FileInfo(new FileInfo(s_Options.m_strAssemblyName).Name).FullName + ".manifest";
                }
                if (s_Options.m_strInputManifestFile != null)
                {
                    s_Options.m_strInputManifestFile = new FileInfo(s_Options.m_strInputManifestFile).FullName;
                }
                string strTlbName = null;
                if (s_Options.m_bGenerateTypeLib)
                {
                    strTlbName = GenerateTypeLibUsingHelper(s_Options.m_strAssemblyName);
                }
                if (strTlbName != null)
                {
                    EmbedTypeLibInAssembly(strTlbName, s_Options.m_strAssemblyName);
                }
                if (s_Options.m_bAddManifest)
                {
                    if (s_Options.m_strInputManifestFile != null)
                    {
                        AddManifestToAssembly(s_Options.m_strInputManifestFile, s_Options.m_strAssemblyName);
                    }
                    else
                    {
                        if (s_Options.m_strOutputFileName != null)
                        {
                            path = s_Options.m_strOutputFileName;
                            if (File.Exists(path))
                            {
                                throw new ApplicationException(Resource.FormatString("Err_OutputManFileExists", path));
                            }
                        }
                        else
                        {
                            int num2 = 0;
                            string str5 = path + ".tmp";
                            while (File.Exists(str5))
                            {
                                str5 = path + ".tmp" + num2++;
                            }
                            path = str5;
                        }
                        GenerateWin32ManifestFileUsingHelper(path, s_Options.m_strAssemblyName, s_Options.m_bGenerateTypeLib, s_Options.m_strReferenceFiles, s_Options.m_strAsmPath);
                        try
                        {
                            AddManifestToAssembly(path, s_Options.m_strAssemblyName);
                        }
                        catch (Exception exception)
                        {
                            if (s_Options.m_strOutputFileName == null)
                            {
                                File.Delete(path);
                            }
                            throw exception;
                        }
                        if (s_Options.m_strOutputFileName == null)
                        {
                            File.Delete(path);
                        }
                    }
                }
                else if (s_Options.m_bRemoveManifest)
                {
                    RemoveManifestFromAssembly(s_Options.m_strAssemblyName);
                }
                else if (s_Options.m_bReplaceManifest)
                {
                    ReplaceManifestInAssembly(s_Options.m_strInputManifestFile, s_Options.m_strAssemblyName);
                }
                else
                {
                    if (s_Options.m_strOutputFileName != null)
                    {
                        path = s_Options.m_strOutputFileName;
                    }
                    else
                    {
                        s_Options.m_strOutputFileName = path;
                    }
                    if (File.Exists(path))
                    {
                        throw new ApplicationException(Resource.FormatString("Err_ManFileExists", path));
                    }
                    GenerateWin32ManifestFileUsingHelper(path, s_Options.m_strAssemblyName, s_Options.m_bGenerateTypeLib, s_Options.m_strReferenceFiles, s_Options.m_strAsmPath);
                }
                ReportResult();
            }
            catch (ReflectionTypeLoadException exception2)
            {
                WriteErrorMsg(Resource.FormatString("Err_TypeLoadExceptions"));
                Exception[] loaderExceptions = exception2.LoaderExceptions;
                for (int i = 0; i < loaderExceptions.Length; i++)
                {
                    try
                    {
                        Console.Error.WriteLine(Resource.FormatString("Msg_DisplayException", i, loaderExceptions[i]));
                    }
                    catch (Exception exception3)
                    {
                        Console.Error.WriteLine(Resource.FormatString("Msg_DisplayNestedException", i, exception3));
                    }
                }
                num = 100;
            }
            catch (Exception exception4)
            {
                WriteErrorMsg(null, exception4);
                num = 100;
            }
            return num;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int SearchPath(string path, string fileName, string extension, int numBufferChars, StringBuilder buffer, int[] filePart);
        private static void WriteErrorMsg(string strErrorMsg)
        {
            Console.Error.WriteLine(Resource.FormatString("Msg_GenMan32ErrorPrefix", strErrorMsg));
        }

        private static void WriteErrorMsg(string strPrefix, Exception e)
        {
            string str = "";
            if (strPrefix != null)
            {
                str = strPrefix;
            }
            if (e.Message != null)
            {
                str = str + e.Message;
            }
            else
            {
                str = str + e.GetType().ToString();
            }
            Console.Error.WriteLine(Resource.FormatString("Msg_GenMan32ErrorPrefix", str));
        }
    }
}

