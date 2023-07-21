using System.Text.RegularExpressions;

namespace PrefabContrastTool.Prefab
{
    class ParseFont : Parse<Font>
    {
        private Font m_font = new Font();
        private Regex regexFontName = new Regex(@"(?<=fontName: ).+");
        public override Font ParseFile(string path)
        {
            LoadFile(path);
            return m_font;
        }
        protected override bool ParseLineString(string line )
        {
            if(regexGuid.IsMatch(line))
            {
                m_font.guid = regexGuid.Match(line).Value;
                return false;
            }
            if(regexFontName.IsMatch(line))
            {
                m_font.name = regexFontName.Match(line).Value;
                return true;
            }
            return false;
        }
    }
}
