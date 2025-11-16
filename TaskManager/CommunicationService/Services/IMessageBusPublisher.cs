using CommunicationService.EventDTOs;

namespace CommunicationService.Services
{
    public interface IMessageBusPublisher
    {
        void PublishTaskUpdated(TaskUpdatedEventDto taskUpdatedDto);
    }
}