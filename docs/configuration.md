# Configuration
This document will give additional insight and information about each configuration value.

## Table of contents

- [Overview](#Overview)
- [Configuration values](#Configuration-values)
  - [GenerateClientOnBuild](#GenerateClientOnBuild)
  - [UsePartialClientClasses](#UsePartialClientClasses)
  - [UseInterfacesForClients](#UseInterfacesForClients)
  - [UseSeparateClientFiles](#UseSeparateClientFiles)
  - [CreateNugetPackageOnBuild](#CreateNugetPackageOnBuild)
  - [UseGitVersionInformation](#UseGitVersionInformation)

## Overview

| Configuration Value | Default Value |      Description |  Quicklink
|:----------|:-------------|:- |:--|
| `ApiClientGenerator_GenerateClientOnBuild` | true | Description | [More](#GenerateClientOnBuild)
| `ApiClientGenerator_UsePartialClientClasses` | true | Description | [More](#UsePartialClientClasses)
| `ApiClientGenerator_UseInterfacesForClients` | true | Description |[More](#UseInterfacesForClients)
| `ApiClientGenerator_UseSeparateClientFiles` | false | Description  |[More](#UseSeparateClientFiles)
| `ApiClientGenerator_CreateNugetPackageOnBuild` | false | Description  |[More](#CreateNugetPackageOnBuild)
| `ApiClientGenerator_UseGitVersionInformation` |true | Description |[More](#UseGitVersionInformation)

## Configuration values

### GenerateClientOnBuild
**Default value:** `true`

To allow for generation of the client(s) without any configuration, this is set to `true` on default. During development, this can be set to `false` to improve performance.
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

When set to `true`, a nuget package will be created each time the API is built. When developing and starting the API a lot, it is recommended to set this value to `false` or to avoid any generation of clients at all, you can set the [GenerateClientOnBuild](###GenerateClientOnBuild) flag to `false`.
### UseGitVersionInformation
