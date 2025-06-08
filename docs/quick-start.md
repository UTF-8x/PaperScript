# PaperScript Quick Start Guide

## Requirements
You will need the following:

 - A working copy of a CreationEngine Game (only Skyrim is officially supported at the moment)
 - A working copy of CreationKit. Make sure to run it before starting so it can extract all the script sources
 - A copy of the Papyrus Compiler. This should be installed into your main Skyrim folder along with Creation Kit
 - The PaperScript binary
 - A text editor

## Creating a New Project

Create a folder somewhere and run `paperscript init`. This will generate a default `project.yaml` file.

If your copy of Skyrim is installed correctly, it should also auto-detect paths to your script folders and your
Papyrus compiler.

Afterwards, open the `project.yaml` file, fill in details like your project name and version and make sure the
paths are all correct.

## Writing Some Code

Create a new file in the `src/` directory with a `.pps` extension and write some code. See [docs/syntax](syntax.md)
for a full reference.

Here is a minimal example:

```scala
script HelloWorldScript : Quest {
    auto property PlayerREF: Actor
    auto property Gold001: MiscItem

    event OnInit() {
        RegisterForSingleUpdate()
    }
    
    event OnUpdate() {
        DoThings()
    }
    
    def DoThings() {
        PlayerREF.GiveItem(Gold001, 100)
        Debug.MessageBox("Have some gold...")
    }
}
```

## Compile Your Code

For the first run, it's a good idea to run a one-off build to make sure everything works
as it should. To do so, run `paperscript build`.

If everything works, you can run PaperScript in *watch* mode. It will recompile your code
every time you change something so you don't have to do it by hand. To use PaperScript in
watch mode, simply run `paperscript watch`

## Done!

And that's it. Happy coding!
