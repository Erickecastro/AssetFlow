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
    private DepartmentModel? _selectedAssetDepartment;
    private AssetModel? _selectedAsset;
    private Guid? _editingDepartmentId;
    private Guid? _editingAssetId;
    private string _searchText = string.Empty;
    private readonly List<DepartmentModel> _allDepartments = [];
    private readonly List<AssetModel> _allAssets = [];

    public MainViewModel(AssetFlowApiClient api)
    {
        _api = api;
        LoginCommand = new AsyncCommand(LoginAsync);
        RefreshCommand = new AsyncCommand(RefreshAsync, () => IsAuthenticated);
        AddDepartmentCommand = new AsyncCommand(AddDepartmentAsync, () => IsAuthenticated);
        AddAssetCommand = new AsyncCommand(AddAssetAsync, () => IsAuthenticated && SelectedAssetDepartment is not null);
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
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value)) ApplySearch();
        }
    }
    public string DepartmentSubmitText => _editingDepartmentId is null ? "Adicionar departamento" : "Salvar alterações";
    public string AssetSubmitText => _editingAssetId is null ? "Adicionar ao inventário" : "Salvar alterações";
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsLoggedOut => !IsAuthenticated;
    public bool HasSelectedAsset => SelectedAsset is not null;
    public string MoveButtonText => SelectedAsset?.Status == 2 ? "Transferir ativo" : "Atribuir ativo";
    public int TotalAssetCount => _allAssets.Count;
    public int AvailableAssetCount => _allAssets.Count(asset => asset.Status == 1);
    public int AssignedAssetCount => _allAssets.Count(asset => asset.Status == 2);
    public int MaintenanceAssetCount => _allAssets.Count(asset => asset.Status == 4);
    public double AvailableAssetRatio => Ratio(AvailableAssetCount);
    public double AssignedAssetRatio => Ratio(AssignedAssetCount);
    public double MaintenanceAssetRatio => Ratio(MaintenanceAssetCount);

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

    public DepartmentModel? SelectedAssetDepartment
    {
        get => _selectedAssetDepartment;
        set
        {
            if (SetProperty(ref _selectedAssetDepartment, value)) RaiseCommandStates();
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

            try
            {
                await RefreshCoreAsync();
                StatusMessage = "Conectado ao AssetFlow.";
            }
            catch (Exception)
            {
                StatusMessage = "Login realizado, mas os dados não puderam ser carregados. Verifique a conexão com a API e o banco de dados.";
            }
        });
    }

    private Task RefreshAsync() => RunAsync(RefreshCoreAsync);

    private async Task RefreshCoreAsync()
    {
        var departments = await _api.GetDepartmentsAsync();
        var assets = await _api.GetAssetsAsync();
        _allDepartments.Clear();
        _allDepartments.AddRange(departments);
        _allAssets.Clear();
        _allAssets.AddRange(assets);
        ApplySearch();
        NotifyDashboardCounts();
        SelectedDepartment ??= Departments.FirstOrDefault();
        SelectedAssetDepartment ??= Departments.FirstOrDefault();
        if (SelectedAsset is not null)
        {
            SelectedAsset = Assets.FirstOrDefault(asset => asset.Id == SelectedAsset.Id);
        }
    }

    private Task AddDepartmentAsync() => RunAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(DepartmentName))
        {
            throw new ArgumentException("Informe o nome do departamento antes de salvar.");
        }

        var department = _editingDepartmentId is Guid id
            ? await _api.UpdateDepartmentAsync(id, DepartmentName, DepartmentDescription)
            : await _api.CreateDepartmentAsync(DepartmentName, DepartmentDescription);
        var existing = _allDepartments.FindIndex(item => item.Id == department.Id);
        if (existing >= 0) _allDepartments[existing] = department; else _allDepartments.Add(department);
        ApplySearch();
        SelectedDepartment = department;
        SelectedAssetDepartment ??= department;
        DepartmentName = string.Empty;
        DepartmentDescription = string.Empty;
        SearchText = string.Empty;
        StatusMessage = $"Departamento {department.Name} salvo.";
        _editingDepartmentId = null;
        OnPropertyChanged(nameof(DepartmentSubmitText));
    });

    private Task AddAssetAsync() => RunAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(AssetCode) || string.IsNullOrWhiteSpace(AssetName))
        {
            throw new ArgumentException("Informe o código patrimonial e o nome do ativo antes de salvar.");
        }

        if (_editingAssetId is null && SelectedAssetDepartment is null)
        {
            throw new ArgumentException("Selecione o departamento ou localização inicial do ativo.");
        }

        var asset = _editingAssetId is Guid id
            ? await _api.UpdateAssetAsync(
                id,
                AssetName,
                AssetSerialNumber,
                AssetDescription,
                _allAssets.First(item => item.Id == id).Condition)
            : await _api.CreateAssetAsync(AssetCode, AssetName, AssetSerialNumber, AssetDescription, SelectedAssetDepartment?.Id);
        var existing = _allAssets.FindIndex(item => item.Id == asset.Id);
        if (existing >= 0) _allAssets[existing] = asset; else _allAssets.Add(asset);
        ApplySearch();
        NotifyDashboardCounts();
        SelectedAsset = asset;
        AssetCode = string.Empty;
        AssetName = string.Empty;
        AssetSerialNumber = string.Empty;
        AssetDescription = string.Empty;
        SearchText = string.Empty;
        StatusMessage = $"Ativo {asset.Code} salvo.";
        _editingAssetId = null;
        OnPropertyChanged(nameof(AssetSubmitText));
    });

    public void BeginEditDepartment(DepartmentModel department)
    {
        _editingDepartmentId = department.Id;
        DepartmentName = department.Name;
        DepartmentDescription = department.Description ?? string.Empty;
        OnPropertyChanged(nameof(DepartmentSubmitText));
    }

    public void BeginEditAsset(AssetModel asset)
    {
        _editingAssetId = asset.Id;
        AssetCode = asset.Code;
        AssetName = asset.Name;
        AssetSerialNumber = asset.SerialNumber ?? string.Empty;
        AssetDescription = asset.Description ?? string.Empty;
        SelectedAssetDepartment = _allDepartments.FirstOrDefault(item => item.Id == asset.DepartmentId);
        OnPropertyChanged(nameof(AssetSubmitText));
    }

    public Task DeleteDepartmentAsync(DepartmentModel department) => RunAsync(async () =>
    {
        await _api.DeleteDepartmentAsync(department.Id);
        _allDepartments.RemoveAll(item => item.Id == department.Id);
        ApplySearch();
        StatusMessage = $"Departamento {department.Name} excluído.";
    });

    public Task DeleteAssetAsync(AssetModel asset) => RunAsync(async () =>
    {
        await _api.DeleteAssetAsync(asset.Id);
        _allAssets.RemoveAll(item => item.Id == asset.Id);
        if (SelectedAsset?.Id == asset.Id) SelectedAsset = null;
        ApplySearch();
        NotifyDashboardCounts();
        StatusMessage = $"Ativo {asset.Code} excluído.";
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
        OnPropertyChanged(nameof(AvailableAssetRatio));
        OnPropertyChanged(nameof(AssignedAssetRatio));
        OnPropertyChanged(nameof(MaintenanceAssetRatio));
    }

    private double Ratio(int count) => TotalAssetCount == 0 ? 0 : (double)count / TotalAssetCount;

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values) target.Add(value);
    }

    private void ApplySearch()
    {
        var term = SearchText.Trim();
        Replace(Departments, string.IsNullOrEmpty(term)
            ? _allDepartments
            : _allDepartments.Where(item => item.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (item.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)));
        Replace(Assets, string.IsNullOrEmpty(term)
            ? _allAssets
            : _allAssets.Where(item => item.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (item.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)));
        NotifyDashboardCounts();
    }
}
