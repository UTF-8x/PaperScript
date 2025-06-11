namespace PaperScript.Compiler.Decompiler;

public class Ops
{
    private const int I = sizeof(uint);
    private const int F = sizeof(float);
    private const int B = sizeof(bool);
    private const int S = sizeof(ushort);
    private const int N = S;
    private const int U = I;
    private const int Q = S;
    private const int A = 0; // ANY?
    private const int L = 0;

    public Dictionary<byte, int> OpTable = new Dictionary<byte, int>
    {
        { 0x00, 0 },        //     nop,
        { 0x01, S+I+I },    // iadd
        { 0x02, S+F+F },    // fadd
        { 0x03, S+I+I },    // isub
        { 0x04, S+F+F },    // fsub
        { 0x05, S+I+I },    // imul
        { 0x06, S+F+F },    // fmul
        { 0x07, S+I+I },    // idiv
        { 0x08, S+F+F },    // fdiv
        { 0x09, S+I+I },    // imod
        { 0x0A, S+A },      // not
        { 0x0B, S+I },      // ineg
        { 0x0C, S+F },      // fneg
        { 0x0D, S+A },      // assign
        { 0x0E, S+A },      // cast
        { 0x0F, S+A+A },    // cmp_eq
        { 0x10, S+A+A },    //cmp_lt
        { 0x11, S+A+A },    //cmp_le
        { 0x12, S+A+A },    //cmp_gt
        { 0x13, S+A+A },    //cmp_ge
        { 0x14, L },        //jmp
        { 0x15, A+L },      //jmpt
        { 0x16, A+L },      //jmpf
        { 0x17, N+S+S },    //callmethod
        { 0x18, N+S },      //callparent
        { 0x19, N+N+S },    //callstatic
        { 0x1A, A },        //return
        { 0x1B, S+Q+Q },    //strcat
        { 0x1C, N+S+S },    //propget
        { 0x1D, N+S+A },    //propset
        { 0x1E, S+U },      //array_create
        { 0x1F, S+S },      //array_length
        { 0x20, S+S+I },    //array_getelement
        { 0x21, S+I+A },    //array_setelement
        { 0x22, S+S+I+I },  //array_findelement
        { 0x23, S+S+I+I }   //array_rfindelement
    };

}