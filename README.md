# Acumatica Customization Project Generator

This is a command line tool, written in PowerShell , to automatically generate Acumatica Extensiibly Project. The tool will generate all required files VS Solution files for the set of features requested. It will also add example code for any features.


## Command line

Running `csMake -help` will give a list of the available command line parameters


```
Usage:  csMake [options] <Project Name>

Options:
 -h|--help      Show command line help and exit.
 -output        Location to place the generated output. The default is the current directory.
 -templates     Comma delimited list of source code templates to include. e.g. Features,Webhooks,Plugin
                    Features - Adds custom features - https://help-2023r1.acumatica.com/Help?ScreenId=ShowWiki&pageid=8285172e-d3b1-48d9-bcc1-5d20e39cc3f0
                    Plugin - Adds Customization Plugin
                    * - Adds all code options
```

 The above arguments can be shortened as much as to be unambiguous (e.g. -p for project, -o for output, etc.).

Build Options | Description
--------------| -----------
Read the generated Readme.md file in the project director for instruction how to build the project, and publish customization project