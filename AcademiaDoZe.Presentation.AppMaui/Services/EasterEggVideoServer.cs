using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AcademiaDoZe.Presentation.AppMaui.Services;

/// <summary>
/// Servidor HTTP local leve para transmitir o vídeo do Easter Egg diretamente para o WebView,
/// suportando requisições com cabeçalho Range (HTTP 206 Partial Content) para reprodução instantânea.
/// </summary>
public sealed class EasterEggVideoServer : IDisposable
{
    private HttpListener? _listener;
    private readonly string _videoPath;
    private int _port;
    private CancellationTokenSource? _cts;

    public EasterEggVideoServer(string videoPath)
    {
        _videoPath = videoPath;
    }

    public string Start()
    {
        // Encontra uma porta local livre
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        _port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
        _listener.Start();

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ListenLoopAsync(_cts.Token));

        return $"http://127.0.0.1:{_port}/";
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _listener != null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
            catch
            {
                break;
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        try
        {
            var req = context.Request;
            var res = context.Response;

            if (req.Url?.AbsolutePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) == true)
            {
                ServeVideo(req, res);
            }
            else
            {
                // Serve página HTML minimalista com player nativo
                string html = """
                    <!DOCTYPE html>
                    <html lang="pt-BR">
                    <head>
                        <meta charset="utf-8">
                        <meta name="viewport" content="width=device-width, initial-scale=1.0">
                        <style>
                            * { box-sizing: border-box; margin: 0; padding: 0; }
                            html, body {
                                width: 100%;
                                height: 100%;
                                background-color: #000;
                                overflow: hidden;
                                display: flex;
                                justify-content: center;
                                align-items: center;
                                font-family: sans-serif;
                            }
                            video {
                                width: 100%;
                                height: 100%;
                                max-height: 100vh;
                                object-fit: contain;
                                outline: none;
                            }
                        </style>
                    </head>
                    <body>
                        <video controls autoplay playsinline preload="auto">
                            <source src="/video.mp4" type="video/mp4">
                            Seu navegador não suporta reprodução de vídeo.
                        </video>
                    </body>
                    </html>
                    """;

                byte[] buf = Encoding.UTF8.GetBytes(html);
                res.ContentType = "text/html; charset=utf-8";
                res.ContentLength64 = buf.Length;
                res.OutputStream.Write(buf, 0, buf.Length);
                res.OutputStream.Close();
            }
        }
        catch
        {
            // Conexão encerrada pelo cliente
        }
    }

    private void ServeVideo(HttpListenerRequest req, HttpListenerResponse res)
    {
        if (!File.Exists(_videoPath))
        {
            res.StatusCode = (int)HttpStatusCode.NotFound;
            res.Close();
            return;
        }

        var fileInfo = new FileInfo(_videoPath);
        long totalLength = fileInfo.Length;

        res.Headers["Accept-Ranges"] = "bytes";
        res.ContentType = "video/mp4";

        string? rangeHeader = req.Headers["Range"];
        long start = 0;
        long end = totalLength - 1;

        if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        {
            var rangeParts = rangeHeader.Substring(6).Split('-');
            if (long.TryParse(rangeParts[0], out var rStart))
                start = rStart;
            if (rangeParts.Length > 1 && long.TryParse(rangeParts[1], out var rEnd))
                end = rEnd;

            if (end >= totalLength)
                end = totalLength - 1;

            long contentLength = end - start + 1;
            res.StatusCode = (int)HttpStatusCode.PartialContent;
            res.Headers["Content-Range"] = $"bytes {start}-{end}/{totalLength}";
            res.ContentLength64 = contentLength;
        }
        else
        {
            res.StatusCode = (int)HttpStatusCode.OK;
            res.ContentLength64 = totalLength;
        }

        using var fs = new FileStream(_videoPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        fs.Seek(start, SeekOrigin.Begin);
        byte[] buffer = new byte[64 * 1024];
        long bytesRemaining = end - start + 1;

        while (bytesRemaining > 0)
        {
            int toRead = (int)Math.Min(buffer.Length, bytesRemaining);
            int read = fs.Read(buffer, 0, toRead);
            if (read <= 0) break;
            res.OutputStream.Write(buffer, 0, read);
            bytesRemaining -= read;
        }

        res.OutputStream.Close();
    }

    /// <summary>
    /// Localiza o caminho físico do vídeo no ambiente de execução.
    /// </summary>
    public static async Task<string?> ObterCaminhoVideoAsync()
    {
        // 1. Verifica se já foi extraído para AppDataDirectory
        var appDataVideo = Path.Combine(FileSystem.AppDataDirectory, "Editado_academiadoze.mp4");
        if (File.Exists(appDataVideo) && new FileInfo(appDataVideo).Length > 0)
            return appDataVideo;

        // 2. Verifica caminhos comuns em desenvolvimento
        var caminhosCandidatos = new[]
        {
            @"D:\Projetos Gemini\Desenvolvimento de sistemas 2\Editado_academiadoze.mp4",
            @"D:\Projetos Gemini\Desenvolvimento de sistemas 2\Git\AcademiaDoZe.Presentation.AppMaui\Resources\Raw\Editado_academiadoze.mp4",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Raw", "Editado_academiadoze.mp4"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Editado_academiadoze.mp4"),
        };

        foreach (var caminho in caminhosCandidatos)
        {
            if (File.Exists(caminho))
                return caminho;
        }

        // 3. Tenta carregar dos recursos Raw embutidos no pacote MAUI
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("Editado_academiadoze.mp4");
            using var fileStream = File.Create(appDataVideo);
            await stream.CopyToAsync(fileStream);
            return appDataVideo;
        }
        catch
        {
            // Prossegue para busca alternativa se falhar
        }

        // 4. Procura subindo até 6 diretórios pai a partir do BaseDirectory
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        for (int i = 0; i < 6 && dir != null; i++)
        {
            var candidato = Path.Combine(dir.FullName, "Editado_academiadoze.mp4");
            if (File.Exists(candidato)) return candidato;

            // Variação com caminho relativo para Git
            var candidatoGit = Path.Combine(dir.FullName, "Git", "AcademiaDoZe.Presentation.AppMaui", "Resources", "Raw", "Editado_academiadoze.mp4");
            if (File.Exists(candidatoGit)) return candidatoGit;

            dir = dir.Parent;
        }

        return null;
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _listener?.Stop(); } catch { }
        try { _listener?.Close(); } catch { }
        _cts?.Dispose();
    }
}
