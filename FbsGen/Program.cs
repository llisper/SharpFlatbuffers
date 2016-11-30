using System;
using System.IO;

namespace FbsGen
{
    // 1. use flatc to generate cs file
    // 2. compile cs file to an assembly
    // parameters: fbs_root 
    partial class Program
    {
        public static string fbsRoot = Directory.GetCurrentDirectory();
        public static string packageName = "FlatbuffersMessage";

        static int Main(string[] args)
        {
            if (!ParseArgs(args))
                return 1;

            try
            {
                ClearOutput();
                GenerateCSharpCode();
                Compile();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }

        static bool ParseArgs(string[] args)
        {
            if (args.Length == 1 && args[0] == "--help")
            {
                Console.WriteLine("{0} --fbs-root [FBS_ROOT]", AppDomain.CurrentDomain.FriendlyName);
                return false;
            }

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "--fbs-root" && i < args.Length - 1)
                {
                    fbsRoot = args[++i];
                }
                else if (args[i] == "--package" && i < args.Length - 1)
                {
                    packageName = args[++i];
                }
            }

            if (!Directory.Exists(fbsRoot))
                throw new DirectoryNotFoundException(fbsRoot + " is not found");
            Directory.SetCurrentDirectory(fbsRoot);
            return true;
        }

        static void ClearOutput()
        {
            if (Directory.Exists("output"))
                Directory.Delete("output", true);
        }
    }
}
