namespace APU.WebApp.Services.SignalRHub.HubClients.ApuHub.Messages;

public record ApuHubApuUpdatedMultipleMessage(Guid ProjectId, List<Guid> ApuIds);