namespace APU.WebApp.Services.SignalRHub.HubClients.ApuHub.Messages;

public record ApuHubApuCreatedMultipleLineItemsMessage(Guid ProjectId, List<Guid> ApuIds);