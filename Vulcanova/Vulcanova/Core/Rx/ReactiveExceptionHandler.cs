using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AppCenter.Crashes;
using ReactiveUI;
using Vulcanova.Uonet.Api;

namespace Vulcanova.Core.Rx;

public class ReactiveExceptionHandler : IObserver<Exception>
{
    private readonly Subject<ExceptionDescriptor> _exceptions = new();

    public ReactiveExceptionHandler()
    {
        var closes = _exceptions
            .Throttle(TimeSpan.FromSeconds(1));

        closes
            .Window(() => closes)
            .SelectMany(w => w.ToList())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(exceptions =>
            {
                var sessionError =
                    exceptions.FirstOrDefault(e => e.Kind == ExceptionKind.SessionError);

                if (sessionError != null)
                {
                    HandleError(sessionError);
                }

                foreach (var exception in exceptions.Where(
                             e => e.Kind != ExceptionKind.SessionError))
                {
                    
                    HandleError(exception);
                }
            });

        _exceptions.Subscribe(ex =>
        {
            if (ex.ShouldBeTracked)
            {
                Crashes.TrackError(ex.Exception);
            }
        });

        static void HandleError(ExceptionDescriptor descriptor)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
                {
                    try
                    {
                        Interactions.Errors.Handle(descriptor).Subscribe();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            );
        }
    }

    public void OnNext(Exception value)
    {
        if (Debugger.IsAttached) Debugger.Break();

        _exceptions.OnNext(new ExceptionDescriptor(value));
    }

    public void OnError(Exception error)
    {
        if (Debugger.IsAttached) Debugger.Break();
        
        _exceptions.OnNext(new ExceptionDescriptor(error));
    }

    public void OnCompleted()
    {
    }
}

public class ExceptionDescriptor
{
    public ExceptionKind Kind { get; }
    public bool ShouldBeTracked => Kind is ExceptionKind.Other or ExceptionKind.VulcanError;
    public Exception Exception { get; }

    public ExceptionDescriptor(Exception e)
    {
        Kind = e switch
        {
            VulcanException { StatusCode: 108 } => ExceptionKind.SessionError,
            MaintenanceBreakException => ExceptionKind.MaintenanceBreak,
            VulcanException => ExceptionKind.VulcanError,
            _ => ExceptionKind.Other
        };
        Exception = e;
    }
}

public enum ExceptionKind
{
    SessionError,
    MaintenanceBreak,
    VulcanError,
    Other
}