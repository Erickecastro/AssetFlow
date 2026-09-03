using System.Collections.ObjectModel;
using AssetFlow.Mobile.Models;
using AssetFlow.Mobile.Services;

namespace AssetFlow.Mobile.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly AssetFlowApiClient _api;
    private string _username = "admin";
    private string _password = "assetflow-demo";
    private string _statusMessage = "Entre para acessar o inventário experimental.";
    private bool _isAuthenticated;
    private bool _isBusy;
    private string _departmentName = string.Empty;
    private string _departmentDescription = string.Empty;
    private string _assetCode = string.Empty;
    private string _assetName = string.Empty;
    private string _assetSerialNumber = string.Empty;
    private string _assetDescription = string.Empty;
    private string _movementNotes = string.Empty;
    private DepartmentModel? _selectedDepartment;
    private AssetModel? _selectedAsset;

    public MainViewModel(AssetFlowApiClient api)
    {
        _api = api;
        LoginCommand = new AsyncCommand(LoginAsync);
        RefreshCommand = new AsyncCommand(RefreshAsync, () => IsAuthenticated);
        AddDepartmentCommand = new AsyncCommand(AddDepartmentAsync, () => IsAuthenticated);
        AddAssetCommand = new AsyncCommand(AddAssetAsync, () => IsAuthenticated);
        MoveAssetCommand = new AsyncCommand(MoveAssetAsync, CanMoveAsset);
        ReturnAssetCommand = new AsyncCommand(ReturnAssetAsync, CanReturnAsset);
        CopyQrCommand = new AsyncCommand(CopyQrAsync, () => SelectedAsset is not null);
        LogoutCommand = new AsyncCommand(LogoutAsync, () => IsAuthenticated);
    }

    public ObservableCollection<DepartmentModel> Departments { get; } = [];
    public ObservableCollection<AssetModel> Assets { get; } = [];
    public AsyncCommand LoginCommand { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand AddDepartmentCommand { get; }
    public AsyncCommand AddAssetCommand { get; }
    public AsyncCommand MoveAssetCommand { get; }
    public AsyncCommand ReturnAssetCommand { get; }
    public AsyncCommand CopyQrCommand { get; }
    public AsyncCommand LogoutCommand { get; }

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string DepartmentName { get => _departmentName; set => SetProperty(ref _departmentName, value); }
    public string DepartmentDescription { get => _departmentDescription; set => SetProperty(ref _departmentDescription, value); }
    public string AssetCode { get => _assetCode; set => SetProperty(ref _assetCode, value); }
    public string AssetName { get => _assetName; set => SetProperty(ref _assetName, value); }
    public string AssetSerialNumber { get => _assetSerialNumber; set => SetProperty(ref _assetSerialNumber, value); }
    public string AssetDescription { get => _assetDescription; set => SetProperty(ref _assetDescription, value); }
    public string MovementNotes { get => _movementNotes; set => SetProperty(ref _movementNotes, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsLoggedOut => !IsAuthenticated;
    public bool HasSelectedAsset => SelectedAsset is not null;
    public string MoveButtonText => SelectedAsset?.Status == 2 ? "Transferir ativo" : "Atribuir ativo";
    public int TotalAssetCount => Assets.Count;
    public int AvailableAssetCount => Assets.Count(asset => asset.Status == 1);
    public int AssignedAssetCount => Assets.Count(asset => asset.Status == 2);
    public int MaintenanceAssetCount => Assets.Count(asset => asset.Status == 4);

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set
        {
            if (SetProperty(ref _isAuthenticated, value))
            {
                OnPropertyChanged(nameof(IsLoggedOut));
                RaiseCommandStates();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public DepartmentModel? SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (SetProperty(ref _selectedDepartment, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public AssetModel? SelectedAsset
    {
        get => _selectedAsset;
        set
        {
            if (SetProperty(ref _selectedAsset, value))
            {
                OnPropertyChanged(nameof(HasSelectedAsset));
                OnPropertyChanged(nameof(MoveButtonText));
                RaiseCommandStates();
            }
        }
    }

    private async Task LoginAsync()
    {
        await RunAsync(async () =>
        {
            await _api.LoginAsync(Username, Password);
            IsAuthenticated = true;
            await RefreshCoreAsync();
            StatusMessage = "Conectado ao AssetFlow.";
        });
    }

    private Task RefreshAsync() => RunAsync(RefreshCoreAsync);

    private async Task RefreshCoreAsync()
    {
        var departments = await _api.GetDepartmentsAsync();
        var assets = await _api.GetAssetsAsync();
        Replace(Departments, departments);
        Replace(Assets, assets);
        NotifyDashboardCounts();
        SelectedDepartment ??= Departments.FirstOrDefault();
        if (SelectedAsset is not null)
        {
            SelectedAsset = Assets.FirstOrDefault(asset => asset.Id == SelectedAsset.Id);
        }
    }

    private Task AddDepartmentAsync() => RunAsync(async () =>
    {
        var department = await _api.CreateDepartmentAsync(DepartmentName, DepartmentDescription);
        Departments.Add(department);
        SelectedDepartment = department;
        DepartmentName = string.Empty;
        DepartmentDescription = string.Empty;
        StatusMessage = $"Departamento {department.Name} adicionado.";
    });

    private Task AddAssetAsync() => RunAsync(async () =>
    {
        var asset = await _api.CreateAssetAsync(AssetCode, AssetName, AssetSerialNumber, AssetDescription);
        Assets.Add(asset);
        NotifyDashboardCounts();
        SelectedAsset = asset;
        AssetCode = string.Empty;
        AssetName = string.Empty;
        AssetSerialNumber = string.Empty;
        AssetDescription = string.Empty;
        StatusMessage = $"Ativo {asset.Code} adicionado.";
    });

    private Task MoveAssetAsync() => RunAsync(async () =>
    {
        var updated = await _api.MoveAssetAsync(SelectedAsset!, SelectedDepartment!.Id, MovementNotes);
        ReplaceAsset(updated);
        MovementNotes = string.Empty;
        StatusMessage = $"Movimentação de {updated.Code} registrada.";
    });

    private Task ReturnAssetAsync() => RunAsync(async () =>
    {
        var updated = await _api.ReturnAssetAsync(SelectedAsset!.Id, MovementNotes);
        ReplaceAsset(updated);
        MovementNotes = string.Empty;
        StatusMessage = $"Ativo {updated.Code} devolvido ao inventário.";
    });

    private async Task CopyQrAsync()
    {
        await Clipboard.Default.SetTextAsync(SelectedAsset!.QrCodeValue);
        StatusMessage = "Identificador QR copiado.";
    }

    private Task LogoutAsync()
    {
        IsAuthenticated = false;
        SelectedAsset = null;
        StatusMessage = "Sessão encerrada.";
        return Task.CompletedTask;
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = "Processando...";
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message.Length <= 240
                ? exception.Message
                : $"{exception.Message[..240]}...";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private bool CanMoveAsset() =>
        IsAuthenticated && SelectedAsset?.Status is 1 or 2 && SelectedDepartment is not null;

    private bool CanReturnAsset() => IsAuthenticated && SelectedAsset?.Status == 2;

    private void ReplaceAsset(AssetModel updated)
    {
        var current = Assets.FirstOrDefault(asset => asset.Id == updated.Id);
        if (current is not null)
        {
            Assets[Assets.IndexOf(current)] = updated;
        }
        SelectedAsset = updated;
        NotifyDashboardCounts();
    }

    private void RaiseCommandStates()
    {
        RefreshCommand.RaiseCanExecuteChanged();
        AddDepartmentCommand.RaiseCanExecuteChanged();
        AddAssetCommand.RaiseCanExecuteChanged();
        MoveAssetCommand.RaiseCanExecuteChanged();
        ReturnAssetCommand.RaiseCanExecuteChanged();
        CopyQrCommand.RaiseCanExecuteChanged();
        LogoutCommand.RaiseCanExecuteChanged();
    }

    private void NotifyDashboardCounts()
    {
        OnPropertyChanged(nameof(TotalAssetCount));
        OnPropertyChanged(nameof(AvailableAssetCount));
        OnPropertyChanged(nameof(AssignedAssetCount));
        OnPropertyChanged(nameof(MaintenanceAssetCount));
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values) target.Add(value);
    }
}
