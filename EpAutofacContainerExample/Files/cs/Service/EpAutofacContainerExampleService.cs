namespace EP.EpAutofacContainerExample
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web.SessionState;
    using Autofac;
    using BPMSoft.Core;
    using BPMSoft.Web.Common;
    using BPMSoft.Web.Common.ServiceRouting;

    [ServiceContract]
    [DefaultServiceRoute, ServiceRoute("ep")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class EpAutofacContainerExampleService : BaseService, IReadOnlySessionState
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpGreetingResponse Greet(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return EpGreetingResponse.Failed("Name must not be empty.");
            IEpGreetingService greetingService = EpAutofacContainer.Root.Resolve<IEpGreetingService>();
            return EpGreetingResponse.Succeeded(greetingService.Greet(name));
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpGreetingResponse GreetCurrentUser()
        {
            using ILifetimeScope scope = EpAutofacContainer.Root.BeginUserConnectionScope(UserConnection);
            IEpCurrentUserGreeter greeter = scope.Resolve<IEpCurrentUserGreeter>();
            return EpGreetingResponse.Succeeded(greeter.GreetCurrentUser());
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpGreetingResponse GreetCurrentUserViaFactory()
        {
            Func<UserConnection, IEpCurrentUserGreeter> createGreeter =
                EpAutofacContainer.Root.Resolve<Func<UserConnection, IEpCurrentUserGreeter>>();
            IEpCurrentUserGreeter greeter = createGreeter(UserConnection);
            return EpGreetingResponse.Succeeded(greeter.GreetCurrentUser());
        }

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public EpUserContextResponse DescribeCurrentUser()
        {
            using ILifetimeScope scope = EpAutofacContainer.Root.BeginUserConnectionScope(UserConnection);
            IEpPersonalGreeter greeter = scope.Resolve<IEpPersonalGreeter>();
            IEpUserCard userCard = scope.Resolve<IEpUserCard>();
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
