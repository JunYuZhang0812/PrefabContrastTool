using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PrefabContrastTool.Prefab
{
    class DeletePrefabNode
    {
        class PrefabTree
        {
            public string name;
            public string gameObjectId;
            public string path;
            public string parentGoId;
            public List<PrefabTree> child;
        }
        struct GameObject
        {
            public string gameObjectId;
            public List<string> componentIds;
            public string name;
        }
        struct Component
        {
            public ComponentType type;
            public string gameObjectId;
            public string uid;
            public string content;
            public int srcIndex;
            public int targetIndex;
            public int order;
            public List<string> child;//Rect专用，子rectId列表
        }
        enum FindTreeType
        {
            gameObjectId,
            path,
        }
        string m_regex = "--- !u!";
        Regex m_regexSpace = new Regex(@"^\s*[^.]?$");
        Regex m_regexRootId = new Regex(@"(?<=m_RootGameObject:[^(\d-)]*)(?:-|\d)+");
        Regex m_regexComId = new Regex(@"(?<=((?:-|\d)+)\s*&)(?:-|\d)+");
        Regex m_regexGameObjectId = new Regex(@"(?<=m_GameObject:[^(\d-)]*)(?:-|\d)+");
        Regex m_regexName = new Regex(@"(?<=m_Name: ).*");
        Regex m_getComponentId = new Regex(@"(?<=- component: {fileID: )(?:-|\d)+(?=})");
        Regex m_regexNoChild = new Regex(@"m_Children: \[\]");
        Regex m_regexChilds = new Regex(@"m_Children:\n\s*(?:\s*- {fileID: (?:-|\d)+}\n)+", RegexOptions.Multiline);
        Regex m_regexChildId = new Regex(@"(?<=- {fileID: )(?:-|\d)+");
        Regex m_regexRootOrder = new Regex(@"(?<=m_RootOrder: )(?:-|\d)+");
        Regex m_regexSprite = new Regex(@"m_Sprite: {fileID: ((?:-|\d)+)(?:, guid: )?([^,]*)");
        Regex m_regexMaterial = new Regex(@"m_Material: {fileID: ((?:-|\d)+)(?:, guid: )?([^,]*)");
        Dictionary<string, GameObject> m_allNodeDic = new Dictionary<string, GameObject>();//goId,GameObjectNode
        Dictionary<string, Component> m_comIdDic = new Dictionary<string, Component>();//comId,comStr
        string m_goId;
        string m_rootId;
        string m_filePath = string.Empty;
        string[] m_fileArr = null;
        List<string> m_fileList;
        PrefabTree m_nodeTree;
        public void ClearCache()
        {
            m_filePath = string.Empty;
        }
        public bool DeletePrefabData(string gameObjectId,string filePath )
        {
            if (gameObjectId == string.Empty || gameObjectId == "0" )
            {
                MessageBox.Show("m_gameObjectId错误："+ gameObjectId);
                return false;
            }
            m_filePath = filePath;
            m_goId = gameObjectId;
            LoadUnityPrefab(m_filePath);
            if(m_fileArr != null)
            {
                FindComponentID();
                SetPrefabTree();
                var data = GetNodeData(gameObjectId);
                if (data == null) return false;
                BeginDeleteNode(data);
            }
            WritePrefab();
            return true;
        }
        private void LoadUnityPrefab(string filePath)
        {
            Logic.ChangeProgress("开始加载预制...");
            string allStr = File.ReadAllText(filePath);
            m_fileArr = Regex.Split(allStr, m_regex, RegexOptions.Multiline);
            m_fileList = m_fileArr.ToList();
        }
        private void FindComponentID()
        {
            m_allNodeDic.Clear();
            m_comIdDic.Clear();
            for (int i = 0; i < m_fileArr.Length; i++)
            {
                var str = m_fileArr[i];
                if (m_regexRootId.IsMatch(str))
                {
                    m_rootId = m_regexRootId.Match(str).Value;
                }
                if (m_regexComId.IsMatch(str))
                {
                    var match = m_regexComId.Match(str);
                    string typeId = match.Groups[1].Value;
                    string comId = match.Value;
                    Component com = new Component();
                    com.uid = comId;
                    com.type = GetComponentTypeById(typeId);
                    com.content = str;
                    com.srcIndex = i;
                    if (m_regexRootOrder.IsMatch(str))
                    {
                        com.order = int.Parse(m_regexRootOrder.Match(str).Value);
                    }
                    if (m_regexGameObjectId.IsMatch(str))
                    {
                        com.gameObjectId = m_regexGameObjectId.Match(str).Value;
                    }
                    if (com.type == ComponentType.GameObject)
                    {
                        GameObject go = new GameObject();
                        go.gameObjectId = comId;
                        go.componentIds = new List<string>();
                        var matchList = m_getComponentId.Matches(str);
                        for (int k = 0; k < matchList.Count; k++)
                        {
                            go.componentIds.Add(matchList[k].Value);
                        }
                        go.name = m_regexName.Match(str).Value;
                        m_allNodeDic.Add(comId, go);
                    }
                    else if (com.type == ComponentType.RectTransform)
                    {
                        com.child = new List<string>();
                        if (m_regexChildId.IsMatch(str))
                        {
                            var matches = m_regexChildId.Matches(str);
                            for (int k = 0; k < matches.Count; k++)
                            {
                                com.child.Add(matches[k].Value);
                            }
                        }
                    }
                    m_comIdDic.Add(comId, com);
                }
            }
        }
        
        private void SetPrefabTree()
        {
            m_nodeTree = new PrefabTree();
            SetTreeNode(m_rootId, ref m_nodeTree);
        }
        private void SetTreeNode(string id, ref PrefabTree node)
        {
            GameObject go;
            if (m_allNodeDic.TryGetValue(id, out go))
            {
                node.name = go.name;
                node.gameObjectId = id;
                if (string.IsNullOrEmpty(node.path))
                    node.path += go.name;
                else
                    node.path += "/" + go.name;
                node.child = new List<PrefabTree>();
                for (int i = 0; i < go.componentIds.Count; i++)
                {
                    Component com = m_comIdDic[go.componentIds[i]];
                    if (com.type == ComponentType.RectTransform)
                    {
                        if (com.child.Count > 0)
                        {
                            for (int k = 0; k < com.child.Count; k++)
                            {
                                PrefabTree child = new PrefabTree();
                                var childCom = m_comIdDic[com.child[k]];
                                child.path = node.path;
                                SetTreeNode(childCom.gameObjectId, ref child);
                                child.parentGoId = node.gameObjectId;
                                node.child.Add(child);
                            }
                        }
                        break;
                    }
                }
            }
        }
        private PrefabTree GetNodeData(string key, FindTreeType type = FindTreeType.gameObjectId)
        {
            return FindTree(key, m_nodeTree, type);
        }
        private PrefabTree FindTree( string key, PrefabTree node , FindTreeType type)
        {
            if(type == FindTreeType.gameObjectId)
            {
                if (node.gameObjectId == key)
                    return node;
            }
            else if( type == FindTreeType.path)
            {
                if (node.path == key)
                    return node;
            }
            if(node.child.Count > 0 )
            {
                for (int i = 0; i < node.child.Count; i++)
                {
                    PrefabTree child = FindTree(key, node.child[i], type);
                    if (child != null)
                        return child;
                }
            }
            return null;
        }
        private ComponentType GetComponentTypeById(string typeId)
        {
            if (typeId == "1")
                return ComponentType.GameObject;
            else if (typeId == "114")
                return ComponentType.MonoBehaviour;
            else if (typeId == "224")
                return ComponentType.RectTransform;
            else if (typeId == "222")
                return ComponentType.CanvasRenderer;
            return ComponentType.Other;
        }
        private void BeginDeleteNode(PrefabTree node)
        {
            string rectComId = null;
            var go = m_allNodeDic[m_goId];
            for (int i = 0; i < go.componentIds.Count; i++)
            {
                var com = m_comIdDic[go.componentIds[i]];
                if (com.type == ComponentType.RectTransform)
                {
                    rectComId = go.componentIds[i];
                    break;
                }
            }
            DeleteNode(m_goId);
            if (rectComId != null)
            {
                var regFileID = new Regex(@"- {fileID: " + rectComId + "}");
                for (int i = 0; i < m_fileList.Count; i++)
                {
                    if(regFileID.IsMatch(m_fileList[i]))
                    {
                        m_fileList[i] = regFileID.Replace(m_fileList[i], "");
                        break;
                    }
                }
            }
        }
        private void DeleteNode(string goId)
        {
            var go = m_allNodeDic[goId];
            for (int i = 0; i < go.componentIds.Count; i++)
            {
                var com = m_comIdDic[go.componentIds[i]];
                m_fileList[com.srcIndex] = "";
                if( com.type == ComponentType.RectTransform )
                {
                    for (int k = 0; k < com.child.Count; k++)
                    {
                        var _com = m_comIdDic[com.child[k]];
                        DeleteNode(_com.gameObjectId);
                    }
                }
            }
            var gocom = m_comIdDic[goId];
            m_fileList[gocom.srcIndex] = "";
        }
        private void WritePrefab()
        {
            if(m_fileList == null || m_fileList.Count <= 0)
            {
                MessageBox.Show("目标文件为空!");
                return;
            }
            Logic.ChangeProgress("开始写入预制...");
            FileInfo file = new FileInfo(m_filePath);
            if (!File.Exists(m_filePath))
            {
                FileStream fs = file.Create();
                fs.Close();
            }
            StreamWriter sw = file.CreateText();
            try
            {
                for (int i = 0; i < m_fileList.Count; i++)
                {
                    if(!m_regexSpace.IsMatch(m_fileList[i]) )
                    {
                        if (i == 0)
                            sw.WriteLine(m_fileList[i]);
                        else
                            sw.WriteLine(m_regex + m_fileList[i]);
                    }
                }
            }
            finally
            {
                sw.Close();
            }
            #region 去除空行
            List<string> lineList = new List<string>();
            StreamReader sr = file.OpenText();
            try
            {
                string strReadLine;
                while ((strReadLine = sr.ReadLine()) != null)
                {
                    if (!m_regexSpace.IsMatch(strReadLine))
                    {
                        strReadLine = strReadLine.Replace("\r\n", "\n");
                        lineList.Add(strReadLine);
                    }
                }
            }
            finally
            {
                sr.Close();
            }
            sw = file.CreateText();
            try
            {
                for (int i = 0; i < lineList.Count; i++)
                {
                    sw.NewLine = "\n";
                     sw.WriteLine(lineList[i]);
                }
            }
            finally
            {
                sw.Close();
            }
            #endregion
        }
    }
}
