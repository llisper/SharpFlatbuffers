using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace FbsGen
{
    partial class Program
    {
        static string FlatcOptions()
        {
            StringBuilder sb = new StringBuilder(" -n --gen-onefile -o output");

            List<string> paths = new List<string>();
            foreach (string path in Directory.EnumerateDirectories(".", "*", SearchOption.AllDirectories))
            {
                sb.Append(" -I ").Append(path);
                paths.Add(path);
            }

            foreach (string path in paths)
            {
                foreach (string file in Directory.EnumerateFiles(path))
                    sb.Append(' ').Append(file);
            }
            return sb.ToString();
        }

        static void GenerateCSharpCode()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @".\flatc.exe";
            start.Arguments = FlatcOptions();
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
            start.WorkingDirectory = Directory.GetCurrentDirectory();
            using (Process process = Process.Start(start))
            {
                process.WaitForExit();
                if (0 != process.ExitCode)
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        throw new Exception(result);
                    }
                }
            }
        }
    }
}
