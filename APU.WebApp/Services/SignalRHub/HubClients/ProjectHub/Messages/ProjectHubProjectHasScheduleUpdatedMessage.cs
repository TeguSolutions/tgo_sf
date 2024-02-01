namespace APU.WebApp.Services.SignalRHub.HubClients.ProjectHub.Messages;

public record ProjectHubProjectHasScheduleUpdatedMessage(Guid ProjectId, bool HasSchedule);