using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using Gamecure.Core.Common;
using Gamecure.Core.Editor.DownloadAndCopy;
using Gamecure.Core.Plastic;
using Gamecure.Core.Plastic.Parser;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;
using ReactiveUI;
using Gamecure.Core.Common.Logging;
using Splat;

namespace Gamecure.GUI.ViewModels.EditorSync;

internal class EditorSyncViewModel : ViewModelBase
{
    public IAsyncTask<DownloadEditorContext> DownloadEditorTask { get; }
    public IAsyncTask PreReqTask { get; }
    public IAsyncTask<PlasticContext> PlasticChangesetTask { get; }
    public ICommand RefreshPlastic { get; }
    public ICommand DownloadEditor { get; }
    public ICommand ReportEditor { get; }
    public ICommand LaunchGluon { get; }
    public ICommand LaunchEditor { get; }

    public ReadOnlyObservableCollection<ChangesetViewModel> Changesets => _changesets;
    private readonly ReadOnlyObservableCollection<ChangesetViewModel> _changesets;
    private readonly SourceList<ChangesetViewModel> _allChangesets = new();

    private string _branchFilter = string.Empty;
    public string BranchFilter
    {
        get => _branchFilter;
        set => SetProperty(ref _branchFilter, value);
    }

    private bool _mainBranchOnly = true;
    public bool MainBranchOnly
    {
        get => _mainBranchOnly;
        set => SetProperty(ref _mainBranchOnly, value);
    }

    private ChangesetViewModel? _selectedChangeset;
    public ChangesetViewModel? SelectedChangeset
    {
        get => _selectedChangeset;
        set => SetProperty(ref _selectedChangeset, value);
    }

    private bool _isVersionMismatch;
    public bool IsVersionMismatch
    {
        get => _isVersionMismatch;
        set => SetProperty(ref _isVersionMismatch, value);
    }

    private bool _newGamecureVersion;
    public bool NewGamecureVersion
    {
        get => _newGamecureVersion;
        set => SetProperty(ref _newGamecureVersion, value);
    }

    public EditorSyncViewModel()
    : this(
        Locator.Current.GetRequiredService<IConfigurationService>(),
        Locator.Current.GetRequiredService<IPlasticService>(),
        Locator.Current.GetRequiredService<IGoogleAuthService>(),
        Locator.Current.GetRequiredService<IDependenciesService>(),
        Locator.Current.GetRequiredService<IEditorService>(),
        Locator.Current.GetRequiredService<IGamecureVersion>()
        )
    {
    }

    public EditorSyncViewModel(IConfigurationService configurationService, IPlasticService plasticService, IGoogleAuthService googleAuthService, IDependenciesService dependenciesService, IEditorService editorService, IGamecureVersion gamecureVersion)
    {
        {
            // NOTE(Jens): this will get out of control very fast if we decide to have more things we can order by or sort. Can we create something that is easier to use and setup?
            // Subscribe to changes in BranchFilter and the checkbox MainBranchOnly
            var filter = this.WhenAnyValue(m => m.BranchFilter, m => m.MainBranchOnly)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Select(BuildBranchFilter);

            // Connect the filter with the list
            _allChangesets
                .Connect()
                .Filter(filter)
                .Sort(new ChangesetViewModelComparar())
                .ObserveOn(AvaloniaScheduler.Instance)
                .Bind(out _changesets)
                .Subscribe();
        }

        // Set up the Plastic changeset list sync
        PlasticChangesetTask = new AsyncTaskModel<PlasticContext>(async () =>
        {
            _allChangesets.Clear();
#if DEBUG
            // Prevent all actions from running in Design time and use some test data(preview in Visual Studio)
            if (Design.IsDesignMode)
            {
                _allChangesets.AddRange(DesignData);
                _selectedChangeset = DesignData[0];
                return AsyncTaskResult<PlasticContext?>.Success(null);
            }
#endif
            var (plasticTask, editorTask) = (plasticService.Run(), editorService.GetVersions());
            var (plasticResult, (currentVersion, editorVersions)) = (await plasticTask, await editorTask);

            _allChangesets.AddRange(plasticResult
                .Changesets
                .Select(c => new ChangesetViewModel(c, editorVersions.FirstOrDefault(e => e.Changeset == c.Id))
                {
                    IsCurrentEditor = currentVersion == c.Id,
                    IsCurrentChangeset = c.Id == plasticResult.Status?.Head
                }));

            var latestChangeset = editorVersions
                .Where(e => e.Branch == "/main")
                .Max(e => e.Changeset);

            // Check if the current editor is the same as the latest on /main, if it isnt, show a warning.
            IsVersionMismatch = !currentVersion.HasValue || latestChangeset != currentVersion.Value;
            return plasticResult.Succeeded
                ? AsyncTaskResult<PlasticContext?>.Success(plasticResult)
                : AsyncTaskResult<PlasticContext?>.Failed(plasticResult.Reason);
        }, startImmediately: true);

        DownloadEditorTask = new AsyncTaskModel<DownloadEditorContext>(async () =>
        {
            var version = _selectedChangeset?.Version;
            if (version != null)
            {
                var result = await editorService.DownloadEditor(version);
                if (result.Failed)
                {
                    return AsyncTaskResult<DownloadEditorContext?>.Failed(result.Reason);
                }
                IsVersionMismatch = false;
                // Trigger a refresh of the changeset
                RefreshPlastic?.Execute(null);
                return AsyncTaskResult<DownloadEditorContext?>.Success(result);
            }
            return AsyncTaskResult<DownloadEditorContext?>.Failed("No editor found.");
        });

        // Set up the Init task, if this fails nothing on the page will be shown
        PreReqTask = new AsyncTaskModel<string>(async () =>
        {
#if DEBUG
            if (Design.IsDesignMode)
            {
                return AsyncTaskResult<string?>.Success(string.Empty);
            }
#endif
            var userConfig = await configurationService.GetUserConfig();
            if (userConfig == null)
            {
                return AsyncTaskResult<string?>.Failed("Failed to get the configuration. Go to the config tab and make sure everything is working.");
            }

            var (googleTask, dependenciesTask) = (googleAuthService.Run(userConfig.ClientSecret!), dependenciesService.Run());
            var (google, dependencies) = (await googleTask, await dependenciesTask);

            if (google.Failed)
            {
                return AsyncTaskResult<string?>.Failed(google.Reason);
            }
            if (dependencies.Failed)
            {
                return AsyncTaskResult<string?>.Failed(dependencies.Reason);
            }

            return AsyncTaskResult<string?>.Success(string.Empty);
        }, startImmediately: true);

        RefreshPlastic = ReactiveCommand.CreateFromTask(() => PlasticChangesetTask.Execute(true));
        DownloadEditor = ReactiveCommand.CreateFromTask(() => DownloadEditorTask.Execute(true));

        ReportEditor = ReactiveCommand.CreateFromTask(async () =>
        {
            var version = _selectedChangeset?.Version ?? throw new InvalidOperationException("No changeset selected.");
            var appConfig = await configurationService.GetAppConfig() ?? throw new InvalidOperationException("Could not get the AppConfig");

            var jira = appConfig.Jira;

            if (jira is not null)
            {
                var querystring = new Querystring()
                    .Param("pid", jira.ProjectId)
                    .Param("issuetype", jira.IssueType)
                    .Param("summary", $"Bug in editor {version.Changeset}: *** ADD BUG TITLE HERE ***")
                    .Param("description", "Summary of the bug:\n\nHow to reproduce the bug:\n")
                    .Param("priority", jira.Priority);

                var result = await ApplicationLauncher.LaunchBrowser(jira.Url, querystring);
                if (!result)
                {
                    Logger.Error("Browser was closed immediately");
                }
            }
            else
            {
                Logger.Error("There is no configured reporting method");
            }
        });

        LaunchGluon = ReactiveCommand.CreateFromTask(async () =>
        {
            var config = await configurationService.GetUserConfig();
            if (config != null)
            {
                await ApplicationLauncher.LaunchGluon(config.Workspace, TimeSpan.FromSeconds(2));
            }
        });
        LaunchEditor = ReactiveCommand.CreateFromTask(async () =>
        {
            var config = await configurationService.GetUserConfig();
            if (config != null)
            {
                await ApplicationLauncher.LaunchUnrealEditor(config.Workspace, TimeSpan.FromSeconds(2));
            }
        });

        _versionPollTask = Task.Run(async () =>
        {
            if (Design.IsDesignMode)
            {
                return;
            }

            // Run until it crashes or the app is closed.
            while (true)
            {
                NewGamecureVersion = await gamecureVersion.HasNewVersion();
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        });
    }

    private Task _versionPollTask;
    private static Func<ChangesetViewModel, bool> BuildBranchFilter((string, bool) args)
    {
        var (filterText, mainBranchOnly) = args;
        if (mainBranchOnly)
        {
            return t => t.Changeset.Branch.Equals("/main", StringComparison.OrdinalIgnoreCase);
        }

        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _ => true;
        }

        return t => t.Changeset.Branch.Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }


    private static readonly ChangesetViewModel[] DesignData = {
        new(new PlasticChangeset(10, "/main", "Ironman", new DateTime(2000, 02, 03), Guid.NewGuid(), "Ironman made changes\nnew line", new []{new PlasticChanges("armor.txt", PlasticChangeType.Moved), new PlasticChanges("helmet.txt", PlasticChangeType.Changed)}), null) {},
        new(new PlasticChangeset(9, "/main/jira-123", "Spiderman", new DateTime(2000, 02, 03), Guid.NewGuid(),"Spiderman is not a hero", new []{new PlasticChanges("hero.txt", PlasticChangeType.Deleted)}), null) { IsCurrentChangeset = true },
        new(new PlasticChangeset(7, "/main/adhoc-12", "Wonder woman", new DateTime(2000, 02, 03), Guid.NewGuid(),"Wonder woman rules", new []{new PlasticChanges("wonderwoman.txt", PlasticChangeType.Added)}), null) { IsCurrentEditor = true }
    };
}
