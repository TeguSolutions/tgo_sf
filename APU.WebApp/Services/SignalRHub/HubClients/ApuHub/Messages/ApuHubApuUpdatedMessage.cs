namespace APU.WebApp.Services.SignalRHub.HubClients.ApuHub.Messages;

public record ApuHubApuUpdatedMessage(Guid ProjectId, Guid ApuId, bool IsLineItem, bool OrderChanged);