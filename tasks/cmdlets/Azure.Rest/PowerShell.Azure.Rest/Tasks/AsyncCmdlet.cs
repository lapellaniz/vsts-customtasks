using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PowerShell.Tasks
{
    /// <summary>
    /// Base class for async-enabled cmdlets
    /// </summary>
    public abstract class AsyncCmdlet : Cmdlet
    {
        /// <summary>
        /// The work items
        /// </summary>
        private BlockingCollection<MarshalItem> _workItems;

        /// <summary>
        /// Gets or sets the bounded capacity.
        /// </summary>
        /// <value>
        /// The bounded capacity.
        /// </value>
        protected int BoundedCapacity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCmdlet"/> class.
        /// </summary>
        /// <param name="boundedCapacity">The bounded capacity.</param>
        protected AsyncCmdlet(int boundedCapacity = 50)
        {
            this.BoundedCapacity = Math.Max(1, boundedCapacity);
            this._workItems = new BlockingCollection<MarshalItem>(this.BoundedCapacity);
        }

        #region sealed overrides
        /// <summary>
        /// Begins the processing.
        /// </summary>
        sealed protected override void BeginProcessing()
        {
            Async(BeginProcessingAsync);
        }

        /// <summary>
        /// Processes the record.
        /// </summary>
        sealed protected override void ProcessRecord()
        {
            Async(ProcessRecordAsync);
        }

        /// <summary>
        /// Ends the processing.
        /// </summary>
        sealed protected override void EndProcessing()
        {
            Async(EndProcessingAsync);
        }

        /// <summary>
        /// Stops the processing.
        /// </summary>
        sealed protected override void StopProcessing()
        {
            Async(StopProcessingAsync);
        }
        #endregion sealed overrides

        #region intercepted methods
        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="text">The text.</param>
        new public void WriteDebug(string text)
        {
            this._workItems.Add(new MarshalItemAction<string>(base.WriteDebug, text));
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="errorRecord">The error record.</param>
        new public void WriteError(ErrorRecord errorRecord)
        {
            this._workItems.Add(new MarshalItemAction<ErrorRecord>(base.WriteError, errorRecord));
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="sendToPipeline">The send to pipeline.</param>
        new public void WriteObject(object sendToPipeline)
        {
            this._workItems.Add(new MarshalItemAction<object>(base.WriteObject, sendToPipeline));
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="sendToPipeline">The send to pipeline.</param>
        /// <param name="enumerateCollection">if set to <c>true</c> [enumerate collection].</param>
        new public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            this._workItems.Add(new MarshalItemAction<object, bool>(base.WriteObject, sendToPipeline, enumerateCollection));
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="progressRecord">The progress record.</param>
        new public void WriteProgress(ProgressRecord progressRecord)
        {
            this._workItems.Add(new MarshalItemAction<ProgressRecord>(base.WriteProgress, progressRecord));
        }

        /// <summary>
        /// Writes a message for verbose level.
        /// </summary>
        /// <param name="text">The text.</param>
        new public void WriteVerbose(string text)
        {
            this._workItems.Add(new MarshalItemAction<string>(base.WriteVerbose, text));
        }

        /// <summary>
        /// Writes a message for warning level.
        /// </summary>
        /// <param name="text">The text.</param>
        new public void WriteWarning(string text)
        {
            this._workItems.Add(new MarshalItemAction<string>(base.WriteWarning, text));
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="text">The text.</param>
        new public void WriteCommandDetail(string text)
        {
            this._workItems.Add(new MarshalItemAction<string>(base.WriteCommandDetail, text));
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        new public bool ShouldProcess(string target)
        {
            var workItem = new MarshalItemFunc<string, bool>(base.ShouldProcess, target);
            this._workItems.Add(workItem);
            return workItem.WaitForResult();
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        new public bool ShouldProcess(string target, string action)
        {
            var workItem = new MarshalItemFunc<string, string, bool>(base.ShouldProcess, target, action);
            this._workItems.Add(workItem);
            return workItem.WaitForResult();
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="verboseDescription">The verbose description.</param>
        /// <param name="verboseWarning">The verbose warning.</param>
        /// <param name="caption">The caption.</param>
        /// <returns></returns>
        new public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            var workItem = new MarshalItemFunc<string, string, string, bool>(base.ShouldProcess, verboseDescription,
                verboseWarning, caption);
            this._workItems.Add(workItem);
            return workItem.WaitForResult();
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="verboseDescription">The verbose description.</param>
        /// <param name="verboseWarning">The verbose warning.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="shouldProcessReason">The should process reason.</param>
        /// <returns></returns>
        new public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption,
            out ShouldProcessReason shouldProcessReason)
        {
            var workItem = new MarshalItemFuncOut<string, string, string, bool, ShouldProcessReason>(
                base.ShouldProcess, verboseDescription, verboseWarning, caption);
            this._workItems.Add(workItem);
            return workItem.WaitForResult(out shouldProcessReason);
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="caption">The caption.</param>
        /// <returns></returns>
        new public bool ShouldContinue(string query, string caption)
        {
            var workItem = new MarshalItemFunc<string, string, bool>(base.ShouldContinue, query, caption);
            this._workItems.Add(workItem);
            return workItem.WaitForResult();
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="yesToAll">if set to <c>true</c> [yes to all].</param>
        /// <param name="noToAll">if set to <c>true</c> [no to all].</param>
        /// <returns></returns>
        new public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            var workItem = new MarshalItemFuncRef<string, string, bool, bool, bool>(base.ShouldContinue, query, caption,
                yesToAll, noToAll);
            this._workItems.Add(workItem);
            return workItem.WaitForResult(ref yesToAll, ref noToAll);
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <returns></returns>
        new public bool TransactionAvailable()
        {
            var workItem = new MarshalItemFunc<bool>(base.TransactionAvailable);
            this._workItems.Add(workItem);
            return workItem.WaitForResult();
        }

        /// <summary>
        /// Writes a message for debug level.
        /// </summary>
        /// <param name="errorRecord">The error record.</param>
        new public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            this._workItems.Add(new MarshalItemAction<ErrorRecord>(base.ThrowTerminatingError, errorRecord));
        }

        #endregion

        #region async processing methods

        /// <summary>
        /// Begins the processing asynchronous.
        /// </summary>
        /// <returns></returns>
        protected virtual Task BeginProcessingAsync()
        {
            return Task.FromResult(0);
        }


        /// <summary>
        /// Ends the processing asynchronous.
        /// </summary>
        /// <returns></returns>
        protected virtual Task EndProcessingAsync()
        {
            return Task.FromResult(0);
        }


        /// <summary>
        /// Processes the record asynchronous.
        /// </summary>
        /// <returns></returns>
        protected virtual Task ProcessRecordAsync()
        {
            return Task.FromResult(0);
        }


        /// <summary>
        /// Stops the processing asynchronous.
        /// </summary>
        /// <returns></returns>
        protected virtual Task StopProcessingAsync()
        {
            return Task.FromResult(0);
        }

        #endregion async processing methods

        /// <summary>
        /// Runs an operation in async mode.
        /// </summary>
        /// <param name="handler">The handler.</param>
        private void Async(Func<Task> handler)
        {
            this._workItems = new BlockingCollection<MarshalItem>(this.BoundedCapacity);

            var task = handler();
            if (task != null)
            {
                var waitable = task.ContinueWith(t => this._workItems.CompleteAdding());

                foreach (var item in this._workItems.GetConsumingEnumerable())
                {
                    item.Invoke();
                }

                waitable.Wait();
            }
        }
        
    }
}