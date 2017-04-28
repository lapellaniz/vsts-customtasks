using System;

internal class MarshalItemFunc<T1, TRet> : MarshalItemFuncBase<TRet>
{
    private readonly Func<T1, TRet> _func;
    private readonly T1 _arg1;

    internal MarshalItemFunc(Func<T1, TRet> func, T1 arg1)
    {
        this._func = func;
        this._arg1 = arg1;
    }

    internal override TRet InvokeFunc()
    {
        return this._func(this._arg1);
    }
}

internal class MarshalItemFunc<TRet> : MarshalItemFuncBase<TRet>
{
    private readonly Func<TRet> _func;

    internal MarshalItemFunc(Func<TRet> func)
    {
        this._func = func;
    }

    internal override TRet InvokeFunc()
    {
        return this._func();
    }
}