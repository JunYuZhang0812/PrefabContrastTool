using PrefabContrastTool.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PrefabContrastTool.Prefab
{
    public class Prefab
    {
        public string filePath;
        public GameObject root { get; set; }//根节点
        public string rootFileId { get; set; }
        public Prefab(bool isSrc , string fullPath )
        {
            m_isSrc = isSrc;
            filePath = fullPath;
            if( !File.Exists(fullPath))
            {
                MessageBox.Show("文件不存在！" + fullPath);
                return;
            }
            BeginParse();
        }
        //重新解析预制
        public void AfreshParsePrefab()
        {
            m_dicComponent.Clear();
            m_dicGameObject.Clear();
            m_otherScripts.Clear();
            m_listComStr = null;
            BeginParse();
        }
        #region 私有逻辑
        private bool m_isSrc;
        private Dictionary<string, Component> m_dicComponent = new Dictionary<string, Component>();//fileId,Component
        private Dictionary<string, GameObject> m_dicGameObject = new Dictionary<string, GameObject>();//fileId,GameObject
        private List<Script> m_otherScripts = new List<Script>();
        private List<string> m_listComStr;
        #region Regex
        private Regex m_sectionTag = new Regex("--- !u!");
        private Regex m_regSelfFileId = new Regex(@"--- !u!\d+\s&(\d+)");
        private Regex m_regComType = new Regex(@"^(\w+):$");
        private Regex m_level2Filed = new Regex(@"^  [^ ]");
        private Regex m_level3Filed = new Regex(@"^  ( |-) [^ ]");
        private Regex m_level3Index = new Regex(@"^  - [^ ]");
        private Regex m_level4Filed = new Regex(@"^    ( |-) [^ ]");
        private Regex m_level4Index = new Regex(@"^    - [^ ]");
        private Regex m_regFiledName = new Regex(@"\b(\w|\d| )+\b");
        private Regex m_regFileId = new Regex(@"fileID:\s(-?\d+)");
        private Regex m_regGuid = new Regex(@"guid:\s((?:\w|\d)+)");
        private Regex m_regLine = new Regex(@"\n");
        private Regex m_regId = new Regex(@"(-?\d+)");
        private Regex m_regColor = new Regex(@"r:\s((?:\d|\.)+),\sg:\s((?:\d|\.)+),\sb:\s((?:\d|\.)+),\sa:\s((?:\d|\.)+)");
        #endregion

        private void BeginParse()
        {
            var allStr = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(allStr)) return;
            string[] arrComStr = m_sectionTag.Split(allStr);
            m_listComStr = arrComStr.ToList();
            for (int i = 0; i < m_listComStr.Count; i++)
            {
                ParseAllComponentStr(i);
            }
            root = m_dicGameObject[rootFileId];
            root.fileFullPath = filePath;
            root.prefab = this;
            FullGameObject();
            FullGameObjectParentAndChilds();
            FullNodePath();
            FormatMembers();
        }
        private Regex m_regUnicode = new Regex(@"\\u([0-9A-F]{4})",RegexOptions.IgnoreCase | RegexOptions.Compiled );
        private string FormatUnicode( string str )
        {
            if (m_regUnicode.IsMatch(str))
            {
                System.Text.StringBuilder newStr = new System.Text.StringBuilder();
                var matchs = m_regUnicode.Matches(str);
                int index = 0;
                for (int i = 0; i < matchs.Count; i++)
                {
                    var match = matchs[i];
                    if (match.Index > index)
                    {
                        newStr.Append(str.Substring(index, match.Index - index));
                    }
                    newStr.Append(Convert.ToChar(Convert.ToUInt16(matchs[i].Groups[1].Value, 16)));
                    index = match.Index + match.Groups[0].Length;
                }
                if (str.Length > index)
                {
                    newStr.Append(str.Substring(index, str.Length - index));
                }
                return newStr.ToString();
            }
            return str;
        }
        private void SetValue(Member member )
        {
            var key = member.key;
            var value = member.value;
            if ( key == "m_Enabled" || key == "m_Interactable")
            {
                value = (value == "1").ToString();
            }
            else if(m_regColor.IsMatch(value))
            {
                var match = m_regColor.Match(value);
                var r = match.Groups[1].Value;
                var g = match.Groups[2].Value;
                var b = match.Groups[3].Value;
                var a = match.Groups[4].Value;
                r = Math.Floor(float.Parse(r) * 255 ).ToString();
                g = Math.Floor(float.Parse(g) * 255).ToString();
                b = Math.Floor(float.Parse(b) * 255).ToString();
                a = Math.Floor(float.Parse(a) * 255).ToString();
                value = string.Format("r:{0}  g:{1}  b:{2}  a:{3}" , r,g,b,a);
            }
            else if(m_regUnicode.IsMatch(value))
            {
                var matchs = m_regUnicode.Matches(value);
                value = "";
                for (int i = 0; i < matchs.Count; i++)
                {
                    value += Convert.ToChar(Convert.ToUInt16(matchs[i].Groups[1].Value, 16));
                }
            }
            else
            {
                string guid = null;
                if (m_regGuid.IsMatch(value))
                {
                    guid = m_regGuid.Match(value).Groups[1].Value;
                }
                string fileId = null;
                if (m_regFileId.IsMatch(value))
                {
                    fileId = m_regFileId.Match(value).Groups[1].Value;
                }
                else if(key.Equals("fileID"))
                {
                    fileId = value;
                }
                if (guid == null)
                {
                    if (fileId != null)
                    {
                        if (fileId == "0")
                        {
                            value = "null";
                        }
                        else
                        {
                            if (m_dicComponent.ContainsKey(fileId))
                            {
                                var com = m_dicComponent[fileId];
                                if (com.gameObject != null)
                                {
                                    value = com.gameObject.name;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (fileId != null)
                    {
                        value = GetMetaInfoName(guid, fileId);
                    }
                }
            }
            member.value = value;
        }
        private void FormatMemberValue(Dictionary<string, Member> members)
        {
            foreach (Member member in members.Values)
            {
                if (member.valueList != null)
                {
                    for (int i = 0; i < member.valueList.Count; i++)
                    {
                        FormatMemberValue(member.valueList[i]);
                    }
                }
                else
                {
                    SetValue(member);
                }
            }
        }
        private string GetMetaInfoName( string guid,string fileId)
        {
            if (m_isSrc)
            {
                return Logic.SourceMetaInfo.GetMetaInfoName(guid, fileId);
            }
            else
            {
                return Logic.TargetMetaInfo.GetMetaInfoName(guid, fileId);
            }
        }
        Regex m_regPreTag = new Regex(@"^( |-)*");
        private int PreTagCount( string line , out bool isList )
        {
            var str = m_regPreTag.Match(line).Value;
            isList = str.Contains('-');
            return str.Length;
        }
        struct MemberParent
        {
            public Member parent;
            public Dictionary<string, Member> members;
        }
        private Stack<MemberParent> m_memberParentStack = new Stack<MemberParent>();
        private void ParseAllComponentStr( int index )
        {
            if (index == 0) return;
            var comStr = m_sectionTag.ToString()+ m_listComStr[index];
            Component com = new Component();
            com.index = index;
            com.comStr = comStr;
            var lines = m_regLine.Split(comStr);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                if(m_regSelfFileId.IsMatch( line ) )
                {
                    com.fileId = m_regSelfFileId.Match(line).Groups[1].Value;
                }
                if(m_regComType.IsMatch(line))
                {
                    var typeStr = m_regComType.Match(line).Groups[1].Value;
                    com.name = typeStr;
                    com.type = GetComponentType(typeStr);
                }
                if(m_level2Filed.IsMatch(line))//是二级字段
                {
                    bool currIsList = false;
                    var preTag = PreTagCount(line,out currIsList);
                    Member member = new Member();
                    var speIndex = line.IndexOf(':');
                    var name = line.Substring(0, speIndex ).Trim();
                    var value = line.Substring(speIndex+1).Trim();

                    if ( i < lines.Length-1)
                    {
                        var nextLine = lines[i + 1];
                        var curPreTag = preTag;
                        Member currParent = null;
                        bool nextIsList = false;
                        var nextPreTag = PreTagCount(nextLine , out nextIsList);
                        Dictionary<string, Member> newMembers = null;
                        Member newMember = member;
                        while (nextPreTag > preTag)//三级字段
                        {
                            var speIndex2 = nextLine.IndexOf(':');
                            if (speIndex2 == -1) //继续上一个member的值
                            {
                                newMember.value += nextLine.Trim();
                            }
                            else
                            {
                                if (nextPreTag > curPreTag)
                                {
                                    if (currParent != null)
                                    {
                                        m_memberParentStack.Push(new MemberParent()
                                        {
                                            parent = currParent,
                                            members = newMembers,
                                        });
                                    }
                                    currParent = newMember;
                                    if (currParent.valueList == null)
                                    {
                                        currParent.valueList = new List<Dictionary<string, Member>>();
                                    }
                                    newMembers = new Dictionary<string, Member>();
                                    currParent.valueList.Add(newMembers);
                                }
                                else if (nextPreTag < curPreTag)
                                {
                                    var memberParent = m_memberParentStack.Pop();
                                    currParent = memberParent.parent;
                                    newMembers = memberParent.members;
                                }
                                else
                                {
                                    if (nextIsList)
                                    {
                                        newMembers = new Dictionary<string, Member>();
                                        currParent.valueList.Add(newMembers);
                                    }
                                }
                                curPreTag = nextPreTag;

                                newMember = new Member();
                                var name2 = nextLine.Substring(0, speIndex2).Trim();
                                name2 = m_regFiledName.Match(name2).Value;
                                var value2 = nextLine.Substring(speIndex2 + 1).Trim();
                                if (name2.Equals("fileID"))
                                {
                                    value2 = m_regId.Match(value2).Groups[1].Value;
                                }
                                newMember.key = name2;
                                newMember.value = value2;
                                newMembers.Add(name2, newMember);
                            }

                            i = i + 1;
                            if (i >= lines.Length - 1)
                                break;
                            nextLine = lines[i + 1];
                            nextPreTag = PreTagCount(nextLine, out nextIsList);
                        }
                        /*Dictionary<string, Member> newMembers = null;
                        bool isAdd = false;
                        while (m_level3Filed.IsMatch(nextLine))
                        {
                            //if( !addN )
                            //{
                            //    addN = true;
                            //    value += nextLine;
                            //}
                            //else
                            //{
                            //    value += "\n" + nextLine;
                            //}
                            if (m_level3Index.IsMatch(nextLine) || !isAdd )
                            {
                                isAdd = true;
                                newMembers = new Dictionary<string, Member>();
                                if (member.valueList == null)
                                {
                                    member.valueList = new List<Dictionary<string, Member>>();
                                }
                                member.valueList.Add(newMembers);
                            }
                            Member newMember = new Member();
                            var speIndex2 = nextLine.IndexOf(':');
                            var name2 = nextLine.Substring(0, speIndex2).Trim();
                            name2 = m_regFiledName.Match(name2).Value;
                            var value2 = nextLine.Substring(speIndex2 + 1).Trim();
                            if(name2.Equals("fileID"))
                            {
                                value2 = m_regId.Match(value2).Groups[1].Value;
                            }
                            newMember.key = name2;
                            newMember.value = value2;
                            newMembers.Add(name2, newMember);
                            i = i + 1;
                            if (i >= lines.Length-1)
                                break;
                            nextLine = lines[i+1];
                        }*/
                    }
                    member.key = name;
                    member.value = value;
                    com.members.Add(name, member);
                }
            }
            if (com.type == ComponentType.GameObject)
            {
                m_dicGameObject.Add(com.fileId, new GameObject());
            }
            if (com.type == ComponentType.Prefab)
            {
                var rootGO = GetComponentInfo(com.members , "m_RootGameObject");
                rootFileId = m_regFileId.Match(rootGO).Groups[1].Value;
            }
            m_dicComponent.Add(com.fileId, com);
        }
        private string GetComponentInfo( Dictionary<string,Member> info , string key )
        {
            Member value;
            if( info.TryGetValue(key,out value))
            {
                return value.value;
            }
            return "";
        }
        private List<Dictionary<string,Member>> GetComponentInfoList(Dictionary<string, Member> info, string key)
        {
            Member value;
            if (info.TryGetValue(key, out value))
            {
                return value.valueList;
            }
            return null;
        }
        private string GetStringInfo( string str , string key )
        {
            Regex reg = new Regex(key + @":\s(.*)");
            if( reg.IsMatch(str))
            {
                return reg.Match(str).Groups[1].Value;
            }
            return "";
        }
        private string GetFileId( string str )
        {
            if(m_regFileId.IsMatch(str))
            {
                var match = m_regFileId.Match(str);
                if (match != null)
                {
                    return match.Groups[1].Value;
                }
            }
            return "0";
        }
        /// <summary>
        /// 填充GameObject数据
        /// </summary>
        private void FullGameObject()
        {
            foreach (var item in m_dicComponent)
            {
                var fileId = item.Key;
                Component com = item.Value;
                if( com.type == ComponentType.GameObject )
                {
                    GameObject go = m_dicGameObject[fileId];
                    go.name = GetComponentInfo(com.members, "m_Name");
                    go.isActive = GetComponentInfo(com.members, "m_IsActive") == "1";
                    go.fileId = com.fileId;
                    go.root = root;
                    //var comStr = GetComponentInfo( com.members ,"m_Component" );
                    //if(m_regFileId.IsMatch(comStr))
                    var comStrList = GetComponentInfoList(com.members, "m_Component");
                    for (int i = 0; i < comStrList.Count; i++)
                    {
                        //var matches = m_regFileId.Matches(comStr);
                        //for (int i = 0; i < matches.Count; i++)
                        var comStr = comStrList[i]["component"].value;
                        if (m_regFileId.IsMatch(comStr))
                        {
                            var match = m_regFileId.Match(comStr);
                            //填充Component
                            //var _fileId = matches[i].Groups[1].Value;
                            var _fileId = match.Groups[1].Value;
                            var _com = m_dicComponent[_fileId];
                            _com.gameObject = go;
                            if (_com.type == ComponentType.RectTransform )
                            {
                                RectTransform rect = Logic.AutoCopy<Component,RectTransform>(_com);
                                rect.isTransform = false;
                                //var childrenStr = GetComponentInfo(_com.members, "m_Children");
                                //var childIds = m_regFileId.Matches(childrenStr);
                                var childrenStrList = GetComponentInfoList(_com.members, "m_Children");
                                if(childrenStrList != null)
                                {
                                    for (int j = 0; j < childrenStrList.Count; j++)
                                    {
                                        var idStr = childrenStrList[j]["fileID"].value;
                                        rect.childIds.Add(idStr);
                                    }
                                }
                                /*for (int j = 0; j < childIds.Count; j++)
                                {
                                    rect.childIds.Add(childIds[j].Groups[1].Value);
                                }*/
                                var parentId = GetFileId(GetComponentInfo(_com.members ,"m_Father" ) );
                                if (!parentId.Equals("0"))
                                    rect.parentId = parentId;
                                rect.localPosition = new Vector(GetComponentInfo(_com.members, "m_LocalPosition"));
                                rect.localRotation = new Vector(GetComponentInfo(_com.members, "m_LocalRotation"));
                                rect.localScale = new Vector(GetComponentInfo(_com.members, "m_LocalScale"));
                                rect.anchoredPosition = new Vector(GetComponentInfo(_com.members, "m_AnchoredPosition"));
                                rect.sizeDelta = new Vector(GetComponentInfo(_com.members, "m_SizeDelta"));
                                rect.anchorMin = new Vector(GetComponentInfo(_com.members, "m_AnchorMin"));
                                rect.anchorMax = new Vector(GetComponentInfo(_com.members, "m_AnchorMax"));
                                rect.pivot = new Vector(GetComponentInfo(_com.members, "m_Pivot"));
                                go.components.Add(rect);
                                go.rectTransform = rect;

                                var script = new Script("0", "0");
                                script.type = ScriptType.RectTransform;
                                script.gameObject = go;
                                script.name = _com.name;
                                script.members = _com.members;
                                go.scripts.Add(script);
                                m_otherScripts.Add(script);
                            }
                            else if( _com.type == ComponentType.Transform )
                            {
                                RectTransform rect = Logic.AutoCopy<Component, RectTransform>(_com);
                                rect.isTransform = true;
                                var childrenStrList = GetComponentInfoList(_com.members, "m_Children");
                                if (childrenStrList != null)
                                {
                                    for (int j = 0; j < childrenStrList.Count; j++)
                                    {
                                        var idStr = childrenStrList[j]["fileID"].value;
                                        rect.childIds.Add(idStr);
                                    }
                                }
                                var parentId = GetFileId(GetComponentInfo(_com.members, "m_Father"));
                                if (!parentId.Equals("0"))
                                    rect.parentId = parentId;
                                rect.localPosition = new Vector(GetComponentInfo(_com.members, "m_LocalPosition"));
                                rect.localRotation = new Vector(GetComponentInfo(_com.members, "m_LocalRotation"));
                                rect.localScale = new Vector(GetComponentInfo(_com.members, "m_LocalScale"));
                                go.components.Add(rect);
                                go.rectTransform = rect;

                                var script = new Script("0", "0");
                                script.type = ScriptType.RectTransform;
                                script.gameObject = go;
                                script.name = _com.name;
                                script.members = _com.members;
                                go.scripts.Add(script);
                                m_otherScripts.Add(script);
                            }
                            else if(_com.type == ComponentType.MonoBehaviour)
                            {
                                MonoBehaviour mono = Logic.AutoCopy<Component, MonoBehaviour>(_com);
                                mono.enabled = GetComponentInfo(_com.members,"m_Enabled") == "1";
                                bool isScript = true;
                                var scriptType = ScriptType.Other;
                                var materialStr = GetComponentInfo(_com.members,"m_Material");
                                var materialFileId = GetFileId(materialStr);
                                if( !materialFileId.Equals("0") )
                                {
                                    scriptType = ScriptType.MonoBehaviour;
                                    isScript = false;
                                    var guid = m_regGuid.Match(materialStr).Groups[1].Value;
                                    mono.material = new Material(guid , materialFileId);
                                    mono.material.name = GetMetaInfoName(guid, materialFileId);
                                }
                                var spriteStr = GetComponentInfo(_com.members, "m_Sprite");
                                var spriteFileId = GetFileId(spriteStr);
                                if (!spriteFileId.Equals("0"))
                                {
                                    scriptType = ScriptType.MonoBehaviour;
                                    isScript = false;
                                    var guid = m_regGuid.Match(spriteStr).Groups[1].Value;
                                    mono.sprite = new Sprite(guid, spriteFileId);
                                    mono.sprite.name = GetMetaInfoName(guid, spriteFileId);
                                    mono.sprite.raycastTarget = GetComponentInfo(_com.members, "m_RaycastTarget");
                                    mono.sprite.color = new Color(GetComponentInfo(_com.members, "m_Color"));
                                }
                                var fontDataList = GetComponentInfoList(_com.members, "m_FontData");
                                Dictionary<string, Member> fontDataDic = null;
                                if(fontDataList != null)
                                {
                                    fontDataDic = fontDataList[0];
                                    var fontStr = fontDataDic["m_Font"].value;
                                    var fontFileId = GetFileId(fontStr);
                                    if (!fontFileId.Equals("0"))
                                    {
                                        scriptType = ScriptType.MonoBehaviour;
                                        isScript = false;
                                        var guid = m_regGuid.Match(fontStr).Groups[1].Value;
                                        mono.font = new Font(guid, fontFileId);
                                        mono.font.name = GetMetaInfoName(guid, fontFileId);
                                        mono.font.text = FormatUnicode( GetComponentInfo(_com.members, "m_Text"));
                                        mono.font.size = fontDataDic["m_FontSize"].value;
                                        mono.font.aligment = fontDataDic["m_Alignment"].value;
                                        mono.font.lineSpacing = fontDataDic["m_LineSpacing"].value;
                                        mono.font.raycastTarget = GetComponentInfo(_com.members, "m_RaycastTarget");
                                        mono.font.color = new Color(GetComponentInfo(_com.members, "m_Color"));
                                    }
                                }
                                var scriptStr = GetComponentInfo(_com.members, "m_Script");
                                var scriptFileId = GetFileId(scriptStr);
                                if (!scriptFileId.Equals("0"))
                                {
                                    var guid = m_regGuid.Match(scriptStr).Groups[1].Value;
                                    mono.script = new Script(guid, scriptFileId);
                                    mono.script.gameObject = go;
                                    mono.script.type = scriptType;
                                    mono.script.name = GetMetaInfoName(guid, scriptFileId);
                                    mono.script.members = _com.members;
                                    m_otherScripts.Add(mono.script);
                                }
                                if( mono.script == null )
                                {
                                    var script = new Script("0", "0");
                                    script.gameObject = go;
                                    script.type = ScriptType.Other;
                                    script.name = Logic.MissName;
                                    script.members = _com.members;
                                    mono.script = script;
                                    m_otherScripts.Add(mono.script);
                                }
                                go.components.Add(mono);
                                go.scripts.Add(mono.script);
                                if (!isScript)
                                {
                                    if( go.monoBehaviour != null )
                                    {
                                        //MessageBox.Show("有多个MonoBehaviour!path" + go.path);
                                        go.differentType = Logic.SetFlag(go.differentType, DifferentType.MutiMono,true);
                                    }
                                    go.monoBehaviour = mono;
                                }
                            }
                            else
                            {
                                go.components.Add(_com);
                                var script = new Script("0", "0");
                                script.gameObject = go;
                                script.type = ScriptType.Other;
                                script.name = _com.name;
                                script.members = _com.members;
                                go.scripts.Add(script);
                                m_otherScripts.Add(script);
                            }
                        }
                    }
                }
            }
        }
        
        private void FullGameObjectParentAndChilds(GameObject gameObject = null )
        {
            if (gameObject == null) gameObject = root;
            var rect = gameObject.rectTransform;
            Component parent;
            if (m_dicComponent.TryGetValue(rect.parentId, out parent))
            {
                gameObject.parent = parent.gameObject;
            }
            for (int i = 0; i < rect.childIds.Count; i++)
            {
                Component child;
                if (m_dicComponent.TryGetValue(rect.childIds[i], out child))
                {
                    gameObject.childs.Add(child.gameObject);
                    FullGameObjectParentAndChilds(child.gameObject);
                }
            }
        }
        //填充节点路径
        private void FullNodePath( GameObject gameObject = null , string path = null )
        {
            if (gameObject == null)
            {
                gameObject = root;
                path = gameObject.name;
            }
            else
            {
                path += "/" + gameObject.name;
            }
            gameObject.path = path;
            var childs = gameObject.childs;
            for (int i = 0; i < childs.Count; i++)
            {
                FullNodePath(childs[i], path);
            }
        }
        private void FormatMembers()
        {
            for (int i = 0; i < m_otherScripts.Count; i++)
            {
                FormatMemberValue(m_otherScripts[i].members);
            }
        }
        private ComponentType GetComponentType( string name )
        {
            if (name.Equals("Prefab"))
                return ComponentType.Prefab;
            else if (name.Equals("GameObject"))
                return ComponentType.GameObject;
            else if (name.Equals("MonoBehaviour"))
                return ComponentType.MonoBehaviour;
            else if (name.Equals("RectTransform"))
                return ComponentType.RectTransform;
            else if (name.Equals("Transform"))
                return ComponentType.Transform;
            else if (name.Equals("CanvasRenderer"))
                return ComponentType.CanvasRenderer;
            return ComponentType.Other;
        }
        #endregion
    }
}
