using System;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace FbsGen
{
    partial class Program
    {
        /* NOTE(llisperzhang):
         *  note that Unity can't use an external dll which target at framework "v4.0" or above,
         *  which means we can't compile our assembly with "CompilerVersion" set to "v4.0". 
         *  But by simply setting to use "v3.5" will not work either because our code make use of the feature "optional parameters" which is not allowed in "v3.5" framework
         *  So to make this work:
         *  1. use v4.0 compiler to compile 
         *  2. claim that we are linking with "mscorlib.dll" of version "v2.0" by specify its path in CompilerParameters.ReferenceAssemblies array
         *  3. add "/nostdlib" compiler options
         *
         *  reference:http://stackoverflow.com/questions/20018979/how-can-i-target-a-specific-language-version-using-codedom
         */
        static void Compile()
        {
            List<string> files = new List<string>();
            foreach (string file in Directory.EnumerateFiles("output"))
                files.Add(file);

            CompilerParameters cp = new CompilerParameters()
            {
                CompilerOptions = @"/optimize /nostdlib",
                GenerateInMemory = false,
                OutputAssembly = Path.Combine("output", packageName + ".dll"),
                TreatWarningsAsErrors = false,
                IncludeDebugInformation = true,
                WarningLevel = 3,
            };

            string flatbuffersLocation = AppDomain.CurrentDomain.Load("SharpFlatbuffers").Location;
            cp.ReferencedAssemblies.AddRange(new string[] {
                "System.dll",
                flatbuffersLocation,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET\Framework\v2.0.50727\mscorlib.dll"),
              });

            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v4.0");

            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);
            CompilerResults results = codeProvider.CompileAssemblyFromFile(cp, files.ToArray());
            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                    sb.Append(error.ToString()).Append('\n');
                throw new Exception(sb.ToString());
            }
            else
            {
                string dir = Path.GetDirectoryName(flatbuffersLocation);
                string lib = Path.GetFileName(flatbuffersLocation);
                string pdb = Path.GetFileNameWithoutExtension(flatbuffersLocation) + ".pdb";
                File.Copy(flatbuffersLocation, Path.Combine("output", lib));
                File.Copy(Path.Combine(dir, pdb), Path.Combine("output", pdb));
            }
        }
    }
}
