namespace GenMan45
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class ExporterCallback : ITypeLibExporterNotifySink
    {
        public void ReportEvent(ExporterEventKind EventKind, int EventCode, string EventMsg)
        {
            if (!global::GenMan45.GenMan32.s_Options.m_bSilentMode)
            {
                Console.WriteLine(EventMsg);
            }
        }

        public object ResolveRef(Assembly asm)
        {
            string directoryName = Path.GetDirectoryName(asm.Location);
            string name = asm.GetName().Name;
            ITypeLib embeddedTlb = TypeLibGenerator.GetEmbeddedTlb(Path.Combine(directoryName, name) + ".tlb.tmp");
            if (embeddedTlb == null)
            {
                throw new ApplicationException(Resource.FormatString("Err_NoEmbeddedTlbInRefAsm", name));
            }
            return embeddedTlb;
        }
    }
}

