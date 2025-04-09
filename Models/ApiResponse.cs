using System.Net;
using System.Text.Json.Serialization;

namespace NetCoreCommonLibrary.Models
{
    /// <summary>
    /// Classe genérica padronizada para respostas de API, incluindo status, mensagem, dados e erros.
    /// </summary>
    /// <typeparam name="T">O tipo dos dados retornados na resposta.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indica se a requisição foi bem-sucedida.
        /// </summary>
        public bool Success { get; private set; }
        
        /// <summary>
        /// Mensagem descritiva associada à resposta (sucesso ou erro).
        /// </summary>
        public string Message { get; private set; } = string.Empty;
        
        /// <summary>
        /// Código de status HTTP da resposta.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
        
        /// <summary>
        /// Dados retornados pela API (pode ser nulo em caso de erro).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; private set; }
        
        /// <summary>
        /// Lista de mensagens de erro detalhadas (se houver).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<string>? Errors { get; private set; }

        private ApiResponse() { }

        /// <summary>
        /// Cria uma resposta de sucesso com dados.
        /// </summary>
        public static ApiResponse<T> CreateSuccess(T data, string message = "Operation completed successfully", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Cria uma resposta de erro sem dados específicos, mas com mensagens de erro.
        /// </summary>
        public static ApiResponse<T> CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors?.AsReadOnly()
            };
        }

        /// <summary>
        /// Cria uma resposta para recurso não encontrado (404).
        /// </summary>
        public static ApiResponse<T> CreateNotFound(string message = "Resource not found")
        {
            return CreateError(message, HttpStatusCode.NotFound);
        }
        
        /// <summary>
        /// Cria uma resposta para acesso não autorizado (401).
        /// </summary>
        public static ApiResponse<T> CreateUnauthorized(string message = "Unauthorized access")
        {
            return CreateError(message, HttpStatusCode.Unauthorized);
        }
        
        /// <summary>
        /// Cria uma resposta para acesso proibido (403).
        /// </summary>
        public static ApiResponse<T> CreateForbidden(string message = "Access forbidden")
        {
            return CreateError(message, HttpStatusCode.Forbidden);
        }
        
        /// <summary>
        /// Cria uma resposta para erro interno do servidor (500).
        /// </summary>
        public static ApiResponse<T> CreateInternalServerError(string message = "An internal server error occurred")
        {
            return CreateError(message, HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Classe para respostas de API que não retornam dados específicos no corpo (apenas status e mensagem).
    /// Herda de ApiResponse<object> onde T é object (ou pode ser um tipo vazio dedicado).
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        private ApiResponse() : base() { }

        /// <summary>
        /// Cria uma resposta de sucesso sem dados.
        /// </summary>
        public static ApiResponse CreateSuccess(string message = "Operation completed successfully", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var baseResponse = ApiResponse<object>.CreateSuccess(null!, message, statusCode);
            return new ApiResponse
            {
                Success = baseResponse.Success,
                Message = baseResponse.Message,
                StatusCode = baseResponse.StatusCode
            };
        }

        /// <summary>
        /// Cria uma resposta de erro sem dados.
        /// </summary>
        public static new ApiResponse CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string>? errors = null)
        {
            var baseResponse = ApiResponse<object>.CreateError(message, statusCode, errors);
            return new ApiResponse
            {
                Success = baseResponse.Success,
                Message = baseResponse.Message,
                StatusCode = baseResponse.StatusCode,
                Errors = baseResponse.Errors
            };
        }
        
        /// <summary>
        /// Cria uma resposta para recurso não encontrado (404) sem dados.
        /// </summary>
        public static new ApiResponse CreateNotFound(string message = "Resource not found")
        {
            return CreateError(message, HttpStatusCode.NotFound);
        }
        
        /// <summary>
        /// Cria uma resposta para acesso não autorizado (401) sem dados.
        /// </summary>
        public static new ApiResponse CreateUnauthorized(string message = "Unauthorized access")
        {
            return CreateError(message, HttpStatusCode.Unauthorized);
        }
        
        /// <summary>
        /// Cria uma resposta para acesso proibido (403) sem dados.
        /// </summary>
        public static new ApiResponse CreateForbidden(string message = "Access forbidden")
        {
            return CreateError(message, HttpStatusCode.Forbidden);
        }
        
        /// <summary>
        /// Cria uma resposta para erro interno do servidor (500) sem dados.
        /// </summary>
        public static new ApiResponse CreateInternalServerError(string message = "An internal server error occurred")
        {
            return CreateError(message, HttpStatusCode.InternalServerError);
        }
    }
} 