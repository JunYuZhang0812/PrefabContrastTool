using PrefabContrastTool.Prefab;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PrefabContrastTool
{
    class ClonePrefabNode
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
        string m_srcFilePath = string.Empty;
        string m_tarFilePath = string.Empty;
        bool m_canCopy = true;
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
        string m_copySrcId;
        string m_tarRootId;
        string m_srcRootId;
        PrefabTree m_tarNodeTree = new PrefabTree();
        PrefabTree m_srcNodeTree = new PrefabTree();
        Dictionary<string, GameObject> m_srcAllNodeDic = new Dictionary<string, GameObject>();//goId,GameObjectNode
        Dictionary<string, Component> m_srcComIdDic = new Dictionary<string, Component>();//comId,comStr
        Dictionary<string, string> m_srcGoIdToPath = new Dictionary<string, string>();//goId,path
        Dictionary<string, GameObject> m_tarAllNodeDic = new Dictionary<string, GameObject>();//goId,GameObjectNode
        Dictionary<string, Component> m_tarComIdDic = new Dictionary<string, Component>();//comId,comStr
        Dictionary<string, string> m_tarGoIdToPath = new Dictionary<string, string>();//goId,path
        string[] m_srcFileArr = null;
        string[] m_tarFileArr = null;
        List<string> m_tarFileList;
        bool m_isSrcToTarget = false;
        public void ClearCache()
        {
            m_srcFilePath = string.Empty;
            m_tarFilePath = string.Empty;
        }
        public bool ClonePrefabData(string gameObjectId,string srcFilePath , string targetFilePath , bool isSrcToTarget )
        {
            m_canCopy = true;
            if (gameObjectId == string.Empty || gameObjectId == "0" )
            {
                MessageBox.Show("m_gameObjectId错误："+ gameObjectId);
                return m_canCopy;
            }
            m_isSrcToTarget = isSrcToTarget;
            m_srcFilePath = srcFilePath;
            m_tarFilePath = targetFilePath;
            m_copySrcId = gameObjectId;
            LoadUnityPrefab(srcFilePath,targetFilePath);
            if(m_tarFileArr != null)
            {
                FindSrcComponentID();
                FindTarComponentID();
                SetPrefabTree();
                var data = GetNodeData(gameObjectId, false);
                if (data == null) return m_canCopy;
                if(CheckRepeat(data.path))
                {
                    //MessageBox.Show("该节点已存在，无法拷贝");
                    //return m_canCopy;
                    //先删除
                    Singleton<DeletePrefabNode>.Instance.DeletePrefabData(gameObjectId, targetFilePath);
                    return ClonePrefabData(m_copySrcId , m_srcFilePath, m_tarFilePath, m_isSrcToTarget);
                }
                BeginCopyFile();
            }
            else
            {
                m_tarFileArr = m_srcFileArr;
                m_tarFileList = m_tarFileArr.ToList();
                ReplaceAllImage();
                ReplaceAllMaterial();
            }
            WritePrefab(targetFilePath);
            return m_canCopy;
        }
        private void LoadUnityPrefab(string srcFilePath, string targetFilePath)
        {
            Logic.ChangeProgress("开始加载预制...");
            string allStr = File.ReadAllText(srcFilePath);
            m_srcFileArr = Regex.Split(allStr, m_regex, RegexOptions.Multiline);
            if (targetFilePath != string.Empty && File.Exists(targetFilePath))
            {
                string allStr2 = File.ReadAllText(targetFilePath);
                m_tarFileArr = Regex.Split(allStr2, m_regex, RegexOptions.Multiline);
                m_tarFileList = m_tarFileArr.ToList();
            }
            else
            {
                m_tarFileArr = null;
                m_tarFileList = null;
            }
        }
        private void FindSrcComponentID()
        {
            m_srcAllNodeDic.Clear();
            m_srcComIdDic.Clear();
            for (int i = 0; i < m_srcFileArr.Length; i++)
            {
                var str = m_srcFileArr[i];
                if (m_regexRootId.IsMatch(str))
                {
                    m_srcRootId = m_regexRootId.Match(str).Value;
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
                        m_srcAllNodeDic.Add(comId, go);
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
                    m_srcComIdDic.Add(comId, com);
                }
            }
        }
        private void FindTarComponentID()
        {
            m_tarAllNodeDic.Clear();
            m_tarComIdDic.Clear();
            for (int i = 0; i < m_tarFileList.Count; i++)
            {
                var str = m_tarFileList[i];
                if(m_regexRootId.IsMatch(str))
                {
                    m_tarRootId = m_regexRootId.Match(str).Value;
                }
                if(m_regexComId.IsMatch(str) )
                {
                    var match = m_regexComId.Match(str);
                    string typeId = match.Groups[1].Value;
                    string comId = match.Value;
                    Component com = new Component();
                    com.uid = comId;
                    com.type = GetComponentTypeById(typeId);
                    com.content = str;
                    com.targetIndex = i;
                    if( m_regexRootOrder.IsMatch(str))
                    {
                        com.order = int.Parse(m_regexRootOrder.Match(str).Value);
                    }
                    if (m_regexGameObjectId.IsMatch(str) )
                    {
                        com.gameObjectId = m_regexGameObjectId.Match(str).Value;
                    }
                    if( com.type == ComponentType.GameObject )
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
                        m_tarAllNodeDic.Add(comId,go);
                    }
                    else if(com.type == ComponentType.RectTransform)
                    {
                        com.child = new List<string>();
                        if (m_regexChildId.IsMatch(str) )
                        {
                            var matches = m_regexChildId.Matches(str);
                            for (int k = 0; k < matches.Count; k++)
                            {
                                com.child.Add(matches[k].Value);
                            }
                        }
                    }
                    m_tarComIdDic.Add(comId, com);
                }
            }
        }
        private void SetPrefabTree()
        {
            m_tarGoIdToPath.Clear();
            m_tarNodeTree = new PrefabTree();
            SetTarTreeNode(m_tarRootId, ref m_tarNodeTree);
            m_srcGoIdToPath.Clear();
            m_srcNodeTree = new PrefabTree();
            SetSrcTreeNode(m_srcRootId, ref m_srcNodeTree);
        }
        private void SetSrcTreeNode(string id, ref PrefabTree node)
        {
            GameObject go;
            if (m_srcAllNodeDic.TryGetValue(id, out go))
            {
                node.name = go.name;
                node.gameObjectId = id;
                if (string.IsNullOrEmpty(node.path))
                    node.path += go.name;
                else
                    node.path += "/" + go.name;
                m_srcGoIdToPath.Add(id, node.path);
                node.child = new List<PrefabTree>();
                for (int i = 0; i < go.componentIds.Count; i++)
                {
                    Component com = m_srcComIdDic[go.componentIds[i]];
                    if (com.type == ComponentType.RectTransform)
                    {
                        if (com.child.Count > 0)
                        {
                            for (int k = 0; k < com.child.Count; k++)
                            {
                                PrefabTree child = new PrefabTree();
                                var childCom = m_srcComIdDic[com.child[k]];
                                child.path = node.path;
                                SetSrcTreeNode(childCom.gameObjectId, ref child);
                                child.parentGoId = node.gameObjectId;
                                node.child.Add(child);
                            }
                        }
                        break;
                    }
                }
            }
        }
        private void SetTarTreeNode( string id ,ref PrefabTree node )
        {
            GameObject go = m_tarAllNodeDic[id];
            node.name = go.name;
            node.gameObjectId = id;
            if (string.IsNullOrEmpty(node.path))
                node.path += go.name;
            else
                node.path += "/" + go.name;
            m_tarGoIdToPath.Add(id, node.path);
            node.child = new List<PrefabTree>();
            for (int i = 0; i < go.componentIds.Count; i++)
            {
                Component com = m_tarComIdDic[go.componentIds[i]];
                if(com.type == ComponentType.RectTransform)
                {
                    if(com.child.Count > 0)
                    {
                        for (int k = 0; k < com.child.Count; k++)
                        {
                            PrefabTree child = new PrefabTree();
                            var childCom = m_tarComIdDic[com.child[k]];
                            child.path = node.path;
                            SetTarTreeNode(childCom.gameObjectId, ref child);
                            child.parentGoId = node.gameObjectId;
                            node.child.Add(child);
                        }
                    }
                    break;
                }
            }
        }
        private PrefabTree GetNodeData(string key, bool isTarget ,FindTreeType type = FindTreeType.gameObjectId)
        {
            if( isTarget)
                return FindTree(key, m_tarNodeTree, type);
            return FindTree(key, m_srcNodeTree, type);
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
        private string GetNewComponentId( string comId )
        {
            if(!m_tarComIdDic.ContainsKey(comId))
            {
                return comId;
            }
            return GetNewComponentId(comId + 1);
        }
        private bool CheckRepeat( string nodePath )
        {
            List<int> removeList = new List<int>();
            var data = GetNodeData(nodePath,true, FindTreeType.path);
            if (data == null) return false;
            return true;
        }
        private void BeginCopyFile()
        {
            string beginNodePath;
            var beginParentNodePath = FindTarParentPathNeedBeginCopy(out beginNodePath);
            if (beginParentNodePath == "")
            {
                m_canCopy = false;
                return;
            }
            CopyNodeStr(beginParentNodePath, beginNodePath);
        }
        //找到目标预制需要可以开始拷贝的父节点id
        private string FindTarParentPathNeedBeginCopy(out string beginPath )
        {
            beginPath = "";
            var srcData = GetNodeData(m_copySrcId, false);
            if (srcData == null)
                return "";
            var parentId = srcData.parentGoId;
            var parentPath = m_srcGoIdToPath[parentId];
            beginPath = srcData.path;
            var tarData = GetNodeData(parentPath, true, FindTreeType.path);
            while (tarData == null)
            {
                srcData = GetNodeData(parentId, false);
                if (srcData == null)
                {
                    beginPath = "";
                    return "";
                }
                beginPath = srcData.path;
                parentId = srcData.parentGoId;
                parentPath = m_srcGoIdToPath[parentId];
                tarData = GetNodeData(parentPath, true, FindTreeType.path);
            }
            return tarData.path;
        }
        private void CopyNodeStr( string beginParentNodePath , string beginNodePath)
        {
            PrefabTree srcParentNode = GetNodeData(beginParentNodePath, false,FindTreeType.path);
            PrefabTree srcNode = null;
            for (int i = 0; i < srcParentNode.child.Count; i++)
            {
                if(beginNodePath == srcParentNode.child[i].path)
                {
                    srcNode = srcParentNode.child[i];
                    break;
                }
            }
            if(srcNode==null)
            {
                m_canCopy = false;
                return;
            }
            PrefabTree tarParentNode = GetNodeData(beginParentNodePath, true, FindTreeType.path);
            //设置子节点RectTransform的Father
            var srcGoId = srcNode.gameObjectId;
            var srcGo = m_srcAllNodeDic[srcGoId];
            var srcGoCom = m_srcComIdDic[srcGoId];
            //父节点的GoId
            var srcParentGoId = srcParentNode.gameObjectId;
            var srcParentGo = m_srcAllNodeDic[srcParentGoId];
            string srcParentRectId = "0";
            for (int i = 0; i < srcParentGo.componentIds.Count; i++)
            {
                var srcCom = m_srcComIdDic[srcParentGo.componentIds[i]];
                if (srcCom.type == ComponentType.RectTransform)
                {
                    srcParentRectId = srcCom.uid;
                    break;
                }
            }
            var tarParentGoId = tarParentNode.gameObjectId;
            var tarParentGo = m_tarAllNodeDic[tarParentGoId];
            string tarParentRectId = "0";
            int tarComIndex = 0;
            for (int i = 0; i < tarParentGo.componentIds.Count; i++)
            {
                var tarCom = m_tarComIdDic[tarParentGo.componentIds[i]];
                if(tarCom.type == ComponentType.RectTransform)
                {
                    tarParentRectId = tarCom.uid;
                    tarComIndex = tarCom.targetIndex;
                    break;
                }
            }
            //获得目标节点的GoId
            var tarGoId = GetNewComponentId(srcGoId);
            var tarGoStr = srcGoCom.content;
            if( tarGoId != srcGoId)
            {
                tarGoStr = tarGoStr.Replace(srcGoId, tarGoId);
            }
            int myRootOrder = 999999;
            for (int i = 0; i < srcGo.componentIds.Count; i++)
            {
                var srcComId = srcGo.componentIds[i];
                var tarComId = GetNewComponentId(srcComId);
                var srcCom = m_srcComIdDic[srcComId];
                var tarComStr = srcCom.content;
                var myOrder = srcCom.order;
                if (tarComId != srcComId)
                {
                    tarComStr = tarComStr.Replace(srcComId, tarComId);
                    tarGoStr = tarGoStr.Replace(srcComId, tarComId);
                }
                if (tarGoId != srcGoId)
                {
                    tarComStr = tarComStr.Replace(srcGoId, tarGoId);
                }
                //设置父节点
                if(srcCom.type == ComponentType.RectTransform)
                {
                    myRootOrder = int.Parse( m_regexRootOrder.Match(tarComStr).Value);
                    //去掉自己身上的子节点
                    if(m_regexChilds.IsMatch(tarComStr))
                    {
                        string str = "m_Children: []\n";
                        tarComStr = m_regexChilds.Replace(tarComStr, str);
                    }
                    if (tarParentRectId != srcParentRectId)
                    {
                        tarComStr = tarComStr.Replace(srcParentRectId, tarParentRectId);
                    }
                    //设置父节点RectTransform的Children
                    var tarParentRectStr = m_tarFileList[tarComIndex];
                    //父节点上无子物体
                    if (m_regexNoChild.IsMatch(tarParentRectStr))
                    {
                        string newStr = "m_Children:\n  - {fileID: " + tarComId + "}";
                        tarParentRectStr = m_regexNoChild.Replace(tarParentRectStr, newStr);
                    }
                    else//原父节点上有子节点
                    {
                        var str = m_regexChilds.Match(tarParentRectStr).Value;
                        str += "  - {fileID: " + tarComId + "}\n";
                        //根据order调整顺序
                        //找到同级节点，改变RootOrder
                        for (int j = 0; j < tarParentNode.child.Count; j++)
                        {
                            var go = m_tarAllNodeDic[tarParentNode.child[j].gameObjectId];
                            for (int k = 0; k < go.componentIds.Count; k++)
                            {
                                var com = m_tarComIdDic[go.componentIds[k]];
                                if (com.type == ComponentType.RectTransform)
                                {
                                    if (com.order >= myRootOrder)
                                    {
                                        com.order++;
                                        m_tarFileList[com.targetIndex] = m_regexRootOrder.Replace(m_tarFileList[com.targetIndex], com.order.ToString());
                                        m_tarComIdDic[go.componentIds[k]] = com;
                                    }
                                    break;
                                }
                            }
                        }
                        var strList = Regex.Split(str, "- {fileID:");
                        Regex regexId = new Regex(@"\d+");
                        List<string> childList = new List<string>();
                        for (int j = 0; j < strList.Length; j++)
                        {
                            if(regexId.IsMatch(strList[j]))
                            {
                                childList.Add(regexId.Match(strList[j]).Value);
                            }
                        }
                        childList.Sort(delegate(string childRectId1, string childRectId2)
                        {
                            int order1, order2;
                            if (m_tarComIdDic.TryGetValue(childRectId1, out Component com1))
                                order1 = com1.order;
                            else
                                order1 = myRootOrder;
                            if (m_tarComIdDic.TryGetValue(childRectId2, out Component com2))
                                order2 = com2.order;
                            else
                                order2 = myRootOrder;
                            if (order1 == order2)
                                return 0;
                            if (order1 < order2)
                                return -1;
                            return 1;
                        });
                        StringBuilder newStr = new StringBuilder("m_Children:\n");
                        for (int j = 0; j < childList.Count; j++)
                        {
                            newStr.Append("  - {fileID: ");
                            newStr.Append(childList[j].ToString());
                            newStr.Append("}\n");
                        }
                        tarParentRectStr = m_regexChilds.Replace(tarParentRectStr, newStr.ToString() );
                    }
                    m_tarFileList[tarComIndex] = tarParentRectStr;
                }
                if(srcCom.type == ComponentType.MonoBehaviour )
                {
                    //设置图集
                    tarComStr = ReplaceImage(tarComStr);
                    //设置材质
                    tarComStr = ReplaceMaterial(tarComStr);
                }
                m_tarFileList.Add(tarComStr);
            }
            m_tarFileList.Insert(m_tarFileList.Count- srcGo.componentIds.Count ,tarGoStr);
            FindTarComponentID();
            SetPrefabTree();
            //检索子节点
            if (srcNode.child.Count > 0)
            {
                for (int i = 0; i < srcNode.child.Count; i++)
                {
                    var newBeginParentNodePath = srcNode.path;
                    var newBeginNodePath = srcNode.child[i].path;
                    CopyNodeStr(newBeginParentNodePath, newBeginNodePath);
                }
            }
        }
        private string ReplaceImage(string secStr)
        {
            if (m_regexSprite.IsMatch(secStr))
            {
                var match = m_regexSprite.Match(secStr);
                string fileId = match.Groups[1].Value;
                if (fileId != "0")
                {
                    string srcGuid = match.Groups[2].Value;
                    string tarGuid;
                    string tarFileId;
                    if (m_isSrcToTarget)
                        tarFileId = GetTargetSpriteFileId(srcGuid, fileId, out tarGuid);
                    else
                        tarFileId = GetSrcSpriteFileId(srcGuid, fileId, out tarGuid);
                    if (tarFileId != null)
                    {
                        if (srcGuid != tarGuid)
                            secStr = secStr.Replace(srcGuid, tarGuid);
                        if (fileId != tarFileId)
                            secStr = secStr.Replace(fileId, tarFileId);
                    }
                }
            }
            return secStr;
        }
        private string ReplaceMaterial(string secStr)
        {
            if (m_regexMaterial.IsMatch(secStr))
            {
                var match = m_regexMaterial.Match(secStr);
                string fileId = match.Groups[1].Value;
                if (fileId != "0")
                {
                    string srcGuid = match.Groups[2].Value;
                    string tarFileId;
                    string tarGuid;
                    if (m_isSrcToTarget)
                        tarFileId = GetTargetMaterialFileId(srcGuid, fileId, out tarGuid);
                    else
                        tarFileId =  GetSrcMaterialFileId(srcGuid, fileId, out tarGuid);
                    if (tarFileId != null)
                    {
                        if (srcGuid != tarGuid)
                            secStr = secStr.Replace(srcGuid, tarGuid);
                        if (fileId != tarFileId)
                            secStr = secStr.Replace(fileId, tarFileId);
                    }
                }
            }
            return secStr;
        }
        private void ReplaceAllImage()
        {
            for (int i = 0; i < m_tarFileList.Count; i++)
            {
                m_tarFileList[i] = ReplaceImage(m_tarFileList[i]);
                m_tarFileList[i] = ReplaceMaterial(m_tarFileList[i]);
            }
        }
        private void ReplaceAllMaterial()
        {
            for (int i = 0; i < m_tarFileList.Count; i++)
            {
                m_tarFileList[i] = ReplaceMaterial(m_tarFileList[i]);
            }
        }
        private void WritePrefab(string targetFilePath)
        {
            if(!m_canCopy)
            {
                MessageBox.Show("该节点无法进行复制操作!");
                return;
            }
            if(m_tarFileList == null || m_tarFileList.Count <= 0)
            {
                MessageBox.Show("目标文件为空!");
                return;
            }
            Logic.ChangeProgress("开始写入预制...");
            FileInfo file = new FileInfo(targetFilePath);
            if (!File.Exists(targetFilePath))
            {
                FileStream fs = file.Create();
                fs.Close();
            }
            StreamWriter sw = file.CreateText();
            try
            {
                for (int i = 0; i < m_tarFileList.Count; i++)
                {
                    if(!m_regexSpace.IsMatch(m_tarFileList[i]) )
                    {
                        if (i == 0)
                            sw.WriteLine(m_tarFileList[i]);
                        else
                            sw.WriteLine(m_regex + m_tarFileList[i]);
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

        private string GetTargetSpriteFileId(string srcGuid, string srcFileId, out string tarGuid)
        {
            tarGuid = "";
            var metaInfo = Logic.SourceMetaInfo.GetMetaInfo(srcGuid, srcFileId);
            if (metaInfo == null) return null;
            string srcSpriteName = metaInfo.name;
            var tarMetaDic = Logic.TargetMetaInfo.GetSpriteMetaDic();
            foreach (var item in tarMetaDic)
            {
                if (item.Value.name.Equals(srcSpriteName))
                {
                    tarGuid = item.Value.guid;
                    Dictionary<string, MetaInfo> metas = Logic.TargetMetaInfo.GetMetaInfoDic(item.Value.guid);
                    foreach (var metaItem in metas)
                    {
                        if (metaItem.Value.name.Equals(srcSpriteName))
                            return metaItem.Value.fileId;
                    }
                    return null;
                }
            }
            return null;
        }
        private string GetSrcSpriteFileId(string tarGuid, string tarFileId, out string srcGuid)
        {
            srcGuid = "";
            var metaInfo = Logic.TargetMetaInfo.GetMetaInfo(tarGuid, tarFileId);
            if (metaInfo == null) return null;
            string tarSpriteName = metaInfo.name;
            var srcMetaDic = Logic.SourceMetaInfo.GetSpriteMetaDic();
            foreach (var item in srcMetaDic)
            {
                if (item.Value.name.Equals(tarSpriteName))
                {
                    srcGuid = item.Value.guid;
                    Dictionary<string, MetaInfo> metas = Logic.SourceMetaInfo.GetMetaInfoDic(item.Value.guid);
                    foreach (var metaItem in metas)
                    {
                        if (metaItem.Value.name.Equals(tarSpriteName))
                            return metaItem.Value.fileId;
                    }
                    return null;
                }
            }
            return null;
        }

        private string GetTargetMaterialFileId(string srcGuid, string srcFileId, out string tarGuid)
        {
            tarGuid = "";
            var metaInfo = Logic.SourceMetaInfo.GetMetaInfo(srcGuid, srcFileId);
            if (metaInfo == null) return null;
            string name = metaInfo.name;
            var tarMetaDic = Logic.TargetMetaInfo.GetMaterialMetaDic();
            foreach (var item in tarMetaDic)
            {
                if (item.Value.name.Equals(name))
                {
                    tarGuid = item.Value.guid;
                    Dictionary<string, MetaInfo> metas = Logic.TargetMetaInfo.GetMetaInfoDic(item.Value.guid);
                    foreach (var metaItem in metas)
                    {
                        if (metaItem.Value.name.Equals(name))
                            return metaItem.Value.fileId;
                    }
                    return null;
                }
            }
            return null;
        }
        private string GetSrcMaterialFileId(string tarGuid, string tarFileId, out string srcGuid)
        {
            srcGuid = "";
            var metaInfo = Logic.TargetMetaInfo.GetMetaInfo(tarGuid, tarFileId);
            if (metaInfo == null) return null;
            string name = metaInfo.name;
            var srcMetaDic = Logic.SourceMetaInfo.GetMaterialMetaDic();
            foreach (var item in srcMetaDic)
            {
                if (item.Value.name.Equals(name))
                {
                    srcGuid = item.Value.guid;
                    Dictionary<string, MetaInfo> metas = Logic.SourceMetaInfo.GetMetaInfoDic(item.Value.guid);
                    foreach (var metaItem in metas)
                    {
                        if (metaItem.Value.name.Equals(name))
                            return metaItem.Value.fileId;
                    }
                    return null;
                }
            }
            return null;
        }
    }
}
