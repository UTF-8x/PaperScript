# PaperScript Syntax

## Script Block

A script must be container in a `script` block. This is equivalent to
`ScriptName ...` in Papyrus.

```scala
script Demo : ObjectReference {}
```

## Auto Properties

An auto property begins with `auto property` and  then follows the general 
syntax of PaperScript variable definitions. Auto properties can have
an optional default value.

```scala
auto property PlayerREF: Actor
auto property Interval: Float = 1.0
```

## Variables

Variable definitions begin with a name, followed by a type and finally an optional value.
Since Papyrus doesn't support type inference, all types must be explicitly declared.

```
someBool: Bool = true
someInt: Int = 1

# Uninitialized variables are None by default
someNull: Actor 
```

## Functions

A function starts with `def`, has a name, optional arguments and an 
optional return type. Functions that return `void` do not need to
specify a return type.

```scala
def Demo() {
}

def DemoWithArgs(player: Actor) {
}

def DemoWithReturn() -> Bool {
    return true
}
```

## Events

Events look the same as functions but never have a return type.

```scala
event OnEquipped(actor: Actor) {
}
```

## If/Else

The If/Else block syntax is virtually identical to Papyrus. As with Papyrus, 
parentheses are optional and only used to specify precedence.

```scala
if something == 1 {
    DoSomething()
} else {
    DoSomethingElse()
}
```

## While

While blocks are identical to Papyrus but with braces. As with if blocks, 
parentheses are optional.

```scala
while true {
}
```

## Range

The range block is a new addition in PaperScript. It implements a feature
sorely missing from Papyrus - the `foreach` loop. The `range` block can only
be used with arrays at the moment.

```scala
range actor in actors {
    DoSomething(actor)
}
```

This will transpile into a Papyrus while block like this:

```
Int actorsIndex = 0
While actorsIndex < actors.Length
    Actor actor = actors[actorsIndex]
    DoSomething(actor)
    actorsIndex += 1
EndWhile
```