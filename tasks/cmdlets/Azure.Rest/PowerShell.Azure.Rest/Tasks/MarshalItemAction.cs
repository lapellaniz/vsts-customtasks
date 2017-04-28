using System;
using PowerShell.Tasks;

internal class MarshalItemAction<T> : MarshalItem
{
    private readonly Action<T> _action;
    private readonly T _arg1;

    internal MarshalItemAction(Action<T> action, T arg1)
    {
        this._action = action;
        this._arg1 = arg1;
    }

    internal override void Invoke()
    {
        this._action(this._arg1);
    }
}