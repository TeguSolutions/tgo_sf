// ReSharper disable once CheckNamespace
namespace APU.DataV2.Entities;

public interface ICommon
{ 
	public Guid Id { get; set; }

	public DateTime LastUpdatedAt { get; set; }

	//public User LastUpdatedBy { get; set; } 
}