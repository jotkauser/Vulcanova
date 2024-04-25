using System.IO;
using System.Reactive;
using Prism.Navigation;
using Prism.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vulcanova.Features.Grades.Summary;
using Vulcanova.Features.Settings;
using Vulcanova.Resources;
using Xamarin.Essentials;

namespace Vulcanova.Features.Grades.SubjectDetails;

public sealed class GradesSubjectDetailsViewModel : ReactiveObject, IInitialize
{
    [Reactive]
    public SubjectGrades Subject { get; private set; }
    
    [ObservableAsProperty]
    public bool CanShare { get; }

    public ReactiveCommand<Grade, Unit> ShareGrade { get; }
    
    public ReactiveCommand<Unit, Unit> ShowAnnualAverageRemarks { get; }

    public GradesSubjectDetailsViewModel(AppSettings appSettings, IPageDialogService dialogService)
    {
        ShareGrade = ReactiveCommand.CreateFromTask(async (Grade grade) =>
        {
            if (string.IsNullOrEmpty(grade.ContentRaw)) return;

            var bytes = GradeShareImageGenerator.DrawImageForGrade(grade);
            
            var file = Path.Combine(FileSystem.CacheDirectory, "grade.png");

            await File.WriteAllBytesAsync(file, bytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share this grade",
                File = new ShareFile(file)
            });
        }, appSettings.WhenAnyValue(settings => settings.LongPressToShareGrade));

        ShareGrade.CanExecute.ToPropertyEx(this, vm => vm.CanShare);

        ShowAnnualAverageRemarks = ReactiveCommand.CreateFromTask(async _ =>
            await dialogService.DisplayAlertAsync(Strings.AnnualAverageRemarksDialogTitle,
                Strings.AnnualAverageRemarksDialogMessage, "OK"));
    }

    public void Initialize(INavigationParameters parameters)
    {
        Subject = (SubjectGrades) parameters[nameof(Subject)];
    }
}