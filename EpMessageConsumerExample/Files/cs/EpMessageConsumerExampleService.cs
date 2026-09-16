namespace EP.EpMessageConsumerExample
{
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web.SessionState;
    using BPMSoft.Core.DB;
    using BPMSoft.Core.ServiceBus;
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
        public EpGenerateContactsResponse GenerateContacts(int count)
        {
            if (count < 1 || count > EpGenerateContactCommand.MaxCount)
            {
                return EpGenerateContactsResponse.Failed(
                    $"Count must be between 1 and {EpGenerateContactCommand.MaxCount}, but was {count}.");
            }

            if (!UserConnection.DBSecurityEngine.GetIsEntitySchemaOperationAllowed(
                    EpContactConsts.SchemaName, SchemaOperationRightLevels.CanAppend))
            {
                return EpGenerateContactsResponse.Failed(
                    "Access denied: the current user is not allowed to create contacts.");
            }

            UserConnection.GetMessageBus().SendToIncoming(new EpGenerateContactCommand { Count = count });
            return EpGenerateContactsResponse.Accepted(count);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpGenerateContactsResponse GenerateContactsInBackground(int count)
        {
            if (count < 1 || count > EpGenerateContactsTaskParameters.MaxCount)
            {
                return EpGenerateContactsResponse.Failed(
                    $"Count must be between 1 and {EpGenerateContactsTaskParameters.MaxCount}, but was {count}.");
            }

            if (!UserConnection.DBSecurityEngine.GetIsEntitySchemaOperationAllowed(
                    EpContactConsts.SchemaName, SchemaOperationRightLevels.CanAppend))
            {
                return EpGenerateContactsResponse.Failed(
                    "Access denied: the current user is not allowed to create contacts.");
            }

            BPMSoft.Core.Tasks.Task.StartNewWithUserConnection<
                EpGenerateContactsBackgroundTask, EpGenerateContactsTaskParameters>(
                new EpGenerateContactsTaskParameters { Count = count });
            return EpGenerateContactsResponse.Accepted(count);
        }
    }
}
