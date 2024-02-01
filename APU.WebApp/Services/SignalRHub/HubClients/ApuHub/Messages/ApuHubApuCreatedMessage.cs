namespace APU.WebApp.Services.SignalRHub.HubClients.ApuHub.Messages;

public record ApuHubApuCreatedMessage(Guid ProjectId, Guid ApuId, bool IsLineItem);