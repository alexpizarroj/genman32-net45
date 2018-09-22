namespace GenMan45
{
    using System;

    [Serializable]
    internal sealed class GenMan32Options
    {
        public string m_strAssemblyName;
        public string m_strTypeLibName;
        public string m_strOutputFileName;
        public string m_strInputManifestFile;
        public string m_strReferenceFiles;
        public string m_strAsmPath;
        public bool m_bSilentMode;
        public bool m_bNoLogo;
        public bool m_bAddManifest;
        public bool m_bRemoveManifest;
        public bool m_bReplaceManifest;
        public bool m_bGenerateTypeLib;
    }
}

