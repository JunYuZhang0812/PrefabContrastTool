using PrefabContrastTool.Core;
using PrefabContrastTool.Prefab;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

public static class Logic
{
    public const string MissName = "MISS";
    public static bool m_isSvnCompare = false;
    public static Regex m_regPrefabName = new Regex(@".*?(?=\.prefab)");
    public static Regex m_regClientPath = new Regex(@".*(?=(\\|//)Assets)");
    #region 对外接口
    #region 属性
    private static string[] drives = new string[] { "D:", "E:", "F:", "G:" };
    private static string _APPDataPath = null;
    public static string APPDataPath
    {
        get
        {
            if (_APPDataPath == null)
            {
                var addPath = @"\APPData\预制对比工具";
                for (int i = 0; i < drives.Length; i++)
                {
                    if (Directory.Exists(drives[i]))
                    {
                        _APPDataPath = drives[i] + addPath;
                        if (!Directory.Exists(_APPDataPath))
                        {
                            Directory.CreateDirectory(_APPDataPath);
                        }
                        break;
                    }
                }
            }
            return _APPDataPath;
        }
    }
    public static SelectFileModeEnum SelectFileMode
    {
        get
        {
            return (SelectFileModeEnum)FileOP.Instance.ReadInt("Config", "SelectFileMode", 0);
        }
        set
        {
            FileOP.Instance.WriteInt("Config", "SelectFileMode", (int)value);
            IsFileMode = value == SelectFileModeEnum.SelectFiles;
        }
    }
    private static bool _IsFileMode;
    public static bool IsFileMode
    {
        get
        {
            return _IsFileMode;
        }
        set
        {
            _IsFileMode = value;
            _SourceClientPath = null;
            _TargetClientPath = null;
            _SourceFilePath = null;
            _TargetFilePath = null;
        }
    }
    /// <summary>
    /// 源文件路径
    /// </summary>
    private static string _SourceFilePath;
    public static string SourceFilePath
    {
        get
        {
            if( string.IsNullOrEmpty(_SourceFilePath))
            {
                var filed = IsFileMode ? "SourceFile" : "SourceFolder";
                if( Logic.m_isSvnCompare )
                {
                    filed = "SVN" + filed;
                }
                _SourceFilePath = FileOP.Instance.ReadString("Path", filed );
            }
            return _SourceFilePath;
        }
        set
        {
            _SourceFilePath = value;
            var filed = IsFileMode ? "SourceFile" : "SourceFolder";
            if (Logic.m_isSvnCompare)
            {
                filed = "SVN" + filed;
            }
            FileOP.Instance.WriteString("Path", filed, value );
            SourceFiles = value.Split(';');
            if ( !Logic.m_isSvnCompare )
            {
                if (SourceFiles.Length > 0)
                {
                    SourceClientPath = m_regClientPath.Match(SourceFiles[0]).Value;
                }
                else
                {
                    SourceClientPath = m_regClientPath.Match(value).Value;
                }
            }
        }
    }
    /// <summary>
    /// 目标文件路径
    /// </summary>
    private static string _TargetFilePath;
    public static string TargetFilePath
    {
        get
        {
            if (string.IsNullOrEmpty(_TargetFilePath))
            {
                var filed = IsFileMode ? "TargetFile" : "TargetFolder";
                if (Logic.m_isSvnCompare)
                {
                    filed = "SVN" + filed;
                }
                _TargetFilePath = FileOP.Instance.ReadString("Path", filed);
            }
            return _TargetFilePath;
        }
        set
        {
            _TargetFilePath = value;
            var filed = IsFileMode ? "TargetFile" : "TargetFolder";
            if (Logic.m_isSvnCompare)
            {
                filed = "SVN" + filed;
            }
            FileOP.Instance.WriteString("Path", filed, value);
            TargetFiles = value.Split(';');
            if (!Logic.m_isSvnCompare)
            {
                if (TargetFiles.Length > 0)
                {
                    TargetClientPath = m_regClientPath.Match(TargetFiles[0]).Value;
                }
                else
                {
                    TargetClientPath = m_regClientPath.Match(value).Value;
                }
            }
        }
    }
    /// <summary>
    /// 源工程Client路径
    /// </summary>
    private static string _SourceClientPath;
    public static string SourceClientPath
    {
        get
        {
            if( string.IsNullOrEmpty(_SourceClientPath) )
            {
                var files = SourceFilePath.Split(';');
                _SourceClientPath = m_regClientPath.Match(files[0]).Value;
            }
            return _SourceClientPath;
        }
        set
        {
            _SourceClientPath = value;
            if(SourceMetaInfo == null || SourceMetaInfo.ClientPath != value )
                SourceMetaInfo = new MetaManager( value , true );
        }
    }
    /// <summary>
    /// 目标工程Client路径
    /// </summary>
    private static string _TargetClientPath;
    public static string TargetClientPath
    {
        get
        {
            if (string.IsNullOrEmpty(_TargetClientPath))
            {
                var files = TargetFilePath.Split(';');
                _TargetClientPath = m_regClientPath.Match(files[0]).Value;
            }
            return _TargetClientPath;
        }
        set
        {
            _TargetClientPath = value;
            if (TargetMetaInfo == null || TargetMetaInfo.ClientPath != value)
                TargetMetaInfo = new MetaManager(value , false );
        }
    }
    /// <summary>
    /// 源工程MetaInfo
    /// </summary>
    public static MetaManager SourceMetaInfo { get; set; }
    /// <summary>
    /// 目标工程MetaInfo
    /// </summary>
    public static MetaManager TargetMetaInfo { get; set; }
    /// <summary>
    /// 显示全部
    /// </summary>
    private static int _ShowAll = -1;
    public static bool ShowAll
    {
        get
        {
            if (_ShowAll == -1)
            {
                _ShowAll = FileOP.Instance.ReadInt("Config", "ShowAll", 0);
            }
            return _ShowAll == 1;
        }
        set
        {
            _ShowAll = value ? 1 : 0;
            FileOP.Instance.WriteInt("Config", "ShowAll", _ShowAll);
        }
    }
    /// <summary>
    /// 对比脚本
    /// </summary>
    private static int _CheckScript = -1;
    public static bool CheckScript
    {
        get
        {
            if (_CheckScript == -1)
            {
                _CheckScript = FileOP.Instance.ReadInt("Config", "CheckScript", 0);
            }
            return _CheckScript == 1;
        }
        set
        {
            _CheckScript = value ? 1 : 0;
            FileOP.Instance.WriteInt("Config", "CheckScript", _CheckScript);
        }
    }
    /// <summary>
    /// 对比材质
    /// </summary>
    private static int _CheckMaterial = -1;
    public static bool CheckMaterial
    {
        get
        {
            if (_CheckMaterial == -1)
            {
                _CheckMaterial = FileOP.Instance.ReadInt("Config", "CheckMaterial", 0);
            }
            return _CheckMaterial == 1;
        }
        set
        {
            _CheckMaterial = value ? 1 : 0;
            FileOP.Instance.WriteInt("Config", "CheckMaterial", _CheckMaterial);
        }
    }
    /// <summary>
    /// 对比图片
    /// </summary>
    private static int _CheckImage = -1;
    public static bool CheckImage
    {
        get
        {
            if (_CheckImage == -1)
            {
                _CheckImage = FileOP.Instance.ReadInt("Config", "CheckImage", 0);
            }
            return _CheckImage == 1;
        }
        set
        {
            _CheckImage = value ? 1 : 0;
            FileOP.Instance.WriteInt("Config", "CheckImage", _CheckImage);
        }
    }
    /// <summary>
    /// 对比字体
    /// </summary>
    private static int _CheckFont = -1;
    public static bool CheckFont
    {
        get
        {
            if (_CheckFont == -1)
            {
                _CheckFont = FileOP.Instance.ReadInt("Config", "CheckFont", 0);
            }
            return _CheckFont == 1;
        }
        set
        {
            _CheckFont = value ? 1 : 0;
            FileOP.Instance.WriteInt("Config", "CheckFont", _CheckFont);
        }
    }
    /// <summary>
    /// 对比Active状态
    /// </summary>
    private static int _CheckActive = -1;
    public static bool CheckActive
    {
        get
        {
            if (_CheckActive == -1)
            {
                _CheckActive = FileOP.Instance.ReadInt("Config", "CheckActive", 0);
            }
            return _CheckActive == 1;
        }
        set
        {
            _CheckActive = value ? 1 : 0;
            FileOP.Instance.WriteInt("Config", "CheckActive", _CheckActive);
        }
    }
    //忽略偏移
    private static int _IgnorePos = -1;
    public static bool IgnorePos
    {
        get
        {
            if (_IgnorePos == -1)
            {
                _IgnorePos = FileOP.Instance.ReadInt("Config", "IgnorePos", 0);
            }
            return _IgnorePos == 1;
        }
        set
        {
            _IgnorePos = value ? 1 : 0;
            FileOP.Instance.WriteInt("Config", "IgnorePos", _IgnorePos);
        }
    }
    #endregion
    #region 方法
    public static bool HasFlag(DifferentType mflag, DifferentType flag)
    {
        return (mflag & flag) != 0;
    }
    public static DifferentType SetFlag(DifferentType mflag, DifferentType flag, bool set)
    {
        mflag &= ~flag;
        if (set)
        {
            mflag |= flag;
        }
        return mflag;
    }
    //将父类属性数值转给子类
    public static TChild AutoCopy<TParent, TChild>(TParent parent) where TChild : TParent, new()
    {
        TChild child = new TChild();
        var ParentType = typeof(TParent);
        var Properties = ParentType.GetProperties();
        foreach (var Propertie in Properties)
        {
            //循环遍历属性
            if (Propertie.CanRead && Propertie.CanWrite)
            {
                //进行属性拷贝
                Propertie.SetValue(child, Propertie.GetValue(parent, null), null);
            }
        }
        return child;
    }
    public static string GetEnumDescription<T>(T obj)
    {
        var type = obj.GetType();
        FieldInfo field = type.GetField(Enum.GetName(type, obj));
        DescriptionAttribute desAttr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (desAttr == null)
            return string.Empty;
        return desAttr.Description;
    }
    public static System.Drawing.Color GetColorByDifferentType(DifferentType differentType)
    {
        if (differentType == DifferentType.Null || HasFlag(differentType, DifferentType.NodePath) || HasFlag(differentType, DifferentType.NodeName))
            return System.Drawing.Color.Red;
        else if (HasFlag(differentType, DifferentType.Matrial) || HasFlag(differentType, DifferentType.MatrialMiss))
            return System.Drawing.Color.Blue;
        else if (HasFlag(differentType, DifferentType.Sprite) || HasFlag(differentType, DifferentType.SpriteMiss))
            return System.Drawing.Color.Green;
        else if (HasFlag(differentType, DifferentType.Font) || HasFlag(differentType, DifferentType.FontMiss))
            return System.Drawing.Color.Orange;
        else if (HasFlag(differentType, DifferentType.Active))
            return System.Drawing.Color.Purple;
        else if (HasFlag(differentType, DifferentType.Script) || HasFlag(differentType, DifferentType.ScriptMiss) || HasFlag(differentType,DifferentType.MutiMono) )
            return System.Drawing.Color.Red;
        else
            return System.Drawing.Color.Black;
    }
    public static string GetStrByDifferentType(DifferentType differentType)
    {
        string str = "";
        if (HasFlag(differentType, DifferentType.Matrial))
        {
            str += "材质差异";
        }
        if (HasFlag(differentType, DifferentType.MatrialMiss))
        {
            str += str == "" ? "材质Miss" : "|材质Miss";
        }
        if (HasFlag(differentType, DifferentType.Sprite))
        {
            str += str == "" ? "图片差异" : "|图片差异";
        }
        if (HasFlag(differentType, DifferentType.SpriteMiss))
        {
            str += str == "" ? "图片Miss" : "|图片Miss";
        }
        if (HasFlag(differentType, DifferentType.Font))
        {
            str += str == "" ? "字体Null" : "|字体Null";
        }
        if (HasFlag(differentType, DifferentType.FontMiss))
        {
            str += str == "" ? "字体Miss" : "|字体Miss";
        }
        if (HasFlag(differentType, DifferentType.Active))
        {
            str += str == "" ? "Active差异" : "|Active差异";
        }
        if (HasFlag(differentType, DifferentType.Script))
        {
            str += str == "" ? "脚本差异" : "|脚本差异";
        }
        if (HasFlag(differentType, DifferentType.ScriptMiss))
        {
            str += str == "" ? "脚本Miss" : "|脚本Miss";
        }
        if (HasFlag(differentType, DifferentType.MutiMono))
        {
            str += str == "" ? "多个Mono" : "|多个Mono";
        }
        return str;
    }
    #endregion
    #endregion
    #region 界面流程相关
    public static List<RootDirectory> SourceDirList { get; set; } = new List<RootDirectory>();
    public static List<RootDirectory> TargetDirList { get; set; } = new List<RootDirectory>();
    public static Dictionary<string, Prefab> DicSourcePrefab = new Dictionary<string, Prefab>();//path,Prefab
    public static Dictionary<string, Prefab> DicTargetPrefab = new Dictionary<string, Prefab>();//path,Prefab
    public static string[] SourceFiles;
    public static string[] TargetFiles;
    public static BackgroundWorker AsyncWorker;
    public static string CurrStateTips { get; set; } = "";
    private static Regex m_regSourcePath;//匹配源目录相对路径
    private static Regex m_regTargetPath;//匹配目标目录相对路径
    private static int m_currProgress { get; set; } = 0; //当前进度
    public static int ReadyBegin()
    {
        if( IsFileMode)
        {
            //选择的时候就设置好了
            //SourceFiles = SourceFilePath.Split(';');
            //TargetFiles = TargetFilePath.Split(';');
        }
        else
        {
            SourceFiles = Directory.GetFiles(SourceFilePath, "*.prefab", SearchOption.AllDirectories);
            TargetFiles = Directory.GetFiles(TargetFilePath, "*.prefab", SearchOption.AllDirectories);
        }
        SourceMetaInfo.Clear();
        TargetMetaInfo.Clear();
        return SourceFiles.Length + TargetFiles.Length;
    }
    public static void Begin()
    {
        if(!IsFileMode)
        {
            m_regSourcePath = new Regex(@"(?<=" + Path.GetFileNameWithoutExtension(SourceFilePath) + "\\\\).*");
            m_regTargetPath = new Regex(@"(?<="+ Path.GetFileNameWithoutExtension(TargetFilePath) + "\\\\).*" );
        }
        m_currProgress = 0;
        SourceDirList.Clear();
        TargetDirList.Clear();
        DicSourcePrefab.Clear();
        DicTargetPrefab.Clear();
        ParseSourcePrefab();
        ParseTargetPrefab();
        ContrastPrefab();
    }
    public static void RestartContrastprefab()
    {
        ContrastPrefab();
    }
    private static void ParseSourcePrefab( string dirPath = null , List<RootDirectory> rootDirs = null )
    {
        if ( !IsFileMode)
        {
            if (dirPath == null)
                dirPath = SourceFilePath;
            bool isRoot = false;
            if (rootDirs == null)
            {
                isRoot = true;
                rootDirs = SourceDirList;
            }
            RootDirectory rootDirectory = new RootDirectory();
            rootDirectory.name = Path.GetFileNameWithoutExtension(dirPath);
            rootDirectory.path = isRoot ? "root" : m_regSourcePath.Match(dirPath).Value;
            var dirs = Directory.GetDirectories(dirPath);
            for (int i = 0; i < dirs.Length; i++)
            {
                ParseSourcePrefab(dirs[i], rootDirectory.rootDirs);
            }
            var prefabs = Directory.GetFiles(dirPath, "*.prefab", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < prefabs.Length; i++)
            {
                ChangeProgress("解析源预制中..." + prefabs[i]);
                Prefab prefab = new Prefab(true, prefabs[i]);
                rootDirectory.prefabs.Add(prefab);
                var prefabName = Path.GetFileNameWithoutExtension(prefabs[i]);
                var path = rootDirectory.path + "//" + prefabName;
                DicSourcePrefab.Add(path, prefab);
            }
            rootDirs.Add(rootDirectory);
        }
        else
        {
            //dirPath = SourceFilePath;
            for (int i = 0; i < SourceFiles.Length; i++)
            {
                dirPath = SourceFiles[i];
                RootDirectory rootDirectory = new RootDirectory();
                //rootDirectory.name = Path.GetFileNameWithoutExtension(dirPath);
                //rootDirectory.path = Path.GetFileNameWithoutExtension(dirPath);
                var fileName = Path.GetFileName(dirPath);
                rootDirectory.name = m_regPrefabName.Match(fileName).Value;
                rootDirectory.path = Path.GetDirectoryName(dirPath) + "\\" + rootDirectory.name;
                ChangeProgress("解析源预制中..." + dirPath);
                Prefab prefab = new Prefab(true, dirPath);
                rootDirectory.prefabs.Add(prefab);
                SourceDirList.Add(rootDirectory);
                DicSourcePrefab.Add(rootDirectory.name, prefab);
            }
        }
    }
    private static void ParseTargetPrefab( string dirPath = null, List<RootDirectory> rootDirs = null )
    {
        if (!IsFileMode)
        {
            if (dirPath == null)
                dirPath = TargetFilePath;
            bool isRoot = false;
            if (rootDirs == null)
            {
                isRoot = true;
                rootDirs = TargetDirList;
            }
            RootDirectory rootDirectory = new RootDirectory();
            rootDirectory.name = Path.GetFileNameWithoutExtension(dirPath);
            rootDirectory.path = isRoot ? "root" : m_regTargetPath.Match(dirPath).Value;
            var dirs = Directory.GetDirectories(dirPath);
            for (int i = 0; i < dirs.Length; i++)
            {
                ParseTargetPrefab(dirs[i], rootDirectory.rootDirs);
            }
            var prefabs = Directory.GetFiles(dirPath, "*.prefab", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < prefabs.Length; i++)
            {
                ChangeProgress("解析目标预制中..." + prefabs[i]);
                Prefab prefab = new Prefab(false, prefabs[i]);
                rootDirectory.prefabs.Add(prefab);
                var prefabName = Path.GetFileNameWithoutExtension(prefabs[i]);
                var path = rootDirectory.path + "//" + prefabName;
                DicTargetPrefab.Add(path, prefab);
            }
            rootDirs.Add(rootDirectory);
        }
        else
        {
            //dirPath = TargetFilePath;
            for (int i = 0; i < TargetFiles.Length; i++)
            {
                dirPath = TargetFiles[i];
                RootDirectory rootDirectory = new RootDirectory();
                var fileName = Path.GetFileName(dirPath);
                rootDirectory.name = m_regPrefabName.Match(fileName).Value;
                rootDirectory.path = Path.GetDirectoryName(dirPath) + "\\" + rootDirectory.name;
                ChangeProgress("解析目标预制中..." + dirPath);
                Prefab prefab = new Prefab(false, dirPath);
                rootDirectory.prefabs.Add(prefab);
                TargetDirList.Add(rootDirectory);
                DicTargetPrefab.Add(rootDirectory.name, prefab);
            }
        }
    }
    /// <summary>
    /// 比较预制
    /// </summary>
    private static void ContrastPrefab()
    {
        Dictionary<string,bool> publicPerfabs = new Dictionary<string, bool>();//公共预制 path,Prefab
        foreach (var item in DicSourcePrefab)
        {
            var key = item.Key;
            var sourcePrefab = item.Value;
            Prefab targetPrefab;
            if( DicTargetPrefab.TryGetValue(key, out targetPrefab ))
            {
                publicPerfabs.Add(key, true);
                ContrastOnePrefab(sourcePrefab, targetPrefab);
            }
        }
        foreach (var item in DicTargetPrefab)
        {
            var key = item.Key;
            if( !publicPerfabs.ContainsKey(key))
            {
                var targetPrefab = item.Value;
                targetPrefab.root.differentType = DifferentType.NodeName;
            }
        }
    }
    /// <summary>
    /// 比较1个预制
    /// </summary>
    /// <param name="sourcePrefab">源预制</param>
    /// <param name="targetPrefab">目标预制</param>
    private static void ContrastOnePrefab( Prefab sourcePrefab , Prefab targetPrefab )
    {
        if (sourcePrefab == null || targetPrefab == null) return;
        ContrastNode(sourcePrefab.root, targetPrefab.root);
        CheckAllNone(sourcePrefab.root);
        CheckAllNone(targetPrefab.root);
    }
    private static void ContrastNode( GameObject sourceGO , GameObject targetGO)
    {
        DifferentType different = DifferentType.Null;
        if(sourceGO != null)
        {
            //比较名字
            if (!sourceGO.name.Equals(targetGO.name))
            {
                different = SetFlag(different, DifferentType.NodeName, true);
            }
            //比较路径
            if (!sourceGO.path.Equals(targetGO.path))
            {
                different = SetFlag(different, DifferentType.NodePath, true);
            }
            //比较脚本
            if (CheckScript)
            {
                bool continueCheck = true;
                Dictionary<string, bool> publicScripts = new Dictionary<string, bool>();
                for (int i = 0; i < sourceGO.scripts.Count; i++)
                {
                    bool hasData = false;
                    var sname = sourceGO.scripts[i].name;
                    if (sname.Equals(MissName))
                    {
                        continueCheck = false;
                        different = SetFlag(different, DifferentType.ScriptMiss, true);
                        continue;
                    }
                    for (int k = 0; k < targetGO.scripts.Count; k++)
                    {
                        var tname = targetGO.scripts[k].name;
                        if (sname.Equals(tname))
                        {
                            if (!publicScripts.ContainsKey(sname))
                            {
                                publicScripts.Add(sname, true);
                            }
                            if (!sourceGO.scripts[i].Equals(targetGO.scripts[k]))
                            {
                                different = SetFlag(different, DifferentType.Script, true);
                            }
                            hasData = true;
                            break;
                        }
                    }
                    if (!hasData)
                    {
                        continueCheck = false;
                        different = SetFlag(different, DifferentType.Script, true);
                        break;
                    }
                }
                if (continueCheck)
                {
                    for (int k = 0; k < targetGO.scripts.Count; k++)
                    {
                        var tname = targetGO.scripts[k].name;
                        if (tname.Equals(MissName))
                        {
                            continueCheck = false;
                            different = SetFlag(different, DifferentType.ScriptMiss, true);
                            continue;
                        }
                        if (!publicScripts.ContainsKey(tname))
                        {
                            different = SetFlag(different, DifferentType.Script, true);
                            break;
                        }
                    }
                }
            }
            var smono = sourceGO.monoBehaviour;
            var tmono = targetGO.monoBehaviour;
            //比较材质
            if (CheckMaterial)
            {
                if (smono != null && smono.material != null)
                {
                    var sname = smono.material.name;
                    if (sname.Equals(MissName))
                    {
                        different = SetFlag(different, DifferentType.MatrialMiss, true);
                    }
                    else if (tmono == null || tmono.material == null)
                    {
                        different = SetFlag(different, DifferentType.Matrial, true);
                    }
                    else if (!sname.Equals(tmono.material.name))
                    {
                        different = SetFlag(different, DifferentType.Matrial, true);
                    }
                }
                else if (tmono != null && tmono.material != null)
                {
                    if (tmono.material.name.Equals(MissName))
                    {
                        different = SetFlag(different, DifferentType.MatrialMiss, true);
                    }
                    else
                    {
                        different = SetFlag(different, DifferentType.Matrial, true);
                    }
                }
            }
            //比较图片
            if (CheckImage)
            {
                if (smono != null && smono.sprite != null)
                {
                    var sname = smono.sprite.name;
                    if (sname.Equals(MissName))
                    {
                        different = SetFlag(different, DifferentType.SpriteMiss, true);
                    }
                    else if (tmono == null || tmono.sprite == null)
                    {
                        different = SetFlag(different, DifferentType.Sprite, true);
                    }
                    else if (!sname.Equals(tmono.sprite.name))
                    {
                        different = SetFlag(different, DifferentType.Sprite, true);
                    }
                }
                else if (tmono != null && tmono.sprite != null)
                {
                    if (tmono.sprite.name.Equals(MissName))
                    {
                        different = SetFlag(different, DifferentType.SpriteMiss, true);
                    }
                    else
                    {
                        different = SetFlag(different, DifferentType.Sprite, true);
                    }
                }
            }
            //比较字体
            if (CheckFont)
            {
                if (smono != null && smono.font != null)
                {
                    var sname = smono.font.name;
                    if (sname.Equals(MissName))
                    {
                        different = SetFlag(different, DifferentType.FontMiss, true);
                    }
                    else if (tmono == null || tmono.font == null)
                    {
                        different = SetFlag(different, DifferentType.Font, true);
                    }
                    /*else if (!sname.Equals(tmono.font.name))
                    {
                        different = SetFlag(different, DifferentType.Font, true);
                    }*/
                }
                else if (tmono != null && tmono.font != null)
                {
                    if (tmono.font.name.Equals(MissName))
                    {
                        different = SetFlag(different, DifferentType.FontMiss, true);
                    }
                    else
                    {
                        different = SetFlag(different, DifferentType.Font, true);
                    }
                }
            }
            //比较Active状态
            if (CheckActive)
            {
                if (sourceGO.isActive != targetGO.isActive)
                {
                    different = SetFlag(different, DifferentType.Active, true);
                }
            }
            if (different == DifferentType.Null)
            {
                different = DifferentType.None;
            }
            if (HasFlag(sourceGO.differentType, DifferentType.MutiMono))
            {
                sourceGO.differentType = different;
                sourceGO.differentType = SetFlag(sourceGO.differentType, DifferentType.MutiMono, true);
            }
            else
            {
                sourceGO.differentType = different;
            }
        }
        else
        {
            different = SetFlag(different, DifferentType.NodePath, true);
        }
        if (HasFlag(targetGO.differentType, DifferentType.MutiMono))
        {
            targetGO.differentType = different;
            targetGO.differentType = SetFlag(targetGO.differentType, DifferentType.MutiMono, true);
        }
        else
        {
            targetGO.differentType = different;
        }
        //清理掉补位的空节点
        /*for (int i = sourceGO.childs.Count -1; i >= 0 ; i-- )
        {
            if(IsPlaceNode(sourceGO.childs[i] ) )
            {
                sourceGO.childs.RemoveAt(i);
            }
        }
        for (int i = targetGO.childs.Count - 1; i >= 0; i--)
        {
            if (IsPlaceNode(targetGO.childs[i]))
            {
                targetGO.childs.RemoveAt(i);
            }
        }*/
        Dictionary<string,bool> publicNode = new Dictionary<string, bool>();
        if(sourceGO != null)
        {
            for (int i = 0; i < sourceGO.childs.Count; i++)
            {
                var _sourceGO = sourceGO.childs[i];
                if (IsPlaceNode(_sourceGO))//补位的节点
                    continue;
                GameObject _targetGO = null;
                int _targetGOIndex = i;
                //先通过FileId查找
                for (int k = 0; k < targetGO.childs.Count; k++)
                {
                    if (targetGO.childs[k].fileId == _sourceGO.fileId)
                    {
                        _targetGO = targetGO.childs[k];
                        _targetGOIndex = k;
                        break;
                    }
                }
                if(_targetGO == null)//fileId没找到再通过名字找（同一节点下如果有多个相同名字的节点，对比会出问题，但是暂无解决办法）
                {
                    for (int k = 0; k < targetGO.childs.Count; k++)
                    {
                        if (targetGO.childs[k].name == _sourceGO.name)
                        {
                            _targetGO = targetGO.childs[k];
                            _targetGOIndex = k;
                            break;
                        }
                    }
                }
                if (_targetGO != null)
                {
                    if (!publicNode.ContainsKey(_sourceGO.name))
                        publicNode.Add(_sourceGO.name, true);
                    /*if (_targetGOIndex == i)//与自己位置相同
                    {

                    }
                    else if( _targetGOIndex < i )//比自己靠前，表示对面缺节点，添加几个空节点
                    {
                        for (int j = _targetGOIndex; j < i; j++)
                        {
                            var newGO = new GameObject();
                            newGO.differentType = DifferentType.NodeName;
                            targetGO.childs.Insert(j, newGO);
                        }
                    }
                    else//比自己靠前，表示自己缺节点，添加几个空节点
                    {
                        for (int j = i; j < _targetGOIndex; j++)
                        {
                            var newGO = new GameObject();
                            newGO.differentType = DifferentType.NodeName;
                            sourceGO.childs.Insert(j, newGO);
                        }
                    }*/
                    ContrastNode(_sourceGO, _targetGO);
                }
                else
                {
                    //对面没找到节点
                    /*if( i < targetGO.childs.Count)
                    {
                        if(!IsPlaceNode( targetGO.childs[i] ) )
                        {
                            var newGO = new GameObject();
                            newGO.differentType = DifferentType.NodeName;
                            targetGO.childs.Insert(i, newGO);
                        }
                    }
                    else
                    {
                        for (int k = targetGO.childs.Count; k < i; k++)
                        {
                            var newGO = new GameObject();
                            newGO.differentType = DifferentType.NodeName;
                            targetGO.childs.Insert(k, newGO);
                        }
                    }*/
                    _sourceGO.differentType = DifferentType.NodeName;
                    targetGO.differentType = DifferentType.ChildNode;
                }
            }
        }
        for (int i = 0; i < targetGO.childs.Count; i++)
        {
            var _targetGO = targetGO.childs[i];
            if( !publicNode.ContainsKey(_targetGO.name))
            {
                _targetGO.differentType = DifferentType.NodeName;
                if(sourceGO != null)
                {
                    sourceGO.differentType = DifferentType.ChildNode;
                }
            }
        }
    }
    public static bool IsPlaceNode( GameObject go )
    {
        return string.IsNullOrEmpty(go.name) && string.IsNullOrEmpty(go.path);
    }
    private static void CheckAllNone(GameObject go )
    {
        if (go == null) return;
        /*if (go.differentType != DifferentType.None)
        {
            return;
        }*/
        bool isAllNone = true;
        for (int i = 0; i < go.childs.Count; i++)
        {
            CheckAllNone(go.childs[i]);
            if(go.childs[i].differentType != DifferentType.AllNone)
            {
                isAllNone = false;
            }
        }
        if(isAllNone && go.differentType == DifferentType.None)
        {
            go.differentType = DifferentType.AllNone;
        }
    }
    //组件中要屏蔽的字段
    private static string[] m_ignoreFileds = new string[]
    {
            "m_ObjectHideFlags",
            "m_PrefabParentObject",
            "m_PrefabInternal",
            "m_GameObject",
            "m_EditorHideFlags",
            "m_Name",
            "m_EditorClassIdentifier",
            "m_OnCullStateChanged",
            "m_Script",
    };
    public static bool IsIgnoreFiled(string name)
    {
        for (int i = 0; i < m_ignoreFileds.Length; i++)
        {
            if (m_ignoreFileds[i].Equals(name))
            {
                return true;
            }
        }
        return false;
    }
    public static void RefreshOnePrefab(Prefab sourcePrefab, Prefab targetPrefab)
    {
        if(sourcePrefab != null)
        {
            sourcePrefab.AfreshParsePrefab();
        }
        if (targetPrefab != null)
        {
            targetPrefab.AfreshParsePrefab();
        }
        ContrastOnePrefab(sourcePrefab, targetPrefab);
    }

    /// <summary>
    /// 上报进度
    /// </summary>
    public static void ChangeProgress(string tips)
    {
        if (!AsyncWorker.IsBusy) return;
        CurrStateTips = tips;
        m_currProgress++;
        AsyncWorker.ReportProgress(m_currProgress);
    }
    #endregion
}
