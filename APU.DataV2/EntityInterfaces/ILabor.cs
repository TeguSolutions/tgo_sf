// ReSharper disable once CheckNamespace
namespace APU.DataV2.Entities;

public interface ILabor
{
    Guid Id { get; set; }

    string Description { get; set; }

    decimal Salary { get; set; }

    decimal HrsYear { get; set; }

    decimal HrsStandardYear { get; set; }

    decimal HrsOvertimeYear { get; set; }

    decimal VacationsDays { get; set; }

    decimal HolydaysYear { get; set; }

    decimal SickDaysYear { get; set; }

    decimal BonusYear { get; set; }

    decimal HealthYear { get; set; }

    decimal LifeInsYear { get; set; }

    decimal Percentage401 { get; set; }

    decimal MeetingsHrsYear { get; set; }

    decimal OfficeHrsYear { get; set; }

    decimal TrainingHrsYear { get; set; }

    decimal UniformsYear { get; set; }

    decimal SafetyYear { get; set; }

    // NotMapped
    decimal Cost { get; set; }
}