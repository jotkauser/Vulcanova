using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vulcanova.Features.Shared;

namespace Vulcanova.Features.Attendance.Report;

public class AttendanceReportViewModel : ReactiveObject
{
    [ObservableAsProperty]
    public IReadOnlyCollection<AttendanceReport> Reports { get; }

    public ReactiveCommand<int, Unit> SelectReport { get; }
    
    [Reactive]
    public AttendanceReport SelectedReport { get; private set; }

    public AttendanceReportViewModel(
        IAttendanceReportRepository attendanceReportRepository,
        AccountContext accountContext)
    {
        var fetchReports = ReactiveCommand.CreateFromTask(async (Unit _) =>
        {
            var currentPeriod = accountContext.Account.Periods.Single(x => x.Current);
            var allPeriodsInYear = accountContext.Account.Periods.Where(x => x.Level == currentPeriod.Level)
                .ToArray();

            var yearStart = allPeriodsInYear.First().Start;
            var yearEnd = allPeriodsInYear.Last().End;

            var items = await attendanceReportRepository.GetAttendanceReportsAsync(accountContext.Account.Id);

            return Array.AsReadOnly(items.OrderBy(x => x.Subject.Name).ToArray());
        });

        fetchReports.ToPropertyEx(this, vm => vm.Reports);

        accountContext.WhenAnyValue(ctx => ctx.Account)
            .WhereNotNull()
            .Select(_ => Unit.Default)
            .InvokeCommand(fetchReports);

        SelectReport = ReactiveCommand.Create((int index) =>
        {
            SelectedReport = Reports.ElementAtOrDefault(index);
        });
    }
}