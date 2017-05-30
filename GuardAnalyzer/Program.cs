using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SonDar.Core.Utils;
using Newtonsoft.Json;
using System.Threading.Tasks.Dataflow;


// TODO MiltiThreading elements
//  1. FileListBuilder - search in subfolders in different threads
//  2. Analyze should call AnalyzeFile in different threads
//  3. Commit part of Main should call replacing and IO in different threads;

//  Check current time
//  Check time atfer multithreading implementation

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
    
    class Program
    {
        static void Main(string[] args)
        {
            // Test data
            args = new string[] { "C:\\Development\\SonDar\\Paragon\\GuardAnalyzer", "Preview","Default","Example997*.cs"};
            // arg0 : path to folder
            string pathToStartFolder = args[0];
            // arg1 : work mode
            WorkMode mode;
            if(!Enum.TryParse(args[1], out mode))
            {
                throw new Exception("Unknown Mode [args[1] = " + mode + "]");
            }
            // arg2 : changeModel file (Optional)
            string path = Directory.GetCurrentDirectory() + "\\changes.txt";
            if (args.Length > 2 && !args[2].Equals("Default"))
            {
                path = args[2];
            }
            // arg3 - wirdcard(Optional)
            string wildcard = "*.cs";
            if (args.Length > 3)
            {
                wildcard = args[3];
            }
            // Blocks list
            TransformBlock<string, ArrayList> fileListBuilder = new TransformBlock<string, ArrayList>
                                (domainFolderName => (new FileListBuilder()).ParseDirectory(domainFolderName, wildcard));
            TransformBlock<ArrayList, ChangeModel> guardAnalyzer = new TransformBlock<ArrayList, ChangeModel>
                                (fileList => (new GuardAnalyzer()).Prepare(fileList));
            TransformBlock<string, ChangeModel> loader = new TransformBlock<string, ChangeModel>
                                (fileName => ChangeModel.Load(fileName));
            TransformBlock<ChangeModel, ChangeModel> printer = new TransformBlock<ChangeModel, ChangeModel>
                                (model => model.Print());
            ActionBlock<ChangeModel> writer = new ActionBlock<ChangeModel>(tempModel => tempModel.Save(path));
            ActionBlock<ChangeModel> commiter = new ActionBlock<ChangeModel>(tempModel => (new GuardAnalyzer()).Commit(tempModel));

            //Start
            Logger.Log("Preview : ");
            if (mode == WorkMode.Preview || mode == WorkMode.Force)
            {
                fileListBuilder.LinkTo(guardAnalyzer);
                fileListBuilder.Completion.ContinueWith(task => guardAnalyzer.Complete());

                guardAnalyzer.LinkTo(printer);
                guardAnalyzer.Completion.ContinueWith(task => printer.Complete());
                if (mode != WorkMode.Force)
                {
                    printer.LinkTo(writer);
                    printer.Completion.ContinueWith(task => writer.Complete());
                    fileListBuilder.Post(pathToStartFolder);
                    fileListBuilder.Complete();
                    writer.Completion.Wait();
                }

            }
            if (mode == WorkMode.Commit || mode == WorkMode.Force)
            {
                if (mode != WorkMode.Force)
                {
                    loader.LinkTo(printer);
                    loader.Completion.ContinueWith(task => printer.Complete());
                }
                printer.LinkTo(commiter);
                printer.Completion.ContinueWith(task => commiter.Complete());
                if (mode != WorkMode.Force)
                {
                    loader.Post(path);
                    loader.Complete();
                } else
                {
                    fileListBuilder.Post(pathToStartFolder);
                    fileListBuilder.Complete();
                }
                commiter.Completion.Wait();
            }

            Console.ReadKey();
        }
    }

    class FileListBuilder
    {

        //Get path and find all by wildcard.
        public ArrayList ParseDirectory(string path, string wildcard = "*.cs")
        {
            ArrayList fileList = new ArrayList();
            IEnumerable<String> files = Directory.EnumerateFiles(path, wildcard);
            foreach (String file in files)
            {
                fileList.Add(file);
            }
            IEnumerable<String> directories = Directory.EnumerateDirectories(path);
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
        
        public ChangeModel Print() {
            foreach (ChangeItem item in Items)
            {
                Logger.Log(item.ToString());
                Logger.Log("");
            }
            // for TransformBlock usage
            return this;
        }

    }

    class GuardAnalyzer
    {
        // Call this.AnalyzeFile for all file in list and create general ChangeModel. 
        public ChangeModel Prepare(ArrayList files)
        {
            ChangeModel changes = new ChangeModel();
            foreach (String fileName in files)
            {
                changes.AddItems(this.PrepareFile(fileName));
            }
            return changes;
        }

        // parse file by recursive expression and return list of ChangeItems
        private ArrayList PrepareFile(string path)
        {
            ArrayList candidateItems = new ArrayList();
            string[] lines = File.ReadAllLines(path);

            // TODO Add all Guard methods list. 
            string methodsList = "ArgumentNotNullOrEmpty,ArgumentNotNull,IsNotNull,ArgumentNull";
            string isMatch = "Guard\\.[" + methodsList + "]";
            
            // Detect lines
            for (int i=0; i< lines.Length; i++){
                if (Regex.IsMatch(lines[i], isMatch))
                {
                    candidateItems.Add(new ChangeItem(path, i, lines[i].Trim()));
                }
            }
            // create line to change
            ArrayList finalResult = new ArrayList();
            string detectMethods = "(" + methodsList.Replace(',','|') + ")";
            string detectParams = "\\(\\) => .{1,}\\)";
            foreach (ChangeItem item in candidateItems)
            {
                string method = Regex.Match(item.From, detectMethods).Value;
                string rawParam = Regex.Match(item.From, detectParams).Value;
                if(rawParam != "")
                {
                    string[] temp = rawParam.Split(')')[1].Split(' ');
                    string param = temp[temp.Length - 1];
                    // TODO change to StringBuilder
                    item.To = "Guard." + method + "(" + param + ", nameof(" + param + "));";
                    finalResult.Add(item);
                    continue;
                }
                //Logger.Log("Ignore line : \"" + item.From + "\" because lambda param is \"" + rawParam + "\"");

            }
            return finalResult; 
        }

        public ChangeModel Commit(ChangeModel model)
        {
            ChangeModel wrongItem = new ChangeModel();
            foreach (ChangeItem item in model.Items)
            {
                Logger.Log("Try to change item : " + item.ToString());
                string[] lines = File.ReadAllLines(item.DomainPath);
                if (lines[item.Line].Contains(item.From))
                {
                    Logger.Log("OK");
                    lines[item.Line] = lines[item.Line].Replace(item.From, item.To);
                }
                else
                {
                    wrongItem.AddItem(item);
                    continue;
                }
                Logger.Log("");
                using (StreamWriter sw = new StreamWriter(item.DomainPath, false, System.Text.Encoding.Default))
                {
                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            Logger.Log("Done");
            return wrongItem;
        }

    }



}
