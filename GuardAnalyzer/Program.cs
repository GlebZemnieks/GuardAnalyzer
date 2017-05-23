using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace SonDar.ParagonChallenge.GuardAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string test1 = "\n" +
                "something another\n" +
                "Guard.ArgumentNotNull(() => storage);\n" +
                "Guard.IsNotNull(() => endpoint);\n" +
                "Guard.ArgumentNotNullOrEmpty(() => catalog);\n" +
                "something another";
            foreach(ChangeItem item in GuardAnalyzer.AnalyzeFile(test1))
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
            // arg1 : path to folder
            // arg2 : work mode
            //      Preview - create ChangeModel and serialize to file
            //      Commit  - change all files by ChangeModel(denaid if Preview not called yet)
            //      Force   - Create Change model and commit in same times

            // if(Preview || Force)
            //      call FileListBuiled
            //      call GuardAnalyzer 
            //      serialize model to file
            // if(Commit || Force)
            //      parse ChangeModel
            //      commit changes
        }
    }

    class FileListBuilder
    {

        //Get path and find all by "*.cs" wildcard.
        public ArrayList ParseDirectory(string path, string wildcard = "*.cs")
        {
            ArrayList fileList = new ArrayList();
            IEnumerable files = Directory.EnumerateFiles(path, wildcard);
            foreach (String file in files)
            {
                fileList.Add(file);
            }
            IEnumerable directories = Directory.EnumerateDirectories(path);
            foreach (String directory in directories)
            {
                fileList.AddRange(this.ParseDirectory(directory));
            }
            return fileList;
        }

    }

    class ChangeItem
    {
        public string DomainPath { get; }
        public int Line { get; }
        public string From { get; }
        public string To { get; set; }

        public ChangeItem(string path, int line, string from)
        {
            this.DomainPath = path;
            this.Line = line;
            this.From = from;
        } 

        static ChangeItem Load(string fileLine)
        {
            // TODO Parse line and return saved Change object
            return null;
        }

        string Save()
        {
            // TODO Serialize current object and return string 
            return null;
        }

        public override string ToString()
        {
            return "\"" + this.From + "\" => \"" + this.To + "\"";
        }

    }

    class ChangeModel
    {
        ArrayList Items { get; }

        public ChangeModel()
        {
            Items = new ArrayList();
        }

        public void AddItems(ArrayList items)
        {
            this.Items.AddRange(items);
        }

        static ChangeModel Load(string path)
        {
            // TODO Parse add items from file by path
            return null;
        }

        void Save(string path)
        {
            // TODO Save all items to file by path
        }

    }

    class GuardAnalyzer
    {
        // Call this.AnalyzeFile for all file in list and create general ChangeModel. 
        ChangeModel Analyze(ArrayList files)
        {
            ChangeModel changes = new ChangeModel();
            foreach (String fileName in files)
            {
                changes.AddItems(GuardAnalyzer.AnalyzeFile(fileName));
            }
            return changes;
        }

        // parse file by recursive expression and return list of ChangeItems
        public static ArrayList AnalyzeFile(string path)
        {
            ArrayList changeItems = new ArrayList();
            // TODO Add all Guard methods list. 
            string methodsList = "ArgumentNotNull,ArgumentNotNullOrEmpty,IsNotNull";
            string isMatch = "Guard.[" + methodsList + "]";
            string[] lines = path.Split('\n');

            // Detect lines
            for (int i=0; i< lines.Length; i++){
                if (Regex.IsMatch(lines[i], isMatch))
                {
                    changeItems.Add(new ChangeItem(path, i, lines[i]));
                }
            }
            // create line to change
            string detectMethods = "(" + methodsList.Replace(',','|') + ")";
            string detectParams = " => .{1,}\\)";
            foreach (ChangeItem item in changeItems)
            {
                string method = Regex.Match(item.From, detectMethods).Value;
                string[] temp = Regex.Match(item.From, detectParams).Value.Split(')')[0].Split(' ');
                string param = temp[temp.Length - 1];
                //Guard.ArgumentNotNull(storage, nameof(storage));
                item.To = "Guards." + method + "(" + param + ", nameof(" + param + "));";
            }

            return changeItems; 
        }

    }



}
