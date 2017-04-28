using System;
using PowerShell.Tasks;

internal class MarshalItemAction<T1, T2> : MarshalItem
{
    private readonly Action<T1, T2> _action;
    private readonly T1 _arg1;
    private readonly T2 _arg2;

    internal MarshalItemAction(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        this._action = action;
        this._arg1 = arg1;
        this._arg2 = arg2;
    }

    internal override void Invoke()
    {
        this._action(this._arg1, this._arg2);
    }
}