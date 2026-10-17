using JinianNet.JNTemplate;
using System;
using System.IO;

namespace Infrastructure.Helper
{
    public class JnHelper
    {
        /// <summary>
        /// Read JN template
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="tplName"></param>
        /// <returns></returns>
        public static ITemplate ReadTemplate(string dirPath, string tplName)
        {
            string path = Environment.CurrentDirectory;
            string fullName = Path.Combine(path, "wwwroot", dirPath, tplName);
            if (File.Exists(fullName))
            {
                return Engine.LoadTemplate(fullName);
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Path not found: {fullName}");
            return null;
        }
    }
}

