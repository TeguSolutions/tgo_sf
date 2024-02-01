using APU.DataV2.Utils.HelperClasses;

namespace APU.WebApp.Services.SignalRHub.HubClients.ApuHub.Messages;

public record ApuHubApuDeletedMessage(Guid ProjectId, Guid ApuId, List<GuidInt> ReorderedGroupItems);