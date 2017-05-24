*SonDar for Paragon*<br />
<br />
GuardAnalyzer - Console application for optimization Gruad method calls<br />
<br />
Example: <br />
GuardAnalyzer.exe "C:\repository\test" Prepare Default "Example*.cs"<br />
//Check changes list <br />
GuardAnalyzer.exe "C:\repository\test" Commit<br />
<br />
<br />
args[0]: path to repository folder<br />
<br />
args[1] WorkMode <br />
    //          Preview - create ChangeModel and serialize to file<br />
    //          Commit  - change all repo files using ChangeModel(denaid if Preview not called yet)<br />
    //          Force   - Create ChangeModel and commit in same times<br />
<br />
(Optional)<br />
args[2]: path for ChangeModel serialization. <br />
    //          Use 'Default' for set default path - 'Directory.GetCurrentDirectory() + "\\changes.txt"'<br />
<br />
(Optional)<br />
args[3]: wildcard for files search<br />
    //          Default - "*.cs"<br />
