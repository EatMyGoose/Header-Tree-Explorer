using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.IO;

using System.Windows.Forms;

namespace HeaderTreeExplorer.Parser
{
    //Todo -> Load lib/additional include directories as well
    public class VcxprojInfo
    {
        public readonly string projFileDir; //The directory the .vcxproj is in
        public readonly string[] headerFileNames; //All header files specified in the project file (rel. Path)
        public readonly string[] cppFileNames; //Implementation files specified in the project file (rel. Path)

        public VcxprojInfo(string _projFileDir, string[] _headerFileNames, string[] _cppFileNames)
        {
            projFileDir = _projFileDir;
            headerFileNames = _headerFileNames;
            cppFileNames = _cppFileNames;
        }
    }

    public static class VcxprojParser
    {
        public static VcxprojInfo ParseProjectFile(string fullPath)
        {
            //Sanity checks
            if (Path.GetExtension(fullPath).ToLower() != ".vcxproj" ||
                !File.Exists(fullPath))
            {
                return null;
            }

            string projectXml = null;
            try
            {
                projectXml = File.ReadAllText(fullPath);
            }
            catch(Exception e)
            {
                MessageBox.Show($"Unable to open file:\"{fullPath}\"\nException details:\n{e.ToString()}");
                return null;
            }

            var dom = new XmlDocument();
            try
            {
                dom.LoadXml(projectXml);
            }
            catch(XmlException e)
            {
                MessageBox.Show($"Unable parse project file xml:\nException details:\n{e.ToString()}");
                return null;
            }

            
            XmlNode root = dom.DocumentElement;

            string defaultNamespace = root.NamespaceURI;
            var nsManager = new XmlNamespaceManager(dom.NameTable);
            nsManager.AddNamespace("def", defaultNamespace);

            const string headerXPath = @"//def:ItemGroup/def:ClInclude[@Include]";
            const string cppXPath = @"//def:ItemGroup/def:ClCompile[@Include]";

            

            string[] headerFiles =
                root
                .SelectNodes(headerXPath, nsManager)
                .Cast<XmlNode>()
                .Select(node => node.Attributes["Include"].Value) //Guaranteed to exist due to the xpath expression
                .ToArray();

            string[] cppFiles =
                root
                .SelectNodes(cppXPath, nsManager)
                .Cast<XmlNode>()
                .Select(node => node.Attributes["Include"].Value)
                .ToArray();

            string projDir = Path.GetDirectoryName(fullPath);
            return new VcxprojInfo(projDir, headerFiles, cppFiles);
        }
    }
}
