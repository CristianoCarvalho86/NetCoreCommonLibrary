using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCoreCommonLibrary.Logging
{
    /// <summary>
    /// Logger aprimorado que automaticamente inclui informações de contexto como nome do método, 
    /// linha de código, tipo da classe, e identificadores de correlação.
    /// </summary>
    public class EnhancedLogger : ILogger
    {
        private readonly ILogger _innerLogger;
        private readonly LogContextEnricher _contextEnricher;

        public EnhancedLogger(ILogger innerLogger, LogContextEnricher contextEnricher)
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
            _contextEnricher = contextEnricher ?? throw new ArgumentNullException(nameof(contextEnricher));
        }

        public IDisposable BeginScope<TState>(TState state) => _innerLogger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!IsEnabled(logLevel))
                return;

            // Adiciona o contexto atual ao escopo de log
            using (var scope = _innerLogger.BeginScope(_contextEnricher.GetContext(memberName, sourceFilePath, sourceLineNumber)))
            {
                _innerLogger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Enriquecedor de contexto de log que mantém informações sobre o ambiente de execução atual.
    /// </summary>
    public class LogContextEnricher
    {
        private readonly string _applicationName;
        private readonly string _environmentName;
        private readonly AsyncLocal<string> _correlationId = new AsyncLocal<string>();
        private readonly AsyncLocal<string> _userId = new AsyncLocal<string>();
        private readonly AsyncLocal<Dictionary<string, object>> _customProperties = 
            new AsyncLocal<Dictionary<string, object>>();

        public LogContextEnricher(string applicationName, string environmentName)
        {
            _applicationName = applicationName;
            _environmentName = environmentName;
        }

        /// <summary>
        /// Define o ID de correlação para o contexto atual.
        /// </summary>
        public string CorrelationId
        {
            get => _correlationId.Value ?? string.Empty;
            set => _correlationId.Value = value;
        }

        /// <summary>
        /// Define o ID do usuário para o contexto atual.
        /// </summary>
        public string UserId
        {
            get => _userId.Value ?? string.Empty;
            set => _userId.Value = value;
        }

        /// <summary>
        /// Obtém ou cria as propriedades customizadas para o contexto atual.
        /// </summary>
        private Dictionary<string, object> CustomProperties
        {
            get
            {
                if (_customProperties.Value == null)
                {
                    _customProperties.Value = new Dictionary<string, object>();
                }
                return _customProperties.Value;
            }
        }

        /// <summary>
        /// Adiciona uma propriedade customizada ao contexto de log atual.
        /// </summary>
        public void AddProperty(string key, object value)
        {
            CustomProperties[key] = value;
        }

        /// <summary>
        /// Remove uma propriedade customizada do contexto de log atual.
        /// </summary>
        public void RemoveProperty(string key)
        {
            if (CustomProperties.ContainsKey(key))
            {
                CustomProperties.Remove(key);
            }
        }

        /// <summary>
        /// Limpa todas as propriedades customizadas do contexto de log atual.
        /// </summary>
        public void ClearProperties()
        {
            CustomProperties.Clear();
        }

        /// <summary>
        /// Obtém o contexto de log completo incluindo informações do chamador.
        /// </summary>
        public Dictionary<string, object> GetContext(string memberName, string sourceFilePath, int sourceLineNumber)
        {
            var context = new Dictionary<string, object>
            {
                ["ApplicationName"] = _applicationName,
                ["Environment"] = _environmentName,
                ["CallerMemberName"] = memberName,
                ["CallerFilePath"] = sourceFilePath,
                ["CallerLineNumber"] = sourceLineNumber,
                ["Timestamp"] = DateTimeOffset.UtcNow
            };

            if (!string.IsNullOrEmpty(CorrelationId))
            {
                context["CorrelationId"] = CorrelationId;
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                context["UserId"] = UserId;
            }

            // Adiciona as propriedades customizadas
            foreach (var prop in CustomProperties)
            {
                context[prop.Key] = prop.Value;
            }

            return context;
        }
    }

    /// <summary>
    /// Extensões para configurar e usar o EnhancedLogger.
    /// </summary>
    public static class EnhancedLoggerExtensions
    {
        /// <summary>
        /// Obtém um EnhancedLogger com o contexto enriquecido.
        /// </summary>
        public static ILogger GetEnhancedLogger(this ILoggerFactory loggerFactory, Type type, LogContextEnricher contextEnricher)
        {
            var logger = loggerFactory.CreateLogger(type);
            return new EnhancedLogger(logger, contextEnricher);
        }

        /// <summary>
        /// Define o correlationId no contexto do log e executa a ação especificada com esse contexto.
        /// </summary>
        public static void WithCorrelationId(this LogContextEnricher enricher, string correlationId, Action action)
        {
            var originalCorrelationId = enricher.CorrelationId;
            try
            {
                enricher.CorrelationId = correlationId;
                action();
            }
            finally
            {
                enricher.CorrelationId = originalCorrelationId;
            }
        }

        /// <summary>
        /// Define o userId no contexto do log e executa a ação especificada com esse contexto.
        /// </summary>
        public static void WithUserId(this LogContextEnricher enricher, string userId, Action action)
        {
            var originalUserId = enricher.UserId;
            try
            {
                enricher.UserId = userId;
                action();
            }
            finally
            {
                enricher.UserId = originalUserId;
            }
        }

        /// <summary>
        /// Define uma propriedade customizada no contexto do log e executa a ação especificada com esse contexto.
        /// </summary>
        public static void WithProperty(this LogContextEnricher enricher, string key, object value, Action action)
        {
            enricher.AddProperty(key, value);
            try
            {
                action();
            }
            finally
            {
                enricher.RemoveProperty(key);
            }
        }
    }
} 