ScriptName Demo extends ObjectReference

Actor Property PlayerREF Auto
Float Property PollInterval = 1.0 Auto
Bool lastCombatState = false

Function HelloWorld(Bool demo = false)
    Debug.Notification("Hello World")
    Int dong = 1
        
    If dong == 1
        Debug.Notification("Dong is 1")
    Else
        Debug.Notification("Dong is not 1")
    WhileDemo()
    EndIf
EndFunction


Bool Function OtherHello(String demo)
    Return false
EndFunction


Function WhileDemo()
    While a > b
        Debug.MessageBox("Hello")
        Utility.Wait(1)
    EndWhile
EndFunction


Function RangeDemo()
    Int[] slots = new Int[9]
    slots[0] = 1
    
    Int slotsIndex = 0
    While slotsIndex < slots.Length
        Int slot = slots[slotsIndex]
        UseSlot(slot)
        slotsIndex += 1
    EndWhile
EndFunction

Event OnEquipped(Actor actor)
    Debug.MessageBox(actor.GetName())
    Bool
    isDone = actor.IsDone
EndEvent
