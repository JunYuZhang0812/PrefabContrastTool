using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PrefabContrastTool.Prefab
{
    class ParseMetaDataInfo
    {
        Dictionary<string, MetaInfo> m_metaInfoDic;//fieldId = MetaInfo

        Regex regBeginKeyword = new Regex(@"serializedVersion:");
        Regex regName = new Regex(@"(?<=name: )(?:\w|\.)+");
        Regex regGuid = new Regex(@"(?<=guid: )\w+");
        Regex regFileId = new Regex(@"(?<=localIdentifier: )(?:\w|-)+");
        Regex regClassName = new Regex(@"(?<=scriptClassName: )(?:\w|\.)+");
        Regex regPath = new Regex(@"(?<=path: )(?:\w|/|\.)+");

        //Regex regInfo = new Regex(@"(?<name>")
        string[] m_moduleAttr;
        public Dictionary<string,MetaInfo> ParseFile( string filePath )
        {
            m_metaInfoDic = new Dictionary<string, MetaInfo>();
            LoadFile(filePath);
            ParseFile();
            return m_metaInfoDic;
        }
        private void LoadFile( string filePath )
        {
            string allStr = File.ReadAllText(filePath);
            m_moduleAttr = regBeginKeyword.Split(allStr);
        }
        private void ParseFile()
        {
            if (m_moduleAttr == null) return;
            for (int i = 0; i < m_moduleAttr.Length; i++)
            {
                string str = m_moduleAttr[i];
                MatchStr(str);
            }
        }
        private void MatchStr(string str)
        {
            if(regFileId.IsMatch(str))
            {
                var fileIdStr = regFileId.Match(str).Value;
                MetaInfo meta;
                if(!m_metaInfoDic.TryGetValue(fileIdStr ,out meta))
                {
                    meta = new MetaInfo();
                    meta.fileId = fileIdStr;
                    MatchName(str,ref meta);
                    MatchGuid(str, ref meta);
                    MatchClassName(str, ref meta);
                    MatchPath(str, ref meta);
                    m_metaInfoDic.Add(fileIdStr, meta);
                }
            }
        }
        private void MatchName( string str ,ref MetaInfo meta )
        {
            if (regName.IsMatch(str))
            {
                var matchStr = regName.Match(str).Value;
                meta.name = matchStr;
            }
        }
        private void MatchGuid(string str, ref MetaInfo meta)
        {
            if (regGuid.IsMatch(str))
            {
                var matchStr = regGuid.Match(str).Value;
                meta.guid = matchStr;
            }
        }
        private void MatchClassName(string str, ref MetaInfo meta)
        {
            if (regClassName.IsMatch(str))
            {
                var matchStr = regClassName.Match(str).Value;
                meta.scriptClassName =  matchStr;
            }
        }
        private void MatchPath(string str, ref MetaInfo meta)
        {
            if (regPath.IsMatch(str))
            {
                var matchStr = regPath.Match(str).Value;
                meta.path = matchStr;
            }
        }
    }
}
