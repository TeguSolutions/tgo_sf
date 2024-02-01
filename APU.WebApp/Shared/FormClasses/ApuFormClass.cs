using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APU.WebApp.Shared.FormClasses;

public class ApuFormClass
{
	public Guid? Id { get; set; }

	public int GroupId { get; set; }

	public int ItemId { get; set; }

	[StringLength(5)]
	public string Code { get; set; }

	public string Description { get; set; }

	[StringLength(20)]
	public string Unit { get; set; }

	public decimal Quantity { get; set; }

	[JsonIgnore] 
	[NotMapped] 
	public bool IsAnyLineItem => GroupId <= 1000 && ItemId is >= 1 and <= 999;
}