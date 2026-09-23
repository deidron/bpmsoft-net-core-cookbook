namespace EP.EpNInjectContainerExample
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using BPMSoft.Core.ServiceModelContract;

    [DataContract]
    public class EpNotifyResponse : BaseResponse
    {
        [DataMember(Name = "deliveries")]
        public Collection<EpNotificationDelivery> Deliveries { get; } = [];

        public static EpNotifyResponse Succeeded(IEnumerable<EpNotificationDelivery> deliveries)
        {
            ArgumentNullException.ThrowIfNull(deliveries);
            EpNotifyResponse response = new() { Success = true };
            foreach (EpNotificationDelivery delivery in deliveries)
                response.Deliveries.Add(delivery);
            return response;
        }

        public static EpNotifyResponse Failed(string errorMessage) =>
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
