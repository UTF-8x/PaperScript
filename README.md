PaperScript is a modern alternative to [Papyrus](https://ck.uesp.net/wiki/Category:Papyrus)
that transpiles into valid Papyrus.

## Components

This project consists of a few components. The ANTLR-based transpiler 
library, a compiler CLI and a VSCode extension for syntax highlighting.

## Transpiler

The transpiler is built with C# and ANTLR4. It takes PaperScript (`.pps`)
source and turns it into valid Papyrus code (`.psc`).

## CLI

The CLI allows you to interact with the transpiler. It currently has
two modes.

### Individual Files
You can transpile individual files from PaperScript into Papyrus:

`paperscript <inputFile> [-o outputFile]`

### Projects

In project mode, the CLI will not only transpile but also compile
all available source files. It requires a `project.yaml` config file.

To run in project mode, simply start the CLI with no arguments: `paperscript`

### Project File
```yaml
# Name of your project
projectName: "Example"

# Version of your project
projectVersion: "v1.0.0"

# Path to your skyrim script sources folder
scriptFolderPath: "C:/Games/Skyrim/Data/Scripts/Source"

# Path to your skyrim compiled scripts folder
scriptOutputPath: "C:/Games/Skyrim/Data/Scripts"

# Path to your TESV_Papyrus_Flags.flg file
papyrusFlagsPath: "C:/Games/Skyrim/Data/Scripts/Source/TESV_Flags.flg"

# Path to the papyrus compiler
papyrusCompilerPath: "C:/Games/Skyrim/Papyrus Compiler/PapyrusCompiler.exe"
```

## Syntax
See [docs/syntax](docs/syntax.md)

## Known Issues
See [docs/known-issuer](docs/known-issues.md)