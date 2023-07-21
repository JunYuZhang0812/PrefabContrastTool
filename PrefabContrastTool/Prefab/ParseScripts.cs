using System;
using System.IO;

namespace PrefabContrastTool.Prefab
{
    class ParseScripts:Parse<Script>
    {
        private Script m_scripts = new Script();
        public override Script ParseFile(string path)
        {
            LoadFile(path);
            m_scripts.name = Path.GetFileNameWithoutExtension(path);
            return m_scripts;
        }
        protected override bool ParseLineString(string line)
        {
            if (regexGuid.IsMatch(line))
            {
                m_scripts.guid = regexGuid.Match(line).Value;
                return true;
            }
            return false;
        }
    }
}
