using System.Management.Automation;

namespace PowerShell.Azure.Rest
{
    /// <summary>
    /// PowerShell command used to create a new context request when requesting Azure resources.
    /// </summary>
    /// <seealso cref="System.Management.Automation.Cmdlet" />
    [Cmdlet(VerbsCommon.New, "AzureResourceContext")]
    public class NewAzureResourceContextCommand : Cmdlet
    {
        /// <summary>
        /// Gets or sets the service principal identifier.
        /// </summary>
        /// <value>
        /// The service principal identifier.
        /// </value>
        [Parameter(Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Service Principal Application ID")]
        public string ServicePrincipalId { get; set; }

        /// <summary>
        /// Gets or sets the service principal key.
        /// </summary>
        /// <value>
        /// The service principal key.
        /// </value>
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Service Principal Secret")]
        public string ServicePrincipalKey { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        /// <value>
        /// The tenant identifier.
        /// </value>
        [Parameter(Position = 2,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Tenant Identifier")]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the subscription identifier.
        /// </summary>
        /// <value>
        /// The subscription identifier.
        /// </value>
        [Parameter(Position = 3,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Subscription Identifier")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Processes the record.
        /// </summary>
        protected override void ProcessRecord()
        {
            var obj = new PSObject();
            obj.Properties.Add(new PSVariableProperty(new PSVariable("ServicePrincipalId", ServicePrincipalId)));
            obj.Properties.Add(new PSVariableProperty(new PSVariable("ServicePrincipalKey", ServicePrincipalKey)));
            obj.Properties.Add(new PSVariableProperty(new PSVariable("TenantId", TenantId)));
            obj.Properties.Add(new PSVariableProperty(new PSVariable("SubscriptionId", SubscriptionId)));
            WriteObject(obj);
        }
    }
}