using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Forms;

namespace HeaderTreeExplorer
{
    public partial class FileAndFolderBrowserDialog : Form
    {
        public static class Helpers
        { 
            public const string defaultDir = @"C:\";

            public static bool IsDir(FileSystemInfo fs)
            {
                return Convert.ToBoolean(fs.Attributes & FileAttributes.Directory);
            }

            [DllImport("shell32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
            internal static extern bool SHGetPathFromIDListW(
                IntPtr pidl,
                [MarshalAs(UnmanagedType.LPTStr)]
                StringBuilder pszPath);

            private static string GetPathFromIDList(byte[] idList, int offset)
            {
                int nBytesToCopy = idList.Length - offset;

                int buffer = Math.Max(nBytesToCopy, 520);
                var sb = new StringBuilder(buffer);

                IntPtr ptr = Marshal.AllocHGlobal(idList.Length);

                Marshal.Copy(idList, offset, ptr, idList.Length - offset);

                bool result = SHGetPathFromIDListW(ptr, sb);
                Marshal.FreeHGlobal(ptr);

                return result ? sb.ToString() : string.Empty;
            }

            public static string GetDefaultDir()
            {
                bool installedOSAboveVista = Environment.OSVersion.Version.Major >= 6;
                string defaultFileBrowserDialogFolderRegKey = installedOSAboveVista ?
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU" :
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedMRU\";

                var regValues = Registry.CurrentUser.OpenSubKey(defaultFileBrowserDialogFolderRegKey, false);

                string currentAppName = System.AppDomain.CurrentDomain.FriendlyName;
                int wCharAppNameLength = currentAppName.Length * 2;

                foreach (string valueName in regValues.GetValueNames())
                {
                    byte[] data = (byte[])regValues.GetValue(valueName);
                    if (data == null) continue;

                    string entryAppName = Encoding.Unicode.GetString(data, 0, wCharAppNameLength);

                    if (entryAppName == currentAppName)
                    {
                        //Matching registry entry
                        int offset = (entryAppName.Length + 1) * 2; //wchars + null terminator
                        string lastAccessedFile = GetPathFromIDList(data, offset);

                        return string.IsNullOrEmpty(lastAccessedFile) ? defaultDir : lastAccessedFile;
                    }
                }

                //No matching registry entry
                return defaultDir;
            }
        }
    }
}
