# Api Client Generator 

## Description

> **Note**: Be aware that this is not a fully released tool yet.

## Table of contents
- [Description](#Description)
- [Installation](#Installation)
  - [Prerequisites](#Prerequisites)
  - [NuGet Package](#NuGet-Package)
  - [Cloning the repo](#Cloning-the-repo)
- [Configuration](#Configuration)
  - [List of configuration values](#List-of-configuration-values)
- [NuGet Package Versioning](#NuGet-Package-Versioning)
- [Known problems](#Known-problems)
- [Contributions](#Contributions)


## Installation


### Prerequisites

**Automatic NuGet package generation:**
Make sure `dotnet.exe` can be used as an environment variable on your system. This file is usually located in `C:\Program Files\dotnet\dotnet.exe`.

### NuGet Package
The current stable release of the tool can be found on NuGet [here](https://www.nuget.org/packages/ApiClientGenerator).

### Cloning the repo
Alternatively, you can build the source yourself and find the NuGet package in the output directory.
## Configuration
There are several options to configure the generation of API clients. This is done using compiler-visible properties that can be accessed by the analyzer during compilation of the project. These properties are added in the project file (`.csproj`) of the API consuming the client generator. An example can be found below.
> **Note**: When no configuration properties are added, default values will be used, which can be found in [the list of configuration values](#List-of-configuration-values) for each respective value.
> 
**Example**

```csharp
<PropertyGroup>
	<ACGT_GenerateClientOnBuild>false</ACGT_GenerateClientOnBuild>
</PropertyGroup>
```

### List of configuration values 
Below you can find a list of the currently available configuration values. You can find more information about each configuration value in the [configuration document](docs/configuration.md).

| Configuration Value | Default Value |      Description
|:----------|:-------------|:-
| `ACGT_GenerateClientOnBuild` | true | Generate client(s) on each build of the API
| `ACGT_UseExternalAssemblyContracts` | true | Models used in Requests/Responses from external references will not be generated 
| `ACGT_UsePartialClientClasses` | true | Client classes (and interfaces) will be marked partial 
| `ACGT_UseInterfacesForClients` | true | An interface will be generated for each client 
| `ACGT_UseSeparateClientFiles` | false | Each generated client (for each controller) will be placed in a separate file 
| `ACGT_CreateNugetPackageOnBuild` | false | A NuGet package will be created on each build of the API  
| `ACGT_UseGitVersionInformation` |false | Current Git version information (if availanle) will be used for versioning the NuGet package 

## NuGet Package Versioning

## Known problems

## Contributions
Any contributions are welcome. If you want to contribute to the project, please open an issue or pull request. The discussion tab can also be used for anything related to the project.