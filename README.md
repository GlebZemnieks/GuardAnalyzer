*SonDar for Paragon*

GuardAnalyzer - Ñonsole application for optimization Gruad method calls

Example: GuardAnalyzer.exe "C:\repository\test" Prepare Default "Example*.cs"

args[0]: path to repository folder
args[1] WorkMode 
    //          Preview - create ChangeModel and serialize to file
    //          Commit  - change all repo files using ChangeModel(denaid if Preview not called yet)
    //(disabled)Force   - Create ChangeModel and commit in same times
args[2]: path for ChangeModel serianlization. 
    //          Use 'Default' for set default path - 'Directory.GetCurrentDirectory() + "\\changes.txt"'
args[3]: wildcard for files search
    //          Default - "*.cs"