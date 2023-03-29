# Configuration
This document will give additional insight and information about each configuration value. For information about how the use these values in your API project see [Usage](#Usage)

## Table of contents

- [Overview](#Overview)
- [Configuration values](#Configuration-values)
  - [GenerateClientOnBuild](#GenerateClientOnBuild)
  - [UseExternalAssemblyContracts](#UseExternalAssemblyContracts)
  - [UsePartialClientClasses](#UsePartialClientClasses)
  - [UseInterfacesForClients](#UseInterfacesForClients)
  - [UseSeparateClientFiles](#UseSeparateClientFiles)
  - [CreateNugetPackageOnBuild](#CreateNugetPackageOnBuild)
  - [UseGitVersionInformation](#UseGitVersionInformation)
- [Usage](#Usage)

## Overview

| Configuration Value | Default Value |      Description |  Quicklink
|:----------|:-------------|:- |:--|
| `ACGT_GenerateClientOnBuild` | true | Generate client(s) on each build of the API | [More](#GenerateClientOnBuild)
| `ACGT_UseExternalAssemblyContracts` | true | Models used in Requests/Responses from external references will not be generated  | [More](#UseExternalAssemblyContracts)
| `ACGT_UsePartialClientClasses` | true | Client classes (and interfaces) will be marked partial  | [More](#UsePartialClientClasses)
| `ACGT_UseInterfacesForClients` | true | An interface will be generated for each client  |[More](#UseInterfacesForClients)
| `ACGT_UseSeparateClientFiles` | false | Each generated client (for each controller) will be placed in a separate file  |[More](#UseSeparateClientFiles)
| `ACGT_CreateNugetPackageOnBuild` | false | A NuGet package will be created on each build of the API  |[More](#CreateNugetPackageOnBuild)
| `ACGT_UseGitVersionInformation` |false | Current Git version information (if availanle) will be used for versioning the NuGet package |[More](#UseGitVersionInformation)

## Configuration values

### GenerateClientOnBuild
**Default value:** `true`

To allow for generation of the client(s) without any configuration, this is set to `true` on default. During development, this can be set to `false` to improve performance.


### UseExternalAssemblyContracts
**Default value:** `true`


### UsePartialClientClasses
**Default value:** `true`

When set to `true`, the `partial` keyword will be added to the client and interface classes as well as the methods. 
If the client file(s) are then used directly, the partial client class and methods are open for extension without changing the generated code file(s) directly.

> **Note**: When the client is packaged as a NuGet, the partial class(es) will be compiled into a single class and will not be open for extension.
### UseInterfacesForClients
**Default value:** `true`

When set to `true`, a interface will be created for each client. This interface can then for example be used for dependency injection.
### UseSeparateClientFiles
**Default value:** `false`

When set to `true`, each client will be placed into a separate `.cs` source file.
### CreateNugetPackageOnBuild
**Default value:** `false`

When set to `true`, a nuget package will be created each time the API is built. When developing and starting the API a lot, it is recommended to set this value to `false` or to avoid any generation of clients at all, you can set the [GenerateClientOnBuild](#GenerateClientOnBuild) flag to `false`.
### UseGitVersionInformation
**Default value:** `false`

When set to `true` and the project is inside a Git repository, the version of the project (if set) will be overriden to a version in the form of the following example value (1.0.1675382285-main): 

```csharp
long lastCommitTimeStamp;
string branchName;
string Version = $"1.0.{lastCommitTimeStamp}-{branchName}"
```

## Usage
There are several options to configure the generation of API clients. This is done using compiler-visible properties that can be accessed by the analyzer during compilation of the project. These properties are added in the project file (`.csproj`) of the API consuming the client generator. An example can be found below.
> 
**Example**

```csharp
<PropertyGroup>
	<ACGT_GenerateClientOnBuild>false</ACGT_GenerateClientOnBuild>
</PropertyGroup>
```
