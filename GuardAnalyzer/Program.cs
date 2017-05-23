using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SonDar.ParagonChallenge.GuardAnalyzer
{

    //      Preview - create ChangeModel and serialize to file
    //      Commit  - change all files by ChangeModel(denaid if Preview not called yet)
    //      Force   - Create Change model and commit in same times
    enum WorkMode
    {
        Preview,
        Commit,
        Force
    }

    static class WorkModeByString
    {
        public static WorkMode getMode(string mode)
        {
            switch (mode)
            {
                case "Preview": return WorkMode.Preview;
                case "Commit": return WorkMode.Commit;
                //case "Force": return WorkMode.Force; No testing -> no force mode!
                default: throw new Exception("Unknown Mode [args[1] = " + mode + "]");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Test data
            args = new string[] { "C:\\Development\\SonDar\\Paragon\\GuardAnalyzer", "Preview","Default","Example*.cs"};
            // arg0 : path to folder
            string pathToStartFolder = args[0];
            // arg1 : work mode
            WorkMode mode = WorkModeByString.getMode(args[1]);
            // arg2 : changeModel file (Optional)
            string path = Directory.GetCurrentDirectory() + "\\changes.txt";
            if (args.Length > 2 && !args[2].Equals("Default"))
            {
                path = args[2];
            }
            string wildcard = "*.cs";
            if (args.Length > 3)
            {
                wildcard = args[3];
            }
            //Start
            ChangeModel model = null;
            if (mode == WorkMode.Preview || mode == WorkMode.Force)
            {
                ArrayList files = (new FileListBuilder()).ParseDirectory(pathToStartFolder, wildcard);
                model = (new GuardAnalyzer()).Analyze(files);
                foreach (ChangeItem item in model.Items)
                {
                    Console.WriteLine(item);
                }
                if(mode != WorkMode.Force)
                {
                    model.Save(path);
                    return;
                }
            }
            if (mode == WorkMode.Commit || mode == WorkMode.Force)
            {
                if (mode != WorkMode.Force)
                {
                    model = ChangeModel.Load(path);
                }
                foreach (ChangeItem item in model.Items)
                {
                    //      commit changes ?? 
                }
            }

            Console.ReadKey();
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
                fileList.AddRange(this.ParseDirectory(directory, wildcard));
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

        public ChangeItem(string path, int line, string from, string to = null)
        {
            this.DomainPath = path;
            this.Line = line;
            this.From = from;
            this.To = to;
        } 

        public override string ToString()
        {
            return "File : " + this.DomainPath + ":" + this.Line + "\n\"" + this.From + "\" => \"" + this.To + "\"";
        }

    }

    class ChangeModel
    {
        public ArrayList Items { get; }

        public ChangeModel()
        {
            Items = new ArrayList();
        }

        public void AddItem(ChangeItem item)
        {
            this.Items.Add(item);
        }

        public void AddItems(ArrayList items)
        {
            this.Items.AddRange(items);
        }

        public static ChangeModel Load(string path)
        {
            string json = "";
            using (StreamReader sr = new StreamReader(path))
            {
                // TODO change to StringBuilder
                json += sr.ReadToEnd();
            }
            ChangeModel returnModel = new ChangeModel();
            dynamic model = JsonConvert.DeserializeObject(json);
            foreach (dynamic obj in model.Items)
            {
                returnModel.AddItem(new ChangeItem(
                (string)obj.DomainPath, 
                (int)obj.Line,
                (string)obj.From,
                (string)obj.To));
            }
            return returnModel;
        }

        public void Save(string path)
        {
            string json = JsonConvert.SerializeObject(this);
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                sw.WriteLine(json);
            }
        }

    }

    class GuardAnalyzer
    {
        // Call this.AnalyzeFile for all file in list and create general ChangeModel. 
        public ChangeModel Analyze(ArrayList files)
        {
            ChangeModel changes = new ChangeModel();
            foreach (String fileName in files)
            {
                changes.AddItems(this.AnalyzeFile(fileName));
            }
            return changes;
        }

        // parse file by recursive expression and return list of ChangeItems
        private ArrayList AnalyzeFile(string path)
        {
            ArrayList changeItems = new ArrayList();
            string[] lines = File.ReadAllLines(path);

            // TODO Add all Guard methods list. 
            string methodsList = "ArgumentNotNull,ArgumentNotNullOrEmpty,IsNotNull";
            string isMatch = "Guard.[" + methodsList + "]";
            
            // Detect lines
            for (int i=0; i< lines.Length; i++){
                if (Regex.IsMatch(lines[i], isMatch))
                {
                    changeItems.Add(new ChangeItem(path, i, lines[i].Trim()));
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
                // TODO change to StringBuilder
                item.To = "Guards." + method + "(" + param + ", nameof(" + param + "));";
            }

            return changeItems; 
        }

    }



}
