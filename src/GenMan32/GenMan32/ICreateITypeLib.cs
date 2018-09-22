namespace GenMan45
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, ComVisible(false), Guid("00020406-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICreateITypeLib
    {
        void CreateTypeInfo();
        void SetName();
        void SetVersion();
        void SetGuid();
        void SetDocString();
        void SetHelpFileName();
        void SetHelpContext();
        void SetLcid();
        void SetLibFlags();
        void SaveAllChanges();
    }
}

