using NetCoreCommonLibrary.Data.Entities;
using NetCoreCommonLibrary.Data.Repositories.Interfaces;
using NetCoreCommonLibrary.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services
{
    /// <summary>
    /// Serviço para gerenciar operações relacionadas a ReportHeader.
    /// </summary>
    public class ReportHeaderService : IReportHeaderService
    {
        private readonly IReportHeaderRepository _repository;
        private readonly ILogger<ReportHeaderService> _logger;

        public ReportHeaderService(IReportHeaderRepository repository, ILogger<ReportHeaderService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ReportHeader>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obtendo todos os ReportHeaders.");
            try
            {
                 return await _repository.GetAllAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Erro ao obter todos os ReportHeaders.");
                 throw; // Re-lança a exceção para ser tratada em camadas superiores
            }
        }

        /// <inheritdoc/>
        public async Task<ReportHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obtendo ReportHeader com ID: {ReportHeaderId}", id);
             try
            {
                var reportHeader = await _repository.GetByIdAsync(id, cancellationToken);
                if (reportHeader == null)
                {
                    _logger.LogWarning("ReportHeader com ID: {ReportHeaderId} não encontrado.", id);
                }
                return reportHeader;
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Erro ao obter ReportHeader com ID: {ReportHeaderId}", id);
                 throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default)
        {
             ArgumentNullException.ThrowIfNull(reportHeader);
            _logger.LogInformation("Criando novo ReportHeader com título: {Title}", reportHeader.Title);

            // Definir CreatedAt e UpdatedAt antes de salvar
            // Usar UtcNow para consistência, especialmente em ambientes distribuídos
            var now = DateTime.UtcNow;
            reportHeader.CreatedAt = now;
            reportHeader.UpdatedAt = now;
            
            try
            {
                var newId = await _repository.AddAsync(reportHeader, cancellationToken);
                _logger.LogInformation("ReportHeader criado com sucesso com ID: {NewReportHeaderId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                 // Logar detalhes do reportHeader pode vazar dados sensíveis se não for cuidadoso
                 _logger.LogError(ex, "Erro ao criar ReportHeader com título: {Title}", reportHeader.Title);
                 throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(reportHeader);
            _logger.LogInformation("Atualizando ReportHeader com ID: {ReportHeaderId}", reportHeader.Id);

            // Atualiza apenas a data de atualização
            reportHeader.UpdatedAt = DateTime.UtcNow;
            
            try
            {
                var success = await _repository.UpdateAsync(reportHeader, cancellationToken);
                if (success)
                {
                     _logger.LogInformation("ReportHeader com ID: {ReportHeaderId} atualizado com sucesso.", reportHeader.Id);
                }
                else
                {
                    _logger.LogWarning("Falha ao atualizar ReportHeader com ID: {ReportHeaderId}. Pode ter sido removido ou erro de concorrência.", reportHeader.Id);
                }
                 return success;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Erro ao atualizar ReportHeader com ID: {ReportHeaderId}", reportHeader.Id);
                 throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Tentando deletar ReportHeader com ID: {ReportHeaderId}", id);
            try
            {
                var success = await _repository.DeleteAsync(id, cancellationToken);
                 if (success)
                {
                     _logger.LogInformation("ReportHeader com ID: {ReportHeaderId} deletado com sucesso.", id);
                }
                else
                {
                    _logger.LogWarning("Falha ao deletar ReportHeader com ID: {ReportHeaderId}. Não encontrado ou erro de concorrência.", id);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar ReportHeader com ID: {ReportHeaderId}", id);
                throw;
            }
        }
    }
} 