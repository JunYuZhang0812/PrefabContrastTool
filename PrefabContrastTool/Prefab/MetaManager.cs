using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PrefabContrastTool.Prefab
{
    public class MetaManager
    {
        public string ClientPath { get; set; }
        public string MetaDirPath { get; set; }
        private bool m_isSrc = false;
        public MetaManager( string clientPath , bool isSrc )
        {
            ClientPath = clientPath;
            MetaDirPath = clientPath + "\\Library\\metadata";
            m_isSrc = isSrc;
        }
        private Regex m_regMetaInfoTag = new Regex("serializedVersion");
        private Regex m_regFileId = new Regex(@"localIdentifier:\s(-?\d+)");
        private Regex m_regName = new Regex(@"name:\s(.+)");
        ParseMetaDataInfo m_metaParse = Singleton<ParseMetaDataInfo>.Instance;
        ParseSprite m_spriteParse = Singleton<ParseSprite>.Instance;
        ParseMaterial m_materialParse = Singleton<ParseMaterial>.Instance;
        //<guid,<fileId,infoStr>>
        private Dictionary<string, Dictionary<string, string>> m_dicMetaInfoStr = new Dictionary<string, Dictionary<string, string>>();
        //[guid][fileId]=MetaInfo
        Dictionary<string, Dictionary<string, MetaInfo>> m_metaInfoDic = new Dictionary<string, Dictionary<string, MetaInfo>>();

        Dictionary<string, Sprite> m_spriteMetaDic = new Dictionary<string, Sprite>();//guid,sprite
        Dictionary<string, Material> m_materialMetaDic = new Dictionary<string, Material>();//guid,material


        public void Clear()
        {
            m_spriteMetaDic.Clear();
            m_materialMetaDic.Clear();
        }
        public string GetMetaInfoName(string guid, string fileId)
        {
            var metaInfo = GetMetaInfoStr(guid, fileId);
            if (metaInfo != null && m_regName.IsMatch(metaInfo))
            {
                return m_regName.Match(metaInfo).Groups[1].Value;
            }
            return Logic.MissName;
        }

        public string GetMetaInfoStr(string guid , string fileId )
        {
            Dictionary<string, string> meta;
            if( !m_dicMetaInfoStr.TryGetValue(guid,out meta))
            {
                meta = new Dictionary<string, string>();
                var dir = guid.Substring(0, 2);
                var filePath = MetaDirPath + "\\" + dir + "\\" + guid + ".info";
                if( File.Exists(filePath))
                {
                    var allStr = File.ReadAllText(filePath);
                    var infoArr = m_regMetaInfoTag.Split(allStr);
                    for (int i = 0; i < infoArr.Length; i++)
                    {
                        var fileIdStr = m_regFileId.Match(infoArr[i]).Groups[1].Value;
                        if( !string.IsNullOrEmpty(fileIdStr) && !meta.ContainsKey(fileIdStr) )
                        {
                            meta.Add(fileIdStr, infoArr[i]);
                        }
                    }
                }
                m_dicMetaInfoStr.Add(guid, meta);
            }
            string info;
            if(meta.TryGetValue(fileId , out info))
            {
                return info;
            }
            return null;
        }

        public Dictionary<string, MetaInfo> GetMetaInfoDic(string guid)
        {
            Dictionary<string, MetaInfo> metaInfos;
            if (m_metaInfoDic.TryGetValue(guid, out metaInfos))
            {
                return metaInfos;
            }
            else
            {
                string parentFloder = guid.Substring(0, 2);
                string filePath = MetaDirPath + "\\" + parentFloder + "\\" + guid + ".info";
                if (File.Exists(filePath))
                {
                    metaInfos = m_metaParse.ParseFile(filePath);
                    m_metaInfoDic.Add(guid, metaInfos);
                    return metaInfos;
                }
            }
            return null;
        }
        public MetaInfo GetMetaInfo(string guid, string fileId)
        {
            Dictionary<string, MetaInfo> metaInfos;
            if (m_metaInfoDic.TryGetValue(guid, out metaInfos))
            {
                MetaInfo metaInfo;
                if (metaInfos.TryGetValue(fileId, out metaInfo))
                {
                    return metaInfo;
                }
            }
            else
            {
                string parentFloder = guid.Substring(0, 2);
                string filePath = MetaDirPath + "\\" + parentFloder + "\\" + guid + ".info";
                if (File.Exists(filePath))
                {
                    metaInfos = m_metaParse.ParseFile(filePath);
                    m_metaInfoDic.Add(guid, metaInfos);
                    MetaInfo metaInfo;
                    if (metaInfos.TryGetValue(fileId, out metaInfo))
                    {
                        return metaInfo;
                    }
                }
            }
            return null;
        }
        private string FormatStr(string str)
        {
            return string.Format(str, m_isSrc ? "源" : "目标");
        }
        private string GetImageFolderPath()
        {
            return ClientPath + "\\Assets\\Resources\\UI\\Atlas";
        }
        private string GetSpriteFolderPath()
        {
            return ClientPath + "\\Assets\\Resources\\UI\\Sprites";
        }
        public Dictionary<string, Sprite> GetSpriteMetaDic()
        {
            if (m_spriteMetaDic.Count <= 0)
            {
                BeginLoadSpriteMetas();
            }
            return m_spriteMetaDic;
        }
        private void BeginLoadSpriteMetas()
        {
            Logic.ChangeProgress(FormatStr("加载{0}图集文件中..."));
            m_spriteMetaDic.Clear();
            string folderPath = GetImageFolderPath();
            if (Directory.Exists(folderPath))
            {
                var allFiles = Directory.GetFiles(folderPath, "*.png.meta", SearchOption.AllDirectories);
                for (int i = 0; i < allFiles.Length; i++)
                {
                    var sprite = m_spriteParse.ParseFile(allFiles[i]);
                    sprite.isAtlas = true;
                    m_spriteMetaDic.Add(sprite.guid, sprite);
                }
            }
            Logic.ChangeProgress(FormatStr("加载{0}散图文件中..."));
            string folderPath2 = GetSpriteFolderPath();
            if (Directory.Exists(folderPath2))
            {
                var allFiles = Directory.GetFiles(folderPath2, "*.png.meta", SearchOption.AllDirectories);
                for (int i = 0; i < allFiles.Length; i++)
                {
                    var sprite = m_spriteParse.ParseFile(allFiles[i]);
                    sprite.isAtlas = false;
                    m_spriteMetaDic.Add(sprite.guid, sprite);
                }
            }
        }
        public Dictionary<string, Sprite> GetMaterialMetaDic()
        {
            if (m_spriteMetaDic.Count <= 0)
            {
                BeginLoadMaterialMetas();
            }
            return m_spriteMetaDic;
        }
        private void BeginLoadMaterialMetas()
        {
            Logic.ChangeProgress(FormatStr("加载{0}材质文件中..."));
            m_materialMetaDic.Clear();
            string folderPath = GetImageFolderPath();
            if (Directory.Exists(folderPath))
            {
                var allFiles = Directory.GetFiles(folderPath, "*.mat", SearchOption.AllDirectories);
                for (int i = 0; i < allFiles.Length; i++)
                {
                    var material = m_materialParse.ParseFile(allFiles[i]);
                    m_materialMetaDic.Add(material.guid, material);
                }
            }
        }

    }
}
