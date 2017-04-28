using System.Threading.Tasks;
using PowerShell.Tasks;

internal class MarshalItemFuncOut<T1, T2, T3, TRet, TOut> : MarshalItem
{
    private readonly FuncOut _func;
    private readonly T1 _arg1;
    private readonly T2 _arg2;
    private readonly T3 _arg3;

    internal delegate TRet FuncOut(T1 t1, T2 t2, T3 t3, out TOut tout);

    private TRet _retVal;
    private TOut _outVal;
    private readonly Task<TRet> _retValTask;

    internal MarshalItemFuncOut(FuncOut func, T1 arg1, T2 arg2, T3 arg3)
    {
        this._func = func;
        this._arg1 = arg1;
        this._arg2 = arg2;
        this._arg3 = arg3;
        this._retValTask = new Task<TRet>(() => this._retVal);
    }

    internal override void Invoke()
    {
        this._retVal = this._func(this._arg1, this._arg2, this._arg3, out this._outVal);
        this._retValTask.Start();
    }

    internal TRet WaitForResult(out TOut val)
    {
        this._retValTask.Wait();
        val = this._outVal;
        return this._retValTask.Result;
    }
}