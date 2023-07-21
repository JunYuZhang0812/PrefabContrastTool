using System.IO;
using System.Text.RegularExpressions;

namespace PrefabContrastTool.Prefab
{
    class ParseMaterial : Parse<Material>
    {

        private Material m_material = new Material();
        Regex regexFileId = new Regex(@"(?<=--- !u!21 &)\d+");
        public override Material ParseFile(string path)
        {
            LoadFile(path);
            LoadFile(path+".meta");
            m_material.name = Path.GetFileNameWithoutExtension(path);
            return m_material;
        }
        protected override bool ParseLineString(string line)
        {
            if (regexGuid.IsMatch(line))
            {
                m_material.guid = regexGuid.Match(line).Value;
                return true;
            }
            if(regexFileId.IsMatch(line))
            {
                m_material.fileId = regexFileId.Match(line).Value;
                return true;
            }
            return false;
        }
    }
}
