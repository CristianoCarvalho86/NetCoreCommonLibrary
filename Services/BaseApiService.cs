using System.Net;
using System.Net.Http.Json;
using NetCoreCommonLibrary.Models;
using NetCoreCommonLibrary.Util;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services
{
    /// <summary>
    /// Serviço base abstrato para comunicação com APIs RESTful.
    /// Fornece métodos genéricos para operações GET, POST, PUT, DELETE.
    /// Requer HttpClient e ILogger via injeção de dependência.
    /// </summary>
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;

        /// <summary>
        /// Construtor do serviço base de API.
        /// </summary>
        /// <param name="httpClient">Instância de HttpClient configurada (preferencialmente via IHttpClientFactory).</param>
        /// <param name="logger">Instância de ILogger para registrar informações e erros.</param>
        protected BaseApiService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Valida se o BaseAddress está configurado no HttpClient
            if (_httpClient.BaseAddress == null)
            {
                 _logger.LogWarning("HttpClient fornecido para {ServiceName} não possui um BaseAddress configurado.", this.GetType().Name);
                 // Opcionalmente, lançar uma exceção:
                 // throw new InvalidOperationException("HttpClient must have a BaseAddress configured.");
            }
        }

        /// <summary>
        /// Realiza uma requisição GET genérica à API e desserializa a resposta.
        /// </summary>
        /// <typeparam name="T">O tipo esperado da resposta.</typeparam>
        /// <param name="endpoint">O caminho relativo do endpoint (ex: "/users" ou "users/1").</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ApiResponse contendo os dados ou informações de erro.</returns>
        protected virtual async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            string requestUrl = PrepareEndpoint(endpoint);
            try
            {
                _logger.LogInformation("Realizando requisição GET para {RequestUrl}", requestUrl);
                
                // Usando a extensão GetFromJsonAsync que já trata JSON
                var result = await _httpClient.GetFromJsonAsync<T>(requestUrl, cancellationToken);
                
                // GetFromJsonAsync lança exceção se status não for sucesso ou se não conseguir desserializar
                // Se chegou aqui, a requisição foi bem sucedida e desserializada (result pode ser null se a API retornar null)
                
                _logger.LogInformation("Requisição GET para {RequestUrl} bem-sucedida.", requestUrl);
                return ApiResponse<T>.CreateSuccess(result!); // Usar null-forgiving se T não for anulável e API garante não-nulidade no sucesso

            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "Recurso não encontrado em GET {RequestUrl}: {StatusCode}", requestUrl, ex.StatusCode);
                return ApiResponse<T>.CreateNotFound($"Recurso em '{requestUrl}' não encontrado.");
            }
             catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
            {
                 _logger.LogInformation("Requisição GET para {RequestUrl} retornou NoContent.", requestUrl);
                // Se NoContent for um sucesso esperado sem dados, retorne sucesso com default(T)
                return ApiResponse<T>.CreateSuccess(default(T)!, "Nenhum conteúdo retornado.", HttpStatusCode.NoContent);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro na requisição GET para {RequestUrl}: {StatusCode} - {Message}", requestUrl, ex.StatusCode, ex.Message);
                return ApiResponse<T>.CreateError($"Erro ao comunicar com a API ({requestUrl}): {ex.Message}", GetStatusCodeOrDefault(ex));
            }
            catch (JsonException ex)
            {
                 _logger.LogError(ex, "Erro ao desserializar resposta JSON de GET {RequestUrl}: {Message}", requestUrl, ex.Message);
                return ApiResponse<T>.CreateError($"Erro ao processar a resposta da API ({requestUrl}): Formato inválido.", HttpStatusCode.InternalServerError);
            }
            catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(ex, "Requisição GET para {RequestUrl} cancelada.", requestUrl);
                 return ApiResponse<T>.CreateError("Requisição cancelada.", HttpStatusCode.RequestTimeout); // Ou outro status apropriado
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na requisição GET para {RequestUrl}: {Message}", requestUrl, ex.Message);
                return ApiResponse<T>.CreateInternalServerError($"Erro inesperado ao processar a requisição para {requestUrl}.");
            }
        }

        /// <summary>
        /// Realiza uma requisição POST genérica à API, enviando dados e esperando uma resposta desserializada.
        /// </summary>
        protected virtual async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
            string endpoint, TRequest data, CancellationToken cancellationToken = default)
        {
            string requestUrl = PrepareEndpoint(endpoint);
            try
            {
                 _logger.LogInformation("Realizando requisição POST para {RequestUrl}", requestUrl);
                 
                // PostAsJsonAsync serializa TRequest para JSON
                var response = await _httpClient.PostAsJsonAsync(requestUrl, data, cancellationToken);

                // Verifica se a requisição foi bem-sucedida (status 2xx)
                response.EnsureSuccessStatusCode(); 

                // Lê e desserializa a resposta
                TResponse? responseData = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
                
                 _logger.LogInformation("Requisição POST para {RequestUrl} bem-sucedida.", requestUrl);
                // Considerar o caso Created (201) vs OK (200)
                return ApiResponse<TResponse>.CreateSuccess(responseData!, "Recurso criado com sucesso.", response.StatusCode);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                 _logger.LogWarning(ex, "Endpoint não encontrado em POST {RequestUrl}: {StatusCode}", requestUrl, ex.StatusCode);
                 return ApiResponse<TResponse>.CreateNotFound($"Endpoint '{requestUrl}' não encontrado para POST.");
            }
             catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                 // Tentar ler o corpo do erro se possível (pode conter detalhes da validação)
                 string errorBody = await TryReadErrorBodyAsync(ex.InnerException as HttpResponseMessage);
                 _logger.LogWarning(ex, "Erro de requisição inválida (BadRequest) em POST {RequestUrl}: {StatusCode}. Body: {ErrorBody}", requestUrl, ex.StatusCode, errorBody);
                 return ApiResponse<TResponse>.CreateError($"Requisição inválida para {requestUrl}. {errorBody}", HttpStatusCode.BadRequest);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro na requisição POST para {RequestUrl}: {StatusCode} - {Message}", requestUrl, ex.StatusCode, ex.Message);
                return ApiResponse<TResponse>.CreateError($"Erro ao comunicar com a API ({requestUrl}): {ex.Message}", GetStatusCodeOrDefault(ex));
            }
             catch (JsonException ex)
            {
                 _logger.LogError(ex, "Erro ao processar JSON na requisição/resposta POST para {RequestUrl}: {Message}", requestUrl, ex.Message);
                return ApiResponse<TResponse>.CreateError($"Erro ao processar dados JSON para {requestUrl}: {ex.Message}", HttpStatusCode.BadRequest); // Ou InternalServer
            }
             catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                 _logger.LogInformation(ex, "Requisição POST para {RequestUrl} cancelada.", requestUrl);
                 return ApiResponse<TResponse>.CreateError("Requisição cancelada.", HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na requisição POST para {RequestUrl}: {Message}", requestUrl, ex.Message);
                 return ApiResponse<TResponse>.CreateInternalServerError($"Erro inesperado ao processar a requisição para {requestUrl}.");
            }
        }

        /// <summary>
        /// Realiza uma requisição PUT genérica à API, enviando dados.
        /// Geralmente usado para atualizações completas de recursos.
        /// </summary>
        protected virtual async Task<ApiResponse> PutAsync<TRequest>(
            string endpoint, TRequest data, CancellationToken cancellationToken = default)
        {
            string requestUrl = PrepareEndpoint(endpoint);
            try
            {
                _logger.LogInformation("Realizando requisição PUT para {RequestUrl}", requestUrl);
                
                // PutAsJsonAsync serializa TRequest para JSON
                var response = await _httpClient.PutAsJsonAsync(requestUrl, data, cancellationToken);

                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Requisição PUT para {RequestUrl} bem-sucedida.", requestUrl);
                // PUT geralmente retorna 200 OK ou 204 No Content
                return ApiResponse.CreateSuccess("Atualização realizada com sucesso.", response.StatusCode);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                 _logger.LogWarning(ex, "Recurso não encontrado em PUT {RequestUrl}: {StatusCode}", requestUrl, ex.StatusCode);
                 return ApiResponse.CreateNotFound($"Recurso em '{requestUrl}' não encontrado para atualização.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                 string errorBody = await TryReadErrorBodyAsync(ex.InnerException as HttpResponseMessage);
                 _logger.LogWarning(ex, "Erro de requisição inválida (BadRequest) em PUT {RequestUrl}: {StatusCode}. Body: {ErrorBody}", requestUrl, ex.StatusCode, errorBody);
                 return ApiResponse.CreateError($"Requisição de atualização inválida para {requestUrl}. {errorBody}", HttpStatusCode.BadRequest);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro na requisição PUT para {RequestUrl}: {StatusCode} - {Message}", requestUrl, ex.StatusCode, ex.Message);
                return ApiResponse.CreateError($"Erro ao comunicar com a API ({requestUrl}): {ex.Message}", GetStatusCodeOrDefault(ex));
            }
             catch (JsonException ex)
            {
                 _logger.LogError(ex, "Erro ao processar JSON na requisição PUT para {RequestUrl}: {Message}", requestUrl, ex.Message);
                return ApiResponse.CreateError($"Erro ao processar dados JSON para {requestUrl}: {ex.Message}", HttpStatusCode.BadRequest);
            }
            catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                 _logger.LogInformation(ex, "Requisição PUT para {RequestUrl} cancelada.", requestUrl);
                 return ApiResponse.CreateError("Requisição cancelada.", HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na requisição PUT para {RequestUrl}: {Message}", requestUrl, ex.Message);
                 return ApiResponse.CreateInternalServerError($"Erro inesperado ao processar a requisição para {requestUrl}.");
            }
        }

        /// <summary>
        /// Realiza uma requisição DELETE genérica à API.
        /// </summary>
        protected virtual async Task<ApiResponse> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            string requestUrl = PrepareEndpoint(endpoint);
            try
            {
                _logger.LogInformation("Realizando requisição DELETE para {RequestUrl}", requestUrl);

                var response = await _httpClient.DeleteAsync(requestUrl, cancellationToken);

                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Requisição DELETE para {RequestUrl} bem-sucedida.", requestUrl);
                 // DELETE geralmente retorna 200 OK ou 204 No Content
                return ApiResponse.CreateSuccess("Exclusão realizada com sucesso.", response.StatusCode);
            }
             catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                 _logger.LogWarning(ex, "Recurso não encontrado em DELETE {RequestUrl}: {StatusCode}", requestUrl, ex.StatusCode);
                 return ApiResponse.CreateNotFound($"Recurso em '{requestUrl}' não encontrado para exclusão.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro na requisição DELETE para {RequestUrl}: {StatusCode} - {Message}", requestUrl, ex.StatusCode, ex.Message);
                return ApiResponse.CreateError($"Erro ao comunicar com a API ({requestUrl}): {ex.Message}", GetStatusCodeOrDefault(ex));
            }
            catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                 _logger.LogInformation(ex, "Requisição DELETE para {RequestUrl} cancelada.", requestUrl);
                 return ApiResponse.CreateError("Requisição cancelada.", HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na requisição DELETE para {RequestUrl}: {Message}", requestUrl, ex.Message);
                return ApiResponse.CreateInternalServerError($"Erro inesperado ao processar a requisição para {requestUrl}.");
            }
        }

        /// <summary>
        /// Prepara o endpoint removendo barras extras.
        /// </summary>
        private string PrepareEndpoint(string endpoint)
        {
            // Remove a barra inicial se presente, pois BaseAddress já termina sem barra (ou deveria)
            return endpoint.TrimStart('/');
        }

        /// <summary>
        /// Obtém o código de status HTTP de uma HttpRequestException ou retorna InternalServerError.
        /// </summary>
        private static HttpStatusCode GetStatusCodeOrDefault(HttpRequestException ex)
        {
            return ex.StatusCode ?? HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Tenta ler o corpo de uma resposta de erro HttpResponseMessage.
        /// </summary>
        private async Task<string> TryReadErrorBodyAsync(HttpResponseMessage? response)
        {
            if (response?.Content == null) return string.Empty;
            try
            {
                // Limita o tamanho da leitura para evitar consumir muita memória
                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(contentStream);
                char[] buffer = new char[1024]; // Lê até 1KB
                int charsRead = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
                return new string(buffer, 0, charsRead);
            }
            catch (Exception ex)
            {
                 _logger.LogWarning(ex, "Falha ao ler o corpo da resposta de erro.");
                return "(Falha ao ler detalhes do erro)";
            }
        }
    }
} 
} 