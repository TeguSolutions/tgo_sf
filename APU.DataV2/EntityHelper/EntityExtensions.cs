using System.Diagnostics;

namespace APU.DataV2.EntityHelper;

public static class EntityExtensions
{
    public static void CalculateCost(this ILabor labor, decimal topFica, decimal fica, decimal futaSuta, decimal workersCompansation)
    {
        try
        {
            var total_worked = labor.Salary * labor.HrsYear;
            total_worked = total_worked.AsRound(4);

            var total_remunerado = total_worked + labor.Salary * (decimal)0.5 * labor.HrsOvertimeYear;
            total_remunerado += labor.VacationsDays * 8 * labor.Salary;
            total_remunerado += labor.HolydaysYear * 8 * labor.Salary;
            total_remunerado += labor.SickDaysYear * 8 * labor.Salary + labor.BonusYear;
            total_remunerado += labor.MeetingsHrsYear * labor.Salary;
            total_remunerado += labor.OfficeHrsYear * labor.Salary;
            total_remunerado += labor.TrainingHrsYear * labor.Salary;
            total_remunerado = total_remunerado.AsRound(4);

            decimal amount_fica;
            if (total_remunerado < topFica)
                amount_fica = total_remunerado * (fica / 100);
            else
                amount_fica = topFica * (fica / 100);
            amount_fica = amount_fica.AsRound(4);

            labor.Cost = labor.SafetyYear / total_worked;
            labor.Cost += labor.Salary * (decimal)0.5 * labor.HrsOvertimeYear / total_worked;
            labor.Cost += labor.VacationsDays * 8 * labor.Salary / total_worked;
            labor.Cost += labor.HolydaysYear * 8 * labor.Salary / total_worked;
            labor.Cost += labor.SickDaysYear * 8 * labor.Salary / total_worked;
            labor.Cost += labor.BonusYear / total_worked;
            labor.Cost += labor.HealthYear / total_worked;
            labor.Cost += labor.LifeInsYear / total_worked;
            labor.Cost += (total_worked + labor.Salary * (decimal)0.5 * labor.HrsOvertimeYear) * (labor.Percentage401 / 100) / total_worked;
            labor.Cost += labor.MeetingsHrsYear * labor.Salary / total_worked;
            labor.Cost += labor.OfficeHrsYear * labor.Salary / total_worked;
            labor.Cost += labor.TrainingHrsYear * labor.Salary / total_worked;
            labor.Cost += labor.UniformsYear / total_worked;
            labor.Cost += futaSuta / total_worked;
            labor.Cost += amount_fica / total_worked;
            labor.Cost += total_remunerado * (workersCompansation / 100) / total_worked;
            // !! not plus equal, just equal !!
            labor.Cost = labor.Cost * labor.Salary + labor.Salary;

            labor.Cost = labor.Cost.AsRound();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            labor.Cost = -1;
        }
    }
}