namespace APU.WebApp.Services.SignalRHub.Messages;

public record MainHubUserConnectionStateMessage(HubState HubState, string Component, Guid UserId, string UserName);