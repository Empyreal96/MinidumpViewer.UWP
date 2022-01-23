using DbgHelp.MinidumpFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MinidumpViewer.UWP
{
    class Globals
    {
        public static StorageFile storageFile { get; set; }
        public static MiniDumpFile _miniDumpFile { get; set; }

        public static string minidumpHandleIDs { get; set; }
        public static string minidumpHandleObjects { get; set; }
        public static string minidumpHandleTypeName { get; set; }

        public static string minidumpCommentA { get; set; }
        public static string minidumpCommentW { get; set; }
    
        public static string exceptionStreamExceptionRecord   { get; set; }
        public static string exceptionStreamExceptionCode { get; set; }
        public static string exceptionStreamExceptionFlag   { get; set; }
        public static string exceptionStreamRecordRaw   { get; set; }
        public static string exceptionStreamExceptionAddress   { get; set; }
        public static string exceptionStreamNumberParameters   { get; set; }
        public static string exceptionStreamExceptionInfo   { get; set; }

        public static string miscData1 { get; set; }
        public static string miscData2 { get; set; }
        public static string miscData3 { get; set; }
        public static string miscData4 { get; set; }

        public static string unloadedModuleData { get; set; }

    }
}
