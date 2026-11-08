using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    private bool _isCarregando = true;
    private EasterEggVideoServer? _videoServer;
    private string? _videoPathAtual;

    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
        _isCarregando = false;
    }

    #region Tema
    private void CarregarTema()
    {
        // uso de expressão switch para carregar o índice selecionado
        TemaPicker.SelectedIndex = Preferences.Get("Tema", "system") switch { "light" => 0, "dark" => 1, _ => 2, };
    }

    private void OnTemaSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isCarregando || TemaPicker.SelectedIndex < 0)
            return;

        string selectedTheme = TemaPicker.SelectedIndex switch { 0 => "light", 1 => "dark", _ => "system" };
        Preferences.Set("Tema", selectedTheme);

        // Disparar mensagem para uso na recarga dinâmica imediata
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));
    }

    private async void OnSalvarTemaClicked(object? sender, EventArgs e)
    {
        string selectedTheme = TemaPicker.SelectedIndex switch { 0 => "light", 1 => "dark", _ => "system" };
        Preferences.Set("Tema", selectedTheme);
        // Disparar mensagem para uso na recarga dinâmica
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));
        await DisplayAlertAsync("Sucesso", "Dados salvos com sucesso!", "OK");
        // Navegar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    #endregion

    #region Banco de Dados
    private void CarregarBanco()
    {
        DatabaseTypePicker.Items.Clear();
        foreach (var tipo in Enum.GetValues<AppDatabaseType>())
        {
            DatabaseTypePicker.Items.Add(tipo.ToString());
        }
        var bancoAtual = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        DatabaseTypePicker.SelectedItem = bancoAtual;
        AtualizarInterfacePorTipoBanco();
    }

    private void OnDatabaseTypeChanged(object? sender, EventArgs? e)
    {
        AtualizarInterfacePorTipoBanco();
    }

    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(selectedTypeStr, out var selectedType))
        {
            return;
        }

        switch (selectedType)
        {
            case AppDatabaseType.Sqlite:
                SqliteInfoCard.IsVisible = true;
                SqliteContainer.IsVisible = true;
                ServidorBancoGrid.IsVisible = false;
                CredenciaisGrid.IsVisible = false;
                ComplementoLabel.Text = "Complemento (ex: Default Timeout=5;)";
                ComplementoEntry.Placeholder = "Default Timeout=5;";
                var defaultSqlitePath = DeviceInfo.Platform == DevicePlatform.WinUI
                    ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                    : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");
                SqliteCaminhoEntry.Text = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultSqlitePath));
                ComplementoEntry.Text = Preferences.Get("Sqlite_Complemento", Preferences.Get("Complemento", "Default Timeout=5;"));
                break;

            case AppDatabaseType.SqlServer:
                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;
                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;
                ServidorEntry.Placeholder = "Ex: 172.24.32.1 ou localhost";
                BancoEntry.Placeholder = "Ex: db_academia_do_ze";
                UsuarioEntry.Placeholder = "Ex: sa";
                ComplementoLabel.Text = "Complemento (SSL / Timeout / Criptografia)";
                ComplementoEntry.Placeholder = "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";
                ServidorEntry.Text = Preferences.Get("SqlServer_Servidor", Preferences.Get("Servidor", "172.24.32.1"));
                BancoEntry.Text = Preferences.Get("SqlServer_Banco", Preferences.Get("Banco", "db_academia_do_ze"));
                UsuarioEntry.Text = Preferences.Get("SqlServer_Usuario", Preferences.Get("Usuario", "sa"));
                SenhaEntry.Text = Preferences.Get("SqlServer_Senha", Preferences.Get("Senha", "abcBolinhas12345"));
                ComplementoEntry.Text = Preferences.Get("SqlServer_Complemento", Preferences.Get("Complemento", "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;"));
                break;

            case AppDatabaseType.MySql:
                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;
                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;
                ServidorEntry.Placeholder = "Ex: 10.30.21.16 ou localhost";
                BancoEntry.Placeholder = "Ex: db_academia_do_ze";
                UsuarioEntry.Placeholder = "Ex: root";
                ComplementoLabel.Text = "Complemento (Porta / Timeout)";
                ComplementoEntry.Placeholder = "Connection Timeout=5;Default Command Timeout=30;";
                ServidorEntry.Text = Preferences.Get("MySql_Servidor", Preferences.Get("Servidor", "10.30.21.16"));
                BancoEntry.Text = Preferences.Get("MySql_Banco", Preferences.Get("Banco", "db_academia_do_ze"));
                UsuarioEntry.Text = Preferences.Get("MySql_Usuario", Preferences.Get("Usuario", "root"));
                SenhaEntry.Text = Preferences.Get("MySql_Senha", Preferences.Get("Senha", "abcBolinhas12345"));
                ComplementoEntry.Text = Preferences.Get("MySql_Complemento", Preferences.Get("Complemento", "Connection Timeout=5;Default Command Timeout=30;"));
                break;
        }
    }

    private async void OnSalvarBdClicked(object? sender, EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(selectedTypeStr, out var selectedType))
        {
            await DisplayAlertAsync("Aviso", "Selecione um tipo de banco de dados válido.", "OK");
            return;
        }

        if (selectedType == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(SqliteCaminhoEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o caminho do arquivo do banco SQLite.", "OK");
                return;
            }
            var caminho = SqliteCaminhoEntry.Text.Trim();
            var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;
            Preferences.Set("Sqlite_Caminho", caminho);
            Preferences.Set("Sqlite_Complemento", complemento);
            // Chaves de compatibilidade
            Preferences.Set("SqliteCaminho", caminho);
            Preferences.Set("Complemento", complemento);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ServidorEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o servidor do banco de dados.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(BancoEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o nome do banco de dados.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(UsuarioEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o usuário do banco de dados.", "OK");
                return;
            }
            var servidor = ServidorEntry.Text.Trim();
            var banco = BancoEntry.Text.Trim();
            var usuario = UsuarioEntry.Text.Trim();
            var senha = SenhaEntry.Text ?? string.Empty;
            var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;
            var prefix = selectedType == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
            Preferences.Set($"{prefix}_Servidor", servidor);
            Preferences.Set($"{prefix}_Banco", banco);
            Preferences.Set($"{prefix}_Usuario", usuario);
            Preferences.Set($"{prefix}_Senha", senha);
            Preferences.Set($"{prefix}_Complemento", complemento);
            // Chaves de compatibilidade
            Preferences.Set("Servidor", servidor);
            Preferences.Set("Banco", banco);
            Preferences.Set("Usuario", usuario);
            Preferences.Set("Senha", senha);
            Preferences.Set("Complemento", complemento);
        }

        Preferences.Set("DatabaseType", selectedType.ToString());
        // Disparar a mensagem para recarga dinâmica (processada pelo ConfigurationHelper)
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));
        await DisplayAlertAsync("Sucesso", $"Configurações do banco de dados ({selectedType}) salvas com sucesso!", "OK");
        // Navegar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    #endregion

    #region Easter Egg
    private async void OnAtivarCodigoClicked(object? sender, EventArgs e)
    {
        var codigo = CodigoEspecialEntry?.Text?.Trim()?.ToLowerInvariant() ?? string.Empty;

        // Aceita "deteuspulos" e variações comuns com espaço
        if (codigo == "deteuspulos" || codigo == "de teus pulos")
        {
            await IniciarEasterEggAsync();
        }
        else if (string.IsNullOrWhiteSpace(codigo))
        {
            await DisplayAlertAsync("Aviso", "Por favor, digite um código de acesso.", "OK");
        }
        else
        {
            await DisplayAlertAsync("Código Inválido", "Código não reconhecido. Dica: Qual é o lema oficial da Academia do Zé? 😉", "Tentar Novamente");
        }
    }

    private async Task IniciarEasterEggAsync()
    {
        try
        {
            _videoPathAtual = await EasterEggVideoServer.ObterCaminhoVideoAsync();

            if (string.IsNullOrEmpty(_videoPathAtual) || !File.Exists(_videoPathAtual))
            {
                await DisplayAlertAsync("Easter Egg", "Vídeo 'Editado_academiadoze.mp4' não encontrado nos caminhos do aplicativo.", "OK");
                return;
            }

            // Inicia servidor local de streaming
            _videoServer?.Dispose();
            _videoServer = new EasterEggVideoServer(_videoPathAtual);
            var serverUrl = _videoServer.Start();

            // Carrega WebView e exibe o popup modal
            EasterEggWebView.Source = new UrlWebViewSource { Url = serverUrl };
            EasterEggOverlay.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Erro ao inicializar vídeo do Easter Egg: {ex.Message}", "OK");
        }
    }

    private void OnFecharEasterEggClicked(object? sender, EventArgs e)
    {
        // Fecha o popup e interrompe a reprodução
        EasterEggOverlay.IsVisible = false;
        EasterEggWebView.Source = new HtmlWebViewSource { Html = "<html><body style='background:#000;'></body></html>" };
        _videoServer?.Dispose();
        _videoServer = null;
    }

    private async void OnAbrirExternoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_videoPathAtual) && File.Exists(_videoPathAtual))
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = "De Teus Pulos - Academia do Zé",
                    File = new ReadOnlyFile(_videoPathAtual)
                });
            }
            else
            {
                await DisplayAlertAsync("Aviso", "Arquivo de vídeo não disponível para abertura externa.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível abrir o player externo: {ex.Message}", "OK");
        }
    }
    #endregion

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        // retornar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }

    // Ao fechar a página, garante liberação de recursos para evitar vazamentos de memória
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Garante que o player é encerrado se o usuário sair da tela
        OnFecharEasterEggClicked(this, EventArgs.Empty);
        // Desinscreve o mensageiro para evitar memory leaks
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
