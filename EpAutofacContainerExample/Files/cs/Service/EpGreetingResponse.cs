namespace EP.EpAutofacContainerExample
{
    using System.Runtime.Serialization;
    using BPMSoft.Core.ServiceModelContract;

    [DataContract]
    public class EpGreetingResponse : BaseResponse
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }

        public static EpGreetingResponse Succeeded(string message) =>
            new() { Success = true, Message = message };

        public static EpGreetingResponse Failed(string errorMessage) =>
            new()
            {
                Success = false,
                ErrorInfo = new ErrorInfo()
                {
                    Message = errorMessage
                }
            };
    }
}
