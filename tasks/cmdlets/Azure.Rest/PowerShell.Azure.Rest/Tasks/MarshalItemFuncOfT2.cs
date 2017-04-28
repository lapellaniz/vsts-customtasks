using System;

internal class MarshalItemFunc<T1, T2, TRet> : MarshalItemFuncBase<TRet>
{
    private readonly Func<T1, T2, TRet> _func;
    private readonly T1 _arg1;
    private readonly T2 _arg2;

    internal MarshalItemFunc(Func<T1, T2, TRet> func, T1 arg1, T2 arg2)
    {
        this._func = func;
        this._arg1 = arg1;
        this._arg2 = arg2;
    }

    internal override TRet InvokeFunc()
    {
        return this._func(this._arg1, this._arg2);
    }
}