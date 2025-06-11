namespace PaperScript.Compiler.Decompiler;

public class ObjectData
{
    public UInt16 ParentClassName { get; set; }
    
    public UInt16 DocString { get; set; }
    
    public UInt32 UserFlags { get; set; }
    
    public UInt16 AutoStateName { get; set; }
    
    public UInt16 NumVariables { get; set; }
    
    public Variable[] Variables { get; set; }
    
    public UInt16 NumProperties { get; set; }
    
    public Property[] Properties { get; set; }
    
    public UInt16 NumStates { get; set; }
    
    public State[] States { get; set; }
}