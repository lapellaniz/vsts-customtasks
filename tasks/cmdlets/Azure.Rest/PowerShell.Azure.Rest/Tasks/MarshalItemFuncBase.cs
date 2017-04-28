using System.Threading.Tasks;
using PowerShell.Tasks;

internal abstract class MarshalItemFuncBase<TRet> : MarshalItem
{
    private TRet _retVal;
    private readonly Task<TRet> _retValTask;

    protected MarshalItemFuncBase()
    {
        this._retValTask = new Task<TRet>(() => this._retVal);
    }

    sealed internal override void Invoke()
    {
        this._retVal = this.InvokeFunc();
        this._retValTask.Start();
    }

    internal TRet WaitForResult()
    {
        this._retValTask.Wait();
        return this._retValTask.Result;
    }

    internal abstract TRet InvokeFunc();
}