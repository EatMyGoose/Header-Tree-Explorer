using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Text.RegularExpressions;

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

            public static Tuple<bool, FileSystemInfo[]> TryListDirectory(string dirPath)
            {
                if (!Directory.Exists(dirPath))
                {
                    return Tuple.Create(false, new FileSystemInfo[] { });
                }

                var dirInfo = new DirectoryInfo(dirPath);
                FileSystemInfo[] fileInfo = dirInfo.GetFileSystemInfos();

                return Tuple.Create(true, fileInfo);
            }

            public static FileSystemInfo[] ApplyFileExtensionFilters(FileSystemInfo[] rawSelection, string[] selectedExtensions)
            {
                return rawSelection
                    .Where(fs =>
                    {
                        if (Helpers.IsDir(fs)) return true; //Only filter files

                    string fileExtension = Path.GetExtension(fs.Name).ToLower().Trim();

                        return selectedExtensions.Any(filter => filter == ".*" || filter == fileExtension);
                    })
                    .ToArray();
            }

            public static FileSystemInfo[] ApplyRegexFilters(FileSystemInfo[] rawSelection, Regex regPattern, bool applyToFolders)
            {
                return rawSelection
                    .Where(fs =>
                    {
                        if (regPattern == null) return true;

                        if (Helpers.IsDir(fs) && !applyToFolders) return true;

                        return regPattern.IsMatch(fs.Name);
                    })
                    .ToArray();
            }

            //Converts the windows file extension format filters (i.e. "c++ files(*.cpp,*.h)|*.cpp;*.h"
            //into a tuple containing the description and array of listed filename extensions 
            //e.g "c++ files(*.cpp,*.h)|*.cpp;*.h" => ("c++ files(*.cpp,*.h)", [".cpp", ".h"])
            public static Tuple<string, string[]>[] GetExtensionFilters(string format)
            {
                string[] tokenList = format.Split('|');

                //(displayedTitle:string, extensions(without the dot):string[])
                var extensionFilters = new List<Tuple<string, string[]>>();

                //Must process in pairs, only iterate up to the highest set of pairs
                int maxPairLength = (tokenList.Length % 2 == 0) ? tokenList.Length : tokenList.Length - 1;
                for (int index = 0; index < maxPairLength; index += 2)
                {
                    string description = tokenList[index];
                    string extensions = tokenList[index + 1];

                    string[] extensionList = extensions
                        .Split(';') //"*.*;*.csv;" => [*.*, *.csv]
                        .Select(str => str.Split('.'))
                        .Where(strPair => strPair.Length == 2)
                        .Select(strPair => "." + strPair[1].ToLower().Trim())
                        .ToArray();

                    extensionFilters.Add(new Tuple<string, string[]>(description, extensionList));
                }
                return extensionFilters.ToArray();
            }
        }
    }
}
