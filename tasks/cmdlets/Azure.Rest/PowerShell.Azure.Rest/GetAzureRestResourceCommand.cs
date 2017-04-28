using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PowerShell.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace PowerShell.Azure.Rest
{
    /// <summary>
    /// PowerShell command used to get an Azure resource via REST.
    /// </summary>
    /// <seealso cref="System.Management.Automation.Cmdlet" />
    [Cmdlet(VerbsCommon.Get, "AzureRestResource")]
    [OutputType(typeof(ExpandoObject))]
    public class GetAzureRestResourceCommand : AsyncCmdlet
    {
        private const string ResourceAppIdUri = "https://management.azure.com/";

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        [Parameter(Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Security information about the REST call.")]
        public PSObject Context { get; set; }

        /// <summary>
        /// Gets or sets the resource provider URI.
        /// </summary>
        /// <remarks>This is the resource's type including children if needed.</remarks>
        /// <value>
        /// The resource provider URI.
        /// </value>
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Type of resource to retieve including resource provider namespace.")]
        public string ResourceProviderUri { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource group that has the resource.
        /// </summary>
        /// <value>
        /// The name of the resource group.
        /// </value>
        [Parameter(Position = 2,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Resource group name where the resource lives.")]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method to use when sending the REST call.
        /// </summary>
        /// <value>
        /// The HTTP method.
        /// </value>
        [Parameter(Position = 3,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The type of HTTP action to take when calling the REST API.")]
        public string HttpMethod { get; set; }
        
        protected override async Task ProcessRecordAsync()
        {
            // Get Context
            string spnId = Context.Properties["ServicePrincipalId"].Value.ToString();
            string spnKey = Context.Properties["ServicePrincipalKey"].Value.ToString();
            string tenantId = Context.Properties["TenantId"].Value.ToString();
            string subscriptionId = Context.Properties["SubscriptionId"].Value.ToString();

            // Log inputs
            WriteVerbose("ServicePrincipalId", spnId);
            WriteVerbose("TenantId", tenantId);
            WriteVerbose("SubscriptionId", subscriptionId);
            WriteVerbose("HttpMethod", HttpMethod);
            WriteVerbose("ResourceGroupName", ResourceGroupName);
            WriteVerbose("ResourceProviderUri", ResourceProviderUri);

            // Create credentials
            var credentials = new ClientCredential(spnId, spnKey);

            // Get access token
            string authority = $"https://login.windows.net/{tenantId}";
            WriteVerbose("Authority", authority);

            var authContext = new AuthenticationContext(authority, false);
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(ResourceAppIdUri, credentials);
            string accessToken = authResult.AccessToken;

            // Send REST Call
            string resourceUri = $"https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups/{ResourceGroupName}/providers/{ResourceProviderUri}";
            WriteVerbose("ResourceUri", resourceUri);

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = null;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                switch (HttpMethod)
                {
                    case "GET":
                        response = await client.GetAsync(new Uri(resourceUri));
                        break;
                    case "POST":
                        // TODO : add param to set content and content type.
                        var content = new StringContent(string.Empty);
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        response = await client.PostAsync(new Uri(resourceUri), content);
                        break;
                    default:
                        var error = new ErrorRecord(new Exception("Http method is not supported"), "1", ErrorCategory.NotImplemented, null);
                        WriteError(error);
                        throw new NotImplementedException();
                }
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Not Successful");
                    var error = new ErrorRecord(new Exception($"Resource was not found: {await response.Content.ReadAsStringAsync()}"), "0", ErrorCategory.ObjectNotFound, null);
                    WriteError(error);
                }
                else
                {
                    Console.WriteLine("Successful");
                    dynamic result = response.Content.ReadAsAsync<ExpandoObject>().GetAwaiter().GetResult();
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                    WriteObject(result);
                }
            }
        }

        /// <summary>
        /// Writes a message using verbose
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="message">The message.</param>
        private void WriteVerbose(string key, string message)
        {
            WriteVerbose($"{key}: {message}");
        }
    }
}
