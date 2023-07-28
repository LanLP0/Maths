using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Avalonia.Controls;
using ReactiveUI;
using Splat;

namespace LToolBox.Ui;

/// <summary>
///     A ReactiveUI AutoSuspendHelper which initializes suspension hooks for
///     Avalonia applications. Call its constructor in your app's composition root,
///     before calling the RxApp.SuspensionHost.SetupDefaultSuspendResume method.
/// </summary>
public sealed class GenericSuspendHelper : IDisposable, IEnableLogger
{
    private readonly Subject<Unit> _crash = new();
    private readonly Subject<Unit> _isLaunchingNew = new();
    private readonly Subject<Unit> _isResuming = new();
    private readonly Subject<Unit> _isUnpausing = new();
    private readonly Subject<IDisposable> _shouldPersistState = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="GenericSuspendHelper" /> class.
    /// </summary>
    /// <param name="lifetime">Pass in the Application.ApplicationLifetime property.</param>
    public GenericSuspendHelper()
    {
        RxApp.SuspensionHost.IsLaunchingNew = _isLaunchingNew;
        RxApp.SuspensionHost.IsResuming = _isResuming;
        RxApp.SuspensionHost.IsUnpausing = _isUnpausing;

        // Handle app crash (invalidate state)
        RxApp.SuspensionHost.ShouldInvalidateState = _crash;
        AppDomain.CurrentDomain.UnhandledException += (_, _) => _crash.OnNext(Unit.Default);

        if (!Design.IsDesignMode)
        {
            RxApp.SuspensionHost.ShouldPersistState = _shouldPersistState;
            return;
        }

        RxApp.SuspensionHost.ShouldPersistState = Observable.Never<IDisposable>();
    }

    /// <summary>
    ///     Disposes internally stored observers.
    /// </summary>
    public void Dispose()
    {
        _shouldPersistState.Dispose();
        _isLaunchingNew.Dispose();
    }

    /// <summary>
    ///     This event is handled by shared code for controlled lifetimes
    /// </summary>
    public void OnCreate()
    {
        _isLaunchingNew.OnNext(Unit.Default);
    }

    public void OnResume()
    {
        _isResuming.OnNext(Unit.Default);
    }

    public void OnUnpause()
    {
        _isUnpausing.OnNext(Unit.Default);
    }

    /// <summary>
    ///     This event is handled by shared code for controlled lifetimes
    /// </summary>
    public void SaveState()
    {
        App.Logger!.Debug("Saving State");

        var manual = new ManualResetEvent(false);
        _shouldPersistState.OnNext(Disposable.Create(() => manual.Set()));
        manual.WaitOne();

        App.Logger.Debug("Saving State Completed");
    }
}