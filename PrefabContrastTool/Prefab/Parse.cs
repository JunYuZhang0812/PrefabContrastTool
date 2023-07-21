using System.IO;
using System.Text.RegularExpressions;

namespace PrefabContrastTool.Prefab
{
    abstract class Parse<T>
    {
        protected Regex regexGuid = new Regex(@"(?<=guid: )\w+");
        abstract public T ParseFile(string path);
        abstract protected bool ParseLineString(string line);

        virtual protected void LoadFile(string path)
        {
            StreamReader fileStream = new StreamReader(path);
            if (fileStream != null)
            {
                try
                {
                    int row = 0;
                    string line;
                    while ((line = fileStream.ReadLine()) != null)
                    {
                        if (ParseLineString(line))
                            break;
                        row++;
                    }
                }
                finally
                {
                    fileStream.Close();
                }
            }
        }

    }
}
