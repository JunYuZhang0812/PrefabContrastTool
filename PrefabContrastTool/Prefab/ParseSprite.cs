using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PrefabContrastTool.Prefab
{
    class ParseSprite:Parse<Sprite>
    {
        private Sprite m_sprite = new Sprite();
        Regex regex = new Regex(@"    (\d+): (.+)");
        Regex regexStop = new Regex(@"serializedVersion: ");
        public override Sprite ParseFile(string path)
        {
            m_sprite.nameDic = new Dictionary<string, string>();
            LoadFile(path);
            m_sprite.name = Path.GetFileNameWithoutExtension(path);
            return m_sprite;
        }
        protected override bool ParseLineString(string line)
        {
            if (regexGuid.IsMatch(line))
            {
                m_sprite.guid = regexGuid.Match(line).Value;
            }
            if(regex.IsMatch(line))
            {
                var match = regex.Match(line);
                m_sprite.nameDic.Add(match.Groups[1].Value, match.Groups[2].Value);
            }
            if (regexStop.IsMatch(line))
                return true;
            return false;
        }
    }
}
