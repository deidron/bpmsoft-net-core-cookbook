namespace EP.EpNInjectContainerExample
{
    using System.Runtime.Serialization;

    [DataContract]
    public class EpNotificationDelivery
    {
        [DataMember(Name = "channel")]
        public string Channel { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }
    }
}
