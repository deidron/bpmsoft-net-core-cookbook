namespace EP.EpNInjectContainerExample
{
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web.SessionState;
    using BPMSoft.Web.Common;
    using BPMSoft.Web.Common.ServiceRouting;
    using Ninject;

    [ServiceContract]
    [DefaultServiceRoute, ServiceRoute("ep")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class EpNInjectContainerExampleService : BaseService, IReadOnlySessionState
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpNotifyResponse Notify(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return EpNotifyResponse.Failed("Text must not be empty.");
            using EpUserConnectionScope scope = EpNInjectContainer.Root.BeginUserConnectionScope(UserConnection);
            IEpNotifier notifier = scope.Get<IEpNotifier>();
            return EpNotifyResponse.Succeeded(notifier.Notify(text));
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpNotifyResponse NotifyViaFactory(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return EpNotifyResponse.Failed("Text must not be empty.");
            IEpNotifierFactory notifierFactory = EpNInjectContainer.Root.Get<IEpNotifierFactory>();
            IEpNotifier notifier = notifierFactory.Create(UserConnection);
            return EpNotifyResponse.Succeeded(notifier.Notify(text));
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpUserContextResponse DescribeCurrentUser()
        {
            using EpUserConnectionScope scope = EpNInjectContainer.Root.BeginUserConnectionScope(UserConnection);
            IEpPersonalGreeter greeter = scope.Get<IEpPersonalGreeter>();
            IEpUserCard userCard = scope.Get<IEpUserCard>();
            return new EpUserContextResponse
            {
                Success = true,
                Greeting = greeter.Greet(),
                UserCard = userCard.Describe(),
                GreeterContextId = greeter.Context.ContextId,
                UserCardContextId = userCard.Context.ContextId
            };
        }
    }
}
