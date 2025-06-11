using System.Text;
using System.Text.Json;
using PaperScript.Compiler.Papyrus;

namespace PaperScript.Compiler.Decompiler;

public class PapyrusDecompiler
{
    public PapyrusScript Decompile(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        using var reader = new BigEndianBinaryReader(stream);
        
        var script = new PapyrusScript();
        
        script.Magic = reader.ReadUInt32();
        script.MajorVersion = reader.ReadByte();
        script.MinorVersion = reader.ReadByte();
        script.GameId = reader.ReadUInt16();
        script.CompilationTime = reader.ReadUInt64();
        
        reader.ReadByte(); // Skip the random null that's here for some reason
        
        script.SourceFileName = reader.ReadUtf8StringU8LengthWithTrailingNull();
        script.Username = reader.ReadUtf8StringU8LengthWithTrailingNull();
        script.MachineName = reader.ReadUtf8StringU8LengthWithTrailingNull();

        
        // String Table
        var stringTableCount = reader.ReadUInt16LE();
        Console.WriteLine($"table count: {stringTableCount}");
        
        script.StringTable = new string[stringTableCount];

        for (var i = 0; i < stringTableCount; i++)
        {
            script.StringTable[i] = reader.ReadUtf8StringU8LengthWithTrailingNull();
        }
        
        // Debug Info
        reader.Rewind();
        script.DebugInfo = new();
        
        script.DebugInfo.HasDebugInfo = reader.ReadByte();
        if (script.DebugInfo.HasDebugInfo != 0)
        {
            script.DebugInfo.ModificationTime = reader.ReadUInt64();
            script.DebugInfo.FunctionCount = reader.ReadUInt16();

            script.DebugInfo.DebugFunctions = new DebugFunction[script.DebugInfo.FunctionCount];
            
            // Read Functions
            for (var i = 0; i < script.DebugInfo.FunctionCount; i++)
            {
                var debugFunction = new DebugFunction();
                debugFunction.ObjectNameIndex = reader.ReadUInt16();
                debugFunction.StateNameIndex = reader.ReadUInt16();
                debugFunction.FunctionNameIndex = reader.ReadUInt16();
                debugFunction.FunctionType = reader.ReadByte();
                debugFunction.InstructionCount = reader.ReadUInt16();
                
                debugFunction.LineNumbers = new UInt16[debugFunction.InstructionCount];
                for (var x = 0; x < debugFunction.InstructionCount; x++)
                {
                    debugFunction.LineNumbers[x] = reader.ReadUInt16();
                }

                script.DebugInfo.DebugFunctions[i] = debugFunction;
            }
        }
        
        // User Flags
        script.UserFlagCount = reader.ReadUInt16();
        script.UserFlags = new UserFlag[script.UserFlagCount];

        for (var i = 0; i < script.UserFlagCount; i++)
        {
            var flag = new UserFlag();
            flag.NameIndex = reader.ReadUInt16();
            flag.FlagIndex = reader.ReadByte();
            script.UserFlags[i] = flag;
        }
        
        // Objects
        script.ObjectCount = reader.ReadUInt16();
        script.Objects = new PapyrusObject[script.ObjectCount];
        
        for (var i = 0; i < script.ObjectCount; i++)
        {
            var obj = new PapyrusObject();
            obj.NameIndex = reader.ReadUInt16();
            obj.Size = reader.ReadUInt32();
            obj.Data = new ObjectData();
            
            obj.Data.ParentClassName = reader.ReadUInt16();
            obj.Data.DocString = reader.ReadUInt16();
            obj.Data.UserFlags = reader.ReadUInt32();
            obj.Data.AutoStateName = reader.ReadUInt16();
            obj.Data.NumVariables = reader.ReadUInt16();
            obj.Data.Variables = new Variable[obj.Data.NumVariables];

            // Variables
            for (int v = 0; v < obj.Data.NumVariables; v++)
            {
                var variable = new Variable();
                
                variable.Name = reader.ReadUInt16();
                variable.TypeName = reader.ReadUInt16();
                variable.UserFlags = reader.ReadUInt32();

                variable.Data = new VariableData();
                variable.Data.Type = reader.ReadByte();

                switch (variable.Data.Type)
                {
                    case 0: // NULL
                        break;
                    case 1: // IDENTIFIER
                    case 2: // STRING
                        variable.Data.StringData = reader.ReadUInt16();
                        break;
                    case 3: // INTEGER
                        variable.Data.IntegerData = reader.ReadUInt32();
                        break;
                    case 4: // FLOAT
                        variable.Data.FloatData = reader.ReadSingle();
                        break;
                    case 5: // BOOL
                        variable.Data.BoolData = reader.ReadByte();
                        break;
                        
                }
                
                obj.Data.Variables[v] = variable;
            }
            
            // Properties
            obj.Data.NumProperties = reader.ReadUInt16();
            obj.Data.Properties = new Property[obj.Data.NumProperties];

            for (int p = 0; p < obj.Data.NumProperties; p++)
            {
                var prop = new Property();
                
                prop.Name = reader.ReadUInt16();
                prop.Type = reader.ReadUInt16();
                prop.Dosctring = reader.ReadUInt16();
                prop.UserFlags = reader.ReadUInt32();
                prop.Flags = reader.ReadByte();
                prop.AutoVarName = reader.ReadUInt16();

                prop.HadReadHandler = (prop.Flags & 5) == 1;
                prop.HasWriteHandler = (prop.Flags & 6) == 1;

                if (prop.HadReadHandler) prop.ReadHandler = ParseNextFunction(reader);
                if (prop.HasWriteHandler) prop.WriteHandler = ParseNextFunction(reader);
                
                obj.Data.Properties[p] = prop;
            }
            
            // States
            obj.Data.NumStates = reader.ReadUInt16();
            obj.Data.States = new State[obj.Data.NumStates];

            for (int s = 0; s < obj.Data.NumStates; s++)
            {
                var state = new State();
                
                state.Name = reader.ReadUInt16();
                state.NumFunctions = reader.ReadUInt16();
                state.Functions = new NamedFunction[state.NumFunctions];

                for (int f = 0; f < state.NumFunctions; f++)
                {
                    var function = new NamedFunction();
                    
                    function.FunctionName = reader.ReadUInt16();
                    function.Function = ParseNextFunction(reader);
                    
                    state.Functions[f] = function;
                }
                
                obj.Data.States[s] = state;
            }
            
            script.Objects[i] = obj;
        }

        return script;
    }

    private Function ParseNextFunction(BigEndianBinaryReader reader)
    {
        var function = new Function();
        
        function.ReturnType = reader.ReadUInt16();
        function.DocString = reader.ReadUInt16();
        function.UserFlags = reader.ReadUInt32();
        function.Flags = reader.ReadByte();
        function.NumParams = reader.ReadUInt16();
        function.Params = new VariableType[function.NumParams];

        for (var x = 0; x < function.NumParams; x++)
        {
            var param = new VariableType();
            
            param.Name = reader.ReadUInt16();
            param.Type = reader.ReadUInt16();
            
            function.Params[x] = param;
        }
        
        function.NumLocals = reader.ReadUInt16();
        function.Locals = new VariableType[function.NumLocals];

        for (var x = 0; x < function.NumLocals; x++)
        {
            var local = new VariableType();
            
            local.Name = reader.ReadUInt16();
            local.Type = reader.ReadUInt16();
            
            function.Locals[x] = local;
        }
        
        function.NumInstructions = reader.ReadUInt16();
        function.Instructions = new Instruction[function.NumInstructions];
        
        for (var x = 0; x < function.NumInstructions; x++)
        {
            
        }

        return function;
    }
}