# Api Client Generator 

## Description

> **Note**: note
> Note note.

## Table of contents

- [Api Client Generator](#Api-Client-Generator)
  - [Table of contents](#Table-of-contents)
  - [Description](#Description)
  - [Installation](#Installation)
    - [Prerequisites](#Prerequisites)
    - [NuGet Package](#NuGet-Package)
    - [Cloning the repo](#Cloning-the-repo)
  - [Configuration](#Configuration)
    - [List of configuration values](#List-of-configuration-values)
  - [Known problems](#Known-problems)
  - [Contributions](#Contributions)


## Installation

### Prerequisites

**Automatic NuGet package generation:**
Make sure `dotnet.exe` can be used as an environment variable on your system. This file is usually located in `C:\Program Files\dotnet\dotnet.exe`.

### NuGet Package

### Cloning the repo

## Configuration
There are several ways to configure the generation of API clients. This is done using compiler-visible properties that can be accessed by the analyzer during compilation of the project. These properties are added in the project file (`.csproj`) of the API consuming the client generator. An example can be found below.
> **Note**: When no configuration properties are added, default values will be used, which can be found in [the list of configuration values](#List-of-configuration-values) for each respective value.
> 
**Example**

```csharp
<PropertyGroup>
	<ClientGenerator_UsePartialClasses>true</ClientGenerator_UsePartialClasses>
</PropertyGroup>
```

### List of configuration values 
Below you can find a list of the currently available configuration values. You can find more information about each configuration value in the [configuration document](docs/configuration.md).

| Configuration Value | Default Value |      Description
|:----------|:-------------|:-
| `ApiClientGenerator_UsePartialClientClasses` | true | Description 
| `ApiClientGenerator_UseInterfacesForClients` | true | Description 
| `ApiClientGenerator_UseSeparateClientFiles` | false | Description 
| `ApiClientGenerator_CreateNugetPackageOnBuild` | false | Description 
| `ApiClientGenerator_UseGitVersionInformation` |true | Description 

## Known problems

## Contributions
