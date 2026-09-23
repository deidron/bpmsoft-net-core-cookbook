namespace EP.EpNInjectContainerExample
{
    using System;
    using System.Runtime.Serialization;
    using BPMSoft.Core.ServiceModelContract;

    [DataContract]
    public class EpUserContextResponse : BaseResponse
    {
        [DataMember(Name = "greeting")]
        public string Greeting { get; set; }

        [DataMember(Name = "userCard")]
        public string UserCard { get; set; }

        [DataMember(Name = "greeterContextId")]
        public Guid GreeterContextId { get; set; }

        [DataMember(Name = "userCardContextId")]
        public Guid UserCardContextId { get; set; }
    }
}
