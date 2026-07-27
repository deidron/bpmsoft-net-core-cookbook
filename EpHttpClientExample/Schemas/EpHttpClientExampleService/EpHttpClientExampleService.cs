namespace BPMSoft.Configuration.EP
{
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Threading.Tasks;
    using BPMSoft.Core.Factories;
    using BPMSoft.Web.Common;
    using BPMSoft.Web.Common.ServiceRouting;

    [ServiceContract]
    [DefaultServiceRoute, ServiceRoute("ep")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class EpHttpClientExampleService : BaseService
    {
        private IEpHttpClientExample DefaultHttpClient { get; } = ClassFactory.Get<IEpHttpClientExample>("Default");

        private IEpHttpClientExample AntiPatternHttpClient { get; } = ClassFactory.Get<IEpHttpClientExample>("AntiPattern");

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Task<string> TestAsync(string endpoint) =>
            DefaultHttpClient.ExecuteAsync(endpoint);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string Test(string endpoint) =>
            DefaultHttpClient.Execute(endpoint);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Task<string> TestAntiPatternAsync(string endpoint) =>
            AntiPatternHttpClient.ExecuteAsync(endpoint);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string TestAntiPattern(string endpoint) =>
            AntiPatternHttpClient.Execute(endpoint);
    }
}
