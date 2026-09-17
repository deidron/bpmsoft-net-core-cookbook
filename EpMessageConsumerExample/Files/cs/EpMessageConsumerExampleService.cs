namespace EP.EpMessageConsumerExample
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web.SessionState;
    using BPMSoft.Core.DB;
    using BPMSoft.Core.ServiceBus;
    using BPMSoft.Core.Tasks;
    using BPMSoft.Web.Common;
    using BPMSoft.Web.Common.ServiceRouting;

    [ServiceContract]
    [DefaultServiceRoute, ServiceRoute("ep")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class EpMessageConsumerExampleService : BaseService, IReadOnlySessionState
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpGenerateContactsResponse GenerateContacts(int count) =>
            StartGeneration<EpGenerateContactCommand>(count, EpGenerateContactCommand.MaxCount,
                command => UserConnection.GetMessageBus().SendToIncoming(command));

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpGenerateContactsResponse GenerateContactsInBackground(int count) =>
            StartGeneration<EpGenerateContactsTaskParameters>(count, EpGenerateContactsTaskParameters.MaxCount,
                parameters => Task.StartNewWithUserConnection<
                    EpGenerateContactsBackgroundTask, EpGenerateContactsTaskParameters>(parameters));

        private EpGenerateContactsResponse StartGeneration<TRequest>(int count, int maxCount, Action<TRequest> start)
            where TRequest : IEpGenerateContactsRequest, new()
        {
            if (count < 1 || count > maxCount)
            {
                return EpGenerateContactsResponse.Failed(
                    $"Count must be between 1 and {maxCount}, but was {count}.");
            }
            if (!UserConnection.DBSecurityEngine.GetIsEntitySchemaOperationAllowed(
                    EpContactConsts.SchemaName, SchemaOperationRightLevels.CanAppend))
            {
                return EpGenerateContactsResponse.Failed(
                    "Access denied: the current user is not allowed to create contacts.");
            }
            TRequest request = new TRequest { Count = count };
            start(request);
            return EpGenerateContactsResponse.Accepted(count);
        }
    }
}
