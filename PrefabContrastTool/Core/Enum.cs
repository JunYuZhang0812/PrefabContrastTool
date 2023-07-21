using System.ComponentModel;

//选择文件模式
public enum SelectFileModeEnum
{
    [Description("选择文件")]
    SelectFiles,  //选择文件模式
    [Description("选择文件夹")]
    SelectFolder,//选择文件夹模式
}
//差异类型
public enum DifferentType
{
    Null = 0,
    [Description("无差异")]
    None = 1,
    [Description("节点名字有差异")]
    NodeName = 1 << 1,
    [Description("节点路径有差异")]
    NodePath = 1 << 2,
    [Description("脚本有差异")]
    Script = 1 << 3,
    [Description("材质有差异")]
    Matrial = 1 << 4,
    [Description("图片有差异")]
    Sprite = 1 << 5,
    [Description("字体有差异")]
    Font = 1 << 6,
    [Description("字体Miss")]
    FontMiss = 1 << 7,
    [Description("激活状态有差异")]
    Active = 1 << 8,
    [Description("图片Miss")]
    SpriteMiss = 1 << 9,
    [Description("材质Miss")]
    MatrialMiss = 1 << 10,
    [Description("脚本Miss")]
    ScriptMiss = 1 << 11,
    [Description("有多个Monobehaviour")]
    MutiMono = 1 << 12,
    [Description("完全无差异（包括子物体")]
    AllNone = 1 << 13,
    PlaceHolder = 1<< 14,
    ChildNode = 1<<15,//子节点有差异
}
public enum ComponentType
{
    Prefab,
    GameObject,
    Transform,
    RectTransform,
    MonoBehaviour,
    CanvasRenderer,
    Other,
}

public enum ScriptType
{
    GameObject,
    RectTransform,
    MonoBehaviour,
    Other,
}
