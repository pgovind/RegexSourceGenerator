using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics;
using System.IO;
//using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RegexSourceGenerator
{
    [Generator]
    public partial class RegexSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Debugger.Launch();
            string assemblyName = "test1";
            string pattern = "abcd";

            RegexCompilationInfo regexInfo = new RegexCompilationInfo(pattern, RegexOptions.None, assemblyName, "", false);
            Regex.CompileWithSourceGenerator(regexInfo);

            //-------------------------- For debugging/prototype
            string directory = @"C:\Users\prgovi\source\repos\RegexSourceGenerator_2";
            string factoryFile = Path.Join(directory, assemblyName + "Factory1.cs");
            System.IO.File.WriteAllText(factoryFile, regexInfo.regexRunnerFactoryCode.ToString());
            string runnerFile = Path.Join(directory, assemblyName + "Runner1.cs");
            System.IO.File.WriteAllText(runnerFile, regexInfo.regexRunnerCode.ToString());
            // Hack up the test1 class that refers to test1Factory
            StringBuilder regexClass = new StringBuilder();
            regexClass.Append(
$@"// test1
using System;
using System.Text.RegularExpressions;

public class {assemblyName} : Regex
{{
	public {assemblyName}()
	{{
		pattern = ""{pattern}"";

        roptions = RegexOptions.Compiled;
        factory = new {assemblyName + "Factory1"}();
        TimeSpan infiniteMatchTimeout = Regex.InfiniteMatchTimeout;
        internalMatchTimeout = infiniteMatchTimeout;
        capsize = 1;
        InitializeReferences();
    }}
}}
");
            string regexFile = Path.Join(directory, assemblyName + ".cs");
            System.IO.File.WriteAllText(regexFile, regexClass.ToString());

            // ---------------------------End debugging/prototype-----------

            // inject the created source into the users compilation
            context.AddSource(assemblyName + "Factory1", SourceText.From(regexInfo.regexRunnerFactoryCode.ToString(), Encoding.UTF8));
            context.AddSource(assemblyName + "Runner1", SourceText.From(regexInfo.regexRunnerCode.ToString(), Encoding.UTF8));
            context.AddSource(assemblyName, SourceText.From(regexClass.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}
