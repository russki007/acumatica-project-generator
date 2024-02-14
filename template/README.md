# #PROJECT# Customization Project

## Overview
This project was bootstrapped from the project generator template repository, It offers a customization package scaffold for the Acumatica xRP Framework. Using this scaffold, you can create a standard customization project complete with common framework dependencies, build scripts, and folder layouts assets and some samples
to get your project up running.

## Getting Started
- Clone this repo
- Configure path to MYOB Advanced Web APP by editing [OpenVS](OpenVS.bat) `ACC_SITE_PATH=<path-to-site>`
- Ensure path to the devenv.exe is correct. Update this like
- Click on [OpenVS](OpenVS.bat) to open VS Solution
- Build and Run

## Project Structure
```
.
|-- Directory.Build.props
|-- OpenVS.cmd			  <--- ❗IMPORTANT: Open the solution file
|-- README.md
|-- artifacts			    <--- Contains build artifacts (customization package)
|-- assets			      <--- Acumatica Project Customizaton project assets
|   `-- Pages
|-- build.cake			  <--- Cake build file
|-- build.ps1
|-- database
|   `-- Schema.sql		<--- Be nice put your DDL code here
|-- src
|   `-- #PROJECT#     <--- Code goes here
|-- #PROJECT#.sln
|-- tests
|   `--  #PROJECT#.Tests
`-- tools
    `-- CsPack
```

## Prerequisites
* [.NET 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)
* Acumatica ERP Site 23R2
* [Nuget.exe](https://www.nuget.org/downloads)
* .NET 8.0 SDK


## Getting Started
- Clone this repo
- Configure path to MYOB Advanced Web APP by editing [OpenVS](OpenVS.bat) `ACC_SITE_PATH=<path-to-site>`
- Ensure path to the devenv.exe is correct.
- Click on [OpenVS](OpenVS.bat) to open VS Solution
- Build and Run

### Optimizations
Set the following keys in the `<appSettings>` section of the web.config to reduce start times
```
<add key="InstantiateAllCaches" value="False" />
<add key="DisableScheduleProcessor" value="True" />
<add key="CompilePages" value="False" />
```
### Build Customization Package using CLI
To produce customization package locally do the following
- Option 1 -  Using .NET Tool
  - Restore the tool NuGet packages `dotnet tool restore`
  - Execute the tool `dotnet cake` or `dotnet cake --PackageVersion=<version>`
- Option 2 - Using bootstrap script. See (Here)[https://cakebuild.net/docs/running-builds/runners/dotnet-tool#bootstrapping-for.net-tool] for more info.
  - Configure path to the MYOB Advance site by environment variable SITE_DIR. e.g. `SET SITE_DIR=<path-to-site>`
  - From the root directory for this customizations run `.\build.ps1` or `.\build.ps` --PackageVersion=<version>`

### Running Unit Tests
- Run unitest `dotnet cake --target=UnitTests` or `.\build.ps1 -Target UnitTests`

## Conventions
1. Graph Extensions - go to Extentions\PMProjectMaintExt
2. DACs & DAC Extentions -  DAC\MYPESetup, DAC\MYPEAPTran.  Note: NO need add namespace DAC segment to the namespace e.g `CompanyName.ProjectName.DAC`. Doing creates concise code and does not require unnecessary import of name space.
3. Both cache extensions (virtual or with backed table) and new DAC must have designated module prefix e.g MYxYourDAC or MYPExPMSetup, where MY prefix is your unqiue ISV code
4. Cache extensions names MUST end with the extended table e.g MYxPMSetup (PMSetup is the original table)
5. Graphs -> ProjectMaint.cs No need for a prefix, in are already in the namespace of the project. If you MUST use to letters of your nominated ISV code e.g [PE]ProjectMaint
6. User defined fields MUST have designated prefix in the name e.g `UsrMYxClaimNbrAttributeID`


### Examples

```
namespace MyProject   // <===== 2  Do not use DAC in the namespace
{
	public class MyCustomTable : IBqlTable {
	}

    public sealed class MYxPMSetup : PXCacheExtension<PMSetup> {

        public abstract class usrMYxClaimNbrAttributeID : PX.Data.BQL.BqlString.Field<usrMYxClaimNbrAttributeID> { }
        public string UsrMYxClaimNbrAttributeID { get; set; }
    }

```

# Customizations
## Screens

- [ ] Create new pages as per screen and report numbering conventions
- [ ] Modify an existing page using Layout Editor
- [ ] Modify the pages using in app - ASPX
- [ ] Modify publish paged ASPX page


## Database

### Create new tables

1. Create DDL for a new table and update database\schema.sql (Choose dialect either MSSQL\MYSQL.  Depends on your local RDBMS and if you plan on migrating the project to the main extension)
2. Run the script against a local database
3. Create file Sql_MYPEDACName.xml e.g (assets\Tables\MYPESetup.xml)
4. Alternatively, refresh the customization project to load the new table and extract definition XML.

### Data changes via SQL
 1. Do seed table(s) with the database. Create file `assets\Sql_AppUpdate.xml` Use VSQL dialect.

### Changes of the tables via C#

1. Use `CustomizationPlugin.UpdateDatabase()` to update the database


## Export Project

## CI/CD
- [ ] Project runs CI with automated build and test on each PR.
- [ ] Team City


# References

- [Customization Guide](https://help-2021r1.acumatica.com/(W(1))/Help?ScreenId=ShowWiki&pageid=316b14fa-f406-4788-993c-7b043b1c5bd9)
- [Custom Feature Switch](https://help-2021r1.acumatica.com/(W(1))/Help?ScreenId=ShowWiki&pageid=8285172e-d3b1-48d9-bcc1-5d20e39cc3f0)

