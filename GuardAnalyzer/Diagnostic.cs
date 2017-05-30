using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SonDar.Core.Utils;
using Newtonsoft.Json;


// TODO MiltiThreading elements
//  1. FileListBuilder - search in subfolders in different threads
//  2. Analyze should call AnalyzeFile in different threads
//  3. Commit part of Main should call replacing and IO in different threads;

//  Check current time
//  Check time atfer multithreading implementation

namespace SonDar.ParagonChallenge.GuardAnalyzer
{

    public static class Diagnostic
    {
        public static void ParseDirectory()
        {
            System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
            swatch.Start();
            ArrayList files = (new FileListBuilder()).ParseDirectory(
                "C:\\Development\\SonDar\\Paragon\\GuardAnalyzer", "*.cs");
            swatch.Stop();
            System.Diagnostics.Stopwatch swatch2 = new System.Diagnostics.Stopwatch();
            swatch2.Start();
            ArrayList files2 = (new FileListBuilder()).ParseDirectoryAsync(
                "C:\\Development\\SonDar\\Paragon\\GuardAnalyzer", "*.cs");
            swatch2.Stop();
            Console.WriteLine(swatch.Elapsed);
            Console.WriteLine(swatch2.Elapsed);
            Console.ReadKey();
        }

        public static void GenerateFileTree(string path)
        {
            for(int i=0; i< 1000; i++)
            {
                using (StreamWriter sw = new StreamWriter(path + "\\Example" + i + ".cs", false, System.Text.Encoding.Default))
                {
                    sw.Write(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guards;

namespace SonDar.ParagonChallenge.Example
{
    class Example" + i + @"
    {
        public void main()
        {
            //Example 
            string test1 = null;
            string test2 = null;

                    Guard.ArgumentNotNull(() => test1);
                    Guard.ArgumentNotNull(() => test2);
                    Guard.ArgumentNotNull(test1, nameof(test1));

                }
            }
        }");
                }
            }
        }
    }
        

}
