# PaperScript

**PaperScript** is a modern alternative to [Papyrus](https://ck.uesp.net/wiki/Category:Papyrus) that transpiles into valid Papyrus code.

> 💬 **Contributions welcome!**  
> Please open issues and submit PRs with feature requests or suggestions. My goal is to make the **v1** release as complete and robust as possible before moving on to a smarter **v2** rewrite.
>
> I'm not great at writing good documentation so that's one area where PRs are especially appreciated.

---

## [Quick Start Guide](docs/quick-start.md)

---

## Why PaperScript?

Because **Papyrus is annoying**. It's not the worst language out there, but it's missing a lot of quality-of-life features and tends to be overly verbose.

For example, Papyrus doesn't support `foreach` or even `for` loops—just `while`. So something as simple as iterating over an array is tedious:

```papyrus
; Papyrus "foreach"
Int[] numbers = new Int[10]
Int numberIndex = 0
While numberIndex < numbers.Length
    Int number = numbers[numberIndex]
    DoSomething(number)
    numberIndex += 1
EndWhile
```

With **PaperScript**, you can write:

```paperscript
numbers: Int[] = new Int[10]

range number in numbers {
    DoSomething(number)
}
```

PaperScript transpiles that into the `while` loop for you—clean, readable, and less error-prone.
More quality of life features, such as a `switch` are coming...

---

## What Makes It Easy to Use?

From day one, PaperScript was designed to **stay out of your way**.

The transpiler doesn't just convert PaperScript to Papyrus—it also **compiles** it for you. When running in *Project Mode*, you configure the paths to your game's script source directory and the Papyrus compiler just once. After that, PaperScript takes care of the rest.

Whenever you update a `.pps` file, PaperScript automatically:

1. Transpiles it to Papyrus (`.psc`)
2. Copies it to your game’s script source folder
3. Runs the Papyrus compiler to produce the final `.pex` file

This means you can keep your PaperScript sources in a clean, organized Git repo—anywhere you like. PaperScript handles deployment and compilation behind the scenes, so you can focus on writing code instead of managing build steps.

In fact, it’s **more convenient than writing Papyrus directly.**

---

## How It Works

The transpiler is built using **C#** and **ANTLR4**. This setup is great for rapid prototyping and early development, but the current version behaves more like a "Google Translate for code" than a full compiler.

This makes some features harder to implement and error reporting less helpful. For example, a simple typo like `autso` instead of `auto` might result in a confusing error:

```
syntax error at line 1, column 0: mismatched input 'property' expecting ':'
```

In **v2**, the plan is to implement a proper recursive descent parser that builds an **AST** (Abstract Syntax Tree) and generates Papyrus from that. It'll be more robust, easier to extend, and allow for better error messages.

---

## Components

This project includes:

- 🧠 **Transpiler Core** — The ANTLR-based transpilation logic (C#)
- 🛠️ **CLI Tool** — A command-line interface for compiling scripts or full projects
- 🧩 **VSCode Extension** — Syntax highlighting and basic tooling support

---

## Transpiler

The transpiler converts `.pps` (PaperScript) source files into `.psc` (Papyrus).

---

## CLI Usage

There are two main modes: **single file** and **project**.

### 🗃️ Project Mode (recommended)

Compile all scripts in a folder by configuring `project.yaml` and simply running:

```
paperscript build
```

`project.yaml` example:

```yaml
projectName: "Example"
projectVersion: "v1.0.0"
scriptFolderPath: "C:/Games/Skyrim/Data/Scripts/Source"
scriptOutputPath: "C:/Games/Skyrim/Data/Scripts"
papyrusFlagsPath: "C:/Games/Skyrim/Data/Scripts/Source/TESV_Flags.flg"
papyrusCompilerPath: "C:/Games/Skyrim/Papyrus Compiler/PapyrusCompiler.exe"
```

You can also run PaperScript in watch mode where it will automatically recompile any changed files.

```
paperscript watch
```

### 📄 Single File Mode

Transpile one `.pps` file at a time:

```
paperscript transpile MyScript.pps <-o MyScript.psc>
```

---

## Syntax

See [docs/syntax](docs/syntax.md)

---

## Known Issues

See [docs/known-issues](docs/known-issues.md)

---

## Roadmap

- [x] Basic transpiler functionality
- [x] Basic preprocessor directives (`#define`, `#if`)
- [x] Skyrim Papyrus support
- [ ] `switch` statement
- [ ] ternary operator (`x = condition? a : b`)
- [ ] Fallout 4 Papyrus support
- [ ] Starfield Papyrus support\*
- [ ] TES6 Papyrus support\*\*

## V2 Roadmap
- [ ] Better error handling
- [ ] Null checker
- [ ] Git-based dependency management
- [ ] Advanced preprocessor support (`#include`, `#error`, etc.)

## Future Plans
- [ ] Direct Papyrus compiler -> skip the transpiling and compile directly to `pex`

\* I’m not really into Starfield, but if someone feels like gifting me a copy, I’ll gladly add support.  
\** When it finally releases… in 2055.

---