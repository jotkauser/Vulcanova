using System.Collections.ObjectModel;

namespace Vulcanova.Features.Grades.Summary;

public class SubjectGrades
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; }
    public decimal? Average { get; set; }
    public decimal? AnnualAverage { get; set; }
    public ObservableCollection<Grade> Grades { get; set; }
}