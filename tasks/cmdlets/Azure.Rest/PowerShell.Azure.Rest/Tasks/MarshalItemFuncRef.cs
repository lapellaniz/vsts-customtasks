using System.Threading.Tasks;
using PowerShell.Tasks;

internal class MarshalItemFuncRef<T1, T2, TRet, TRef1, TRef2> : MarshalItem
{
    internal delegate TRet FuncRef(T1 t1, T2 t2, ref TRef1 tref1, ref TRef2 tref2);

    private readonly Task<TRet> _retValTask;
    private readonly FuncRef _func;
    private readonly T1 _arg1;
    private readonly T2 _arg2;
    private TRef1 _arg3;
    private TRef2 _arg4;
    private TRet _retVal;

    internal MarshalItemFuncRef(FuncRef func, T1 arg1, T2 arg2, TRef1 arg3, TRef2 arg4)
    {
        this._func = func;
        this._arg1 = arg1;
        this._arg2 = arg2;
        this._arg3 = arg3;
        this._arg4 = arg4;
        this._retValTask = new Task<TRet>(() => this._retVal);
    }

    internal override void Invoke()
    {
        this._retVal = this._func(this._arg1, this._arg2, ref this._arg3, ref this._arg4);
        this._retValTask.Start();
    }

    // ReSharper disable RedundantAssignment
    internal TRet WaitForResult(ref TRef1 ref1, ref TRef2 ref2)
    {
        this._retValTask.Wait();
        ref1 = this._arg3;
        ref2 = this._arg4;
        return this._retValTask.Result;
    }
    // ReSharper restore RedundantAssignment
}