*SonDar for Paragon*<br />
<br />
GuardAnalyzer - Ñonsole application for optimization Gruad method calls<br />
<br />
Example: GuardAnalyzer.exe "C:\repository\test" Prepare Default "Example*.cs"<br />
<br />
args[0]: path to repository folder<br />
args[1] WorkMode <br />
    //          Preview - create ChangeModel and serialize to file<br />
    //          Commit  - change all repo files using ChangeModel(denaid if Preview not called yet)<br />
    //(disabled)Force   - Create ChangeModel and commit in same times<br />
args[2]: path for ChangeModel serianlization. <br />
    //          Use 'Default' for set default path - 'Directory.GetCurrentDirectory() + "\\changes.txt"'<br />
args[3]: wildcard for files search<br />
    //          Default - "*.cs"<br />