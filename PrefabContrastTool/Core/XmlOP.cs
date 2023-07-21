using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

public class XmlOp
{
    private static XmlOp _instance;
    public static XmlOp Instance
    {
        get
        {
            if (_instance == null)
                _instance = new XmlOp();
            return _instance;
        }
    }
    /// <summary>
    /// 文件路径
    /// </summary>
    private string m_filePath = string.Empty;
    private XElement xml;
    public XmlOp()
    {
        InitFile();
        GetRootXml();
    }
    private void InitFile()
    {
        m_filePath = Logic.APPDataPath + "\\预制对比工具数据配置.xml";
        if (!File.Exists(m_filePath))
        {
            CreateFile(m_filePath);
            InitXmlFile();
        }
    }
    private void InitXmlFile()
    {
        StreamWriter streamWriter = new StreamWriter(m_filePath);
        streamWriter.WriteLine("<xml>");
        streamWriter.WriteLine("</xml>");
        streamWriter.Flush();
        streamWriter.Close();
    }
    /// <summary>
    /// 获取xml根节点
    /// </summary>
    /// <returns></returns>
    public XElement GetRootXml()
    {
        if (xml == null)
            xml = XElement.Load(m_filePath);
        return xml;
    }
    /// <summary>
    /// 读取所有要素
    /// </summary>
    public List<XElement> GetXmlAllElement(XElement xElement = null)
    {
        List<XElement> xmlList = new List<XElement>();
        if (xElement == null)
            xElement = xml;
        var eleList = xElement.Elements();
        foreach (var item in eleList)
        {
            xmlList.Add(item);
        }
        return xmlList;
    }
    /// <summary>
    /// 获取要素
    /// </summary>
    public XElement GetXmlElement(string name, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        return xElement.Element(name);
    }
    public string GetXmlElementValue(XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        return xElement.Value;
    }
    public string GetXmlElementValue(string name, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        XElement element = GetXmlElement(name, xElement);
        if (element == null) return "";
        return element.Value;
    }
    public void SetXmlElementValue(string name, string value, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        XElement element = GetXmlElement(name, xElement);
        if (element == null)
        {
            MessageBox.Show("没有这个元素!" + name);
            return;
        }
        element.Value = value;
        Save();
    }
    public void SetOrAddElement(string name, string value, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        XElement element = GetXmlElement(name, xElement);
        if (element == null)
        {
            xElement.Add(new XElement(name, value));
            Save();
            return;
        }
        element.Value = value;
        Save();
    }
    public void SetOrAddElement(XElement xe, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        XElement element = GetXmlElement(xe.Name.ToString(), xElement);
        if (element == null)
        {
            xElement.Add(xe);
            Save();
            return;
        }
        ReplaceElement(xe, element);
    }
    private void ReplaceElement(XElement source, XElement target)
    {
        var attrList = target.Attributes();
        foreach (var item in attrList)
        {
            var name = item.Name;
            var attr = source.Attribute(name);
            if (attr != null)
                target.SetAttributeValue(name, attr.Value);
            else
                target.Attribute(name).Remove();
        }
        Save();
    }
    public void DeleteXmlElement(string name, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        XElement element = GetXmlElement(name, xElement);
        if (element == null) return;
        element.Remove();
        Save();
    }
    /// <summary>
    /// 获取属性
    /// </summary>
    public string GetXmlAttributValue(string xName, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        var attr = xElement.Attribute(xName);
        if (attr != null)
            return attr.Value;
        return "";
    }
    public void SetXmlAttributValue(string name, string value, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        xElement.SetAttributeValue(name, value);
        Save();
    }
    public void SetOrAddXmlAttributValue(string name, string value, XElement xElement = null)
    {
        if (xElement == null)
            xElement = xml;
        var attr = xElement.Attribute(name);
        if (attr == null)
        {
            XAttribute xAttribute = new XAttribute(name, value);
            xElement.Add(xAttribute);
            Save();
            return;
        }
        xElement.SetAttributeValue(name, value);
        Save();
    }
    /// <summary>
    /// 添加要素
    /// </summary>
    public XElement AddXmlElement(XElement xElement, XElement parent = null)
    {
        if (parent == null)
            parent = xml;
        parent.Add(xElement);
        Save();
        return xElement;
    }
    /// <summary>
    /// 添加属性
    /// </summary>
    public void AddXmlAttribute(XAttribute xAttribute, XElement parent = null)
    {
        if (parent == null)
            parent = xml;
        parent.Add(xAttribute);
        Save();
    }
    public void Save()
    {
        xml.Save(m_filePath);
    }
    /// <summary>
    /// 设置文件路径
    /// </summary>
    public void SetFilePath(string strFilePath)
    {
        m_filePath = strFilePath;
    }
    /// <summary>
    /// 读取文件路径
    /// </summary>
    public string GetFilePtah()
    {
        return m_filePath;
    }
    /// <summary>
    /// 创建文件
    /// </summary>
    public void CreateFile(string strFilePath)
    {
        SetFilePath(strFilePath);
        CreateFile();
    }
    /// <summary>
    /// 创建文件
    /// </summary>
    public void CreateFile()
    {
        if (m_filePath.Length == 0) return;
        if (File.Exists(m_filePath)) return;
        FileStream fileStream = File.Create(m_filePath);
        if (fileStream != null)
            fileStream.Close();
    }
}

