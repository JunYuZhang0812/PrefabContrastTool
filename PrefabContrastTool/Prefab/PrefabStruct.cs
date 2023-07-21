using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PrefabContrastTool.Prefab
{
    public class RootDirectory
    {
        public string name { get; set; }
        public string  path { get; set; }
        public List<Prefab> prefabs { get; set; } = new List<Prefab>();
        public List<RootDirectory> rootDirs { get; set; } = new List<RootDirectory>();
    }
    public class GameObject
    {
        public string fileFullPath { get; set; }
        public Prefab prefab { get; set; }
        public string name { get; set; }
        public string fileId { get; set; }
        public string  path { get; set; }
        public GameObject root { get; set; }
        public bool isActive { get; set; }
        public RectTransform rectTransform { get; set; }
        public MonoBehaviour monoBehaviour { get; set; }
        public List<Script> scripts { get; set; } = new List<Script>();
        public DifferentType differentType { get; set; }
        public List<Component> components { get; set; } = new List<Component>();
        public GameObject parent { get; set; }
        public List<GameObject> childs { get; set; } = new List<GameObject>();

        public GameObject( string name = "" , string path = "")
        {
            this.name = name;
            this.path = path;
        }
    }
    public class Member
    {
        public string key;
        public string value;
        public List<Dictionary<string,Member>> valueList = null;
        public bool Equals( Member other )
        {
            if (!key.Equals(other.key)) return false;
            if (!value.Equals(other.value)) return false;
            if (valueList == null && other.valueList == null) return true;
            if (valueList == null && other.valueList != null) return false;
            if (valueList != null && other.valueList == null) return false;
            if (valueList.Count != other.valueList.Count) return false;
            for (int i = 0; i < valueList.Count; i++)
            {
                foreach (var item in valueList[i])
                {
                    var _key = item.Key;
                    if( !other.valueList[i].ContainsKey(_key))
                    {
                        return false;
                    }
                    if( !item.Value.Equals(other.valueList[i][_key]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    public class Component
    {
        public string fileId { get; set; }
        public ComponentType type { get; set; }
        public GameObject gameObject { get; set; }
        public int index { get; set; }//在文件段落中的索引
        public string comStr { get; set; }//文件段落内容
        public string name { get; set; }
        public int order { get; set; }
        public Dictionary<string, Member> members { get; set; } = new Dictionary<string, Member>();
    }
    public class RectTransform: Component
    {
        public bool isTransform { get; set; }
        public Vector localPosition { get; set; }
        public Vector localScale { get; set; }
        public Vector localRotation { get; set; }
        public Vector anchoredPosition { get; set; }
        public Vector sizeDelta { get; set; }
        public Vector anchorMin { get; set; }
        public Vector anchorMax { get; set; }
        public Vector pivot { get; set; }
        public string parentId { get; set; } = "0";
        public List<string> childIds { get; set; } = new List<string>();

        public bool Equals(RectTransform other)
        {
            if (other == null) return false;
            if (!name.Equals(other.name))
            {
                return false;
            }
            if (isTransform)
            {
                var comPare = localScale.Equals(other.localScale) && localRotation.Equals(other.localRotation);
                if (comPare && !Logic.IgnorePos )
                {
                    comPare = localPosition.Equals(other.localPosition);
                }
                return comPare;
            }
            else
            {
                var comPare = localScale.Equals(other.localScale) && localRotation.Equals(other.localRotation) &&
                    sizeDelta.Equals(other.sizeDelta) && anchorMax.Equals(other.anchorMax) && anchorMin.Equals(other.anchorMin) && pivot.Equals(other.pivot);
                if (comPare && !Logic.IgnorePos)
                {
                    comPare = anchoredPosition.Equals(other.anchoredPosition);
                }
                return comPare;
            }
        }
    }
    public class MonoBehaviour : Component
    {
        public bool enabled { get; set; }
        public Material material { get; set; }
        public Sprite sprite { get; set; }
        public Font font { get; set; }
        public Script script { get; set; }
    }
    public class Vector
    {
        public string x { get; set; }
        public string y { get; set; }
        public string z { get; set; }
        public string w { get; set; }
        public Vector( string x = null,string y = null, string z = null, string w = null)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public Vector(string vector )
        {
            Regex regX = new Regex(@"x:\s(-?(\d|\.)+)");
            Regex regY = new Regex(@"y:\s(-?(\d|\.)+)");
            Regex regZ = new Regex(@"z:\s(-?(\d|\.)+)");
            Regex regW = new Regex(@"w:\s(-?(\d|\.)+)");
            if (regX.IsMatch(vector))
                this.x = regX.Match(vector).Groups[1].Value;
            if (regY.IsMatch(vector))
                this.y = regY.Match(vector).Groups[1].Value;
            if (regZ.IsMatch(vector))
                this.z = regZ.Match(vector).Groups[1].Value;
            if (regW.IsMatch(vector))
                this.w = regW.Match(vector).Groups[1].Value;
        }
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            if( x != null )
            {
                str.Append("x:");
                str.Append( x);
            }
            if (y != null)
            {
                str.Append("  y:");
                str.Append(y);
            }
            if ( z != null )
            {
                str.Append("  z:");
                str.Append(z);
            }
            if (w != null)
            {
                str.Append("  w:");
                str.Append(w);
            }
            return str.ToString();
        }
        public bool Equals(Vector other )
        {
            if (other == null) return false;
            if ( x == other.x && y == other.y && z == other.z && w == other.w )
                return true;
            return false;
        }
    }
    public class Sprite
    {
        public string guid { get; set; }
        public string fileId { get; set; }
        public string name { get; set; }
        public string raycastTarget { get; set; }
        public Color color { get; set; }

        //Parse用
        public bool isAtlas;
        public Dictionary<string, string> nameDic;//fileId,name
        public Sprite()
        {

        }
        public Sprite(string guid, string fileId)
        {
            this.guid = guid;
            this.fileId = fileId;
        }
        public bool Equals(Sprite other)
        {
            if (other == null) return false;
            if ( name.Equals(other.name) && raycastTarget.Equals(other.raycastTarget) && color.Equals(other.color) )
            {
                return true;
            }
            return false;
        }
    }
    public class Material
    {
        public string guid { get; set; }
        public string fileId { get; set; }
        public string name { get; set; }
        public Material()
        {

        }
        public Material( string guid , string fileId )
        {
            this.guid = guid;
            this.fileId = fileId;
        }
        public bool Equals(Material other)
        {
            if (other == null) return false;
            if (name.Equals(other.name))
            {
                return true;
            }
            return false;
        }
        public override string ToString()
        {
            return name;
        }
    }
    public class Font
    {
        public string guid { get; set; }
        public string fileId { get; set; }
        public string name { get; set; }
        public string text { get; set; }
        public string size { get; set; }
        public string aligment { get; set; }
        public string lineSpacing { get; set; }
        public string raycastTarget { get; set; }
        public Color color { get; set; }

        public Font()
        {

        }
        public Font(string guid, string fileId)
        {
            this.guid = guid;
            this.fileId = fileId;
        }
        public bool Equals(Font other)
        {
            if (other == null) return false;
            if (name.Equals(other.name) && size.Equals(other.size) && aligment.Equals(other.aligment) && lineSpacing.Equals(other.lineSpacing) 
                && raycastTarget.Equals(other.raycastTarget) && color.Equals(other.color) && text.Equals(other.text))
            {
                return true;
            }
            return false;
        }
    }
    public class Script
    {
        public ScriptType type { get; set; } = ScriptType.Other;
        public GameObject gameObject { get; set; }
        public string guid { get; set; }
        public string fileId { get; set; }
        public string name { get; set; }
        public Dictionary<string, Member> members = null;
        public Script()
        {

        }
        public Script(string guid, string fileId , string comStr = "" )
        {
            this.guid = guid;
            this.fileId = fileId;
        }
        public bool Equals( Script other )
        {
            if (other == null) return false;
            if (!name.Equals(other.name))
                return false;
            if (gameObject == null) return false;
            if (type == ScriptType.Other)
            {
                if (members == null && other.members == null) return true;
                if (members == null && other.members != null) return false;
                if (members != null && other.members == null) return false;
                if (members.Count != other.members.Count) return false;
                foreach (var item in members)
                {
                    var key = item.Key;
                    if (Logic.IsIgnoreFiled(key)) continue;
                    if( !other.members.ContainsKey(key))
                    {
                        return false;
                    }
                    if( !item.Value.Equals(other.members[key]))
                    {
                        return false;
                    }
                }
                return true;
            }
            if( type == ScriptType.RectTransform )
            {
                return gameObject.rectTransform.Equals(other.gameObject.rectTransform);
            }
            if( type == ScriptType.MonoBehaviour)
            {
                var mono = gameObject.monoBehaviour;
                var otherMono = other.gameObject.monoBehaviour;
                if (mono == null && otherMono == null)
                    return true;
                if (mono == null && otherMono != null)
                    return false;
                if (mono != null && otherMono == null)
                    return false;
                if (mono.material == null && otherMono.material != null)
                    return false;
                if (mono.material != null && otherMono.material == null)
                    return false;
                if (mono.material != null && !mono.material.Equals(otherMono.material))
                    return false;
                if (mono.sprite == null && otherMono.sprite != null)
                    return false;
                if (mono.sprite != null && otherMono.sprite == null)
                    return false;
                if (mono.sprite != null && !mono.sprite.Equals(otherMono.sprite))
                    return false;
                if (mono.font == null && otherMono.font != null)
                    return false;
                if (mono.font != null && otherMono.font == null)
                    return false;
                if (mono.font != null && !mono.font.Equals(otherMono.font))
                    return false;
            }
            return true;
        }
    }
    public class Color
    {
        public string r { get; set; }
        public string g { get; set; }
        public string b { get; set; }
        public string a { get; set; }
        public Color(string r = "0", string g = "0" ,string b = "0" , string a = "0" )
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public Color(string color)
        {
            Regex regR = new Regex(@"r:\s((\d|\.)+)");
            Regex regG = new Regex(@"g:\s((\d|\.)+)");
            Regex regB = new Regex(@"b:\s((\d|\.)+)");
            Regex regA = new Regex(@"a:\s((\d|\.)+)");
            if (regR.IsMatch(color))
                this.r = Math.Floor(float.Parse(regR.Match(color).Groups[1].Value) * 255).ToString();
            if (regG.IsMatch(color))
                this.g = Math.Floor(float.Parse(regG.Match(color).Groups[1].Value) * 255).ToString();
            if (regB.IsMatch(color))
                this.b = Math.Floor(float.Parse(regB.Match(color).Groups[1].Value) * 255).ToString();
            if (regA.IsMatch(color))
                this.a = Math.Floor(float.Parse(regA.Match(color).Groups[1].Value) * 255).ToString();
        }
        public bool Equals(Color other)
        {
            if (other == null) return false;
            if (this.r.Equals(other.r) && this.g.Equals(other.g) && this.b.Equals(other.b) && this.a.Equals(other.a))
                return true;
            return false;
        }
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append("r:");
            str.Append(r);
            str.Append("  g:");
            str.Append(g);
            str.Append("  b:");
            str.Append(b);
            str.Append("  a:");
            str.Append(a);
            return str.ToString();
        }
    }

    public class MetaInfo
    {
        public string guid;
        public string fileId;
        public string name;
        public string scriptClassName;
        public string path;
    }
}
