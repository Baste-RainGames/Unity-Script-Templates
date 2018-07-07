# Unity-Script-Templates
Custom templates for creating new scripts in Unity.

This project adds some entires under Create->Script Template for creating ScriptableObject and plain class files.

![example](https://github.com/Baste-RainGames/Unity-Script-Templates/blob/master/README_IMG/ExampleImage.png)

# Usage

This works exactly like "Create->C# Script", except that the templates are placed in the Assets folder (in EditorDefaultResources) rather than in the Unity install folder. You can edit the templates however you like. To add a new template, see UnityScriptTemplates/CustomScriptTemplates.cs. It should be pretty easy to edit, as I've commented the code extensively. You should be able to add your own templates as needed, it only requites a couple of lines of code and a new template file.  You can also just use the code as an example to create your own tooling. Go wild.

The templates works exactly like the ones found in Unity's install folder. There's some very light find-and-replace done to the text content in the file before it's used. You can check the behaviour by looking up ProjectWindowUtil.CreateScriptAssetFromTemplate from Unity's C# reference, but here's an overview:
#NOTRIM# is replaced with empty space. To quote Unity, " #NOTRIM# is a special marker that is used to mark the end of a line where we want to leave whitespace. prevent editors auto-stripping it by accident."
#NAME# is replaced with the name you entered for the file
#SCRIPTNAME# is replaced with the name you entered for the file, with spaces removed
#SCRIPTNAME_LOWER# is strange:
- if the script name starts with an upper case letter, it is replaced with the script name in all lower cases.
- if the script name starts with a lower case letter, it is prefixed with "my", and the old first letter is now upper case. So "thisIsAnExample" is turned into "myThisIsAnExample". No clue what this behaviour is good for.

# Notes
Public domain lincense, so do whatever.
I've been wanting this for a while, and after yet another person asked about it, I went and looked through the public C# source. Turns out it's not so hard to do this! Thanks to Unity for making the source available.

If you've got problems with the code, just create an issue. If you have more general questions, you can reach me at baste@rain-games.com, or as "Baste" at the Unity forums. 