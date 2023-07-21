using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PrefabContrastTool
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main( params string[] arg )
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            PrefabContrastTool tool;
            if (arg.Length != 2)
            {
                tool = new PrefabContrastTool();
            }
            else
            {
                tool = new PrefabContrastTool(arg[0], arg[1]);
            }
            Application.Run(tool);
        }
    }
}
