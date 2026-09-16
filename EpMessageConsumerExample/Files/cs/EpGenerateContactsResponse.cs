namespace EP.EpMessageConsumerExample
{
    using System.Runtime.Serialization;
    using BPMSoft.Core.ServiceModelContract;

    [DataContract]
    public class EpGenerateContactsResponse : BaseResponse
    {
        [DataMember(Name = "acceptedCount")]
        public int AcceptedCount { get; set; }

        public static EpGenerateContactsResponse Accepted(int count) =>
            new EpGenerateContactsResponse { Success = true, AcceptedCount = count };

        public static EpGenerateContactsResponse Failed(string errorMessage) =>
            new EpGenerateContactsResponse
            {
                Success = false,
                ErrorInfo = new ErrorInfo()
                {
                    Message = errorMessage
                }
            };
    }
}
