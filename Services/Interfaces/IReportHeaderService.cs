using NetCoreCommonLibrary.Data.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Services.Interfaces
{
    /// <summary>
    /// Interface específica para o serviço que gerencia operações de ReportHeader.
    /// Herda operações básicas e pode definir métodos adicionais específicos.
    /// </summary>
    public interface IReportHeaderService
    {
        /// <summary>
        /// Obtém todos os ReportHeaders de forma assíncrona.
        /// </summary>
        Task<IEnumerable<ReportHeader>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtém um ReportHeader pelo seu ID de forma assíncrona.
        /// </summary>
        Task<ReportHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cria um novo ReportHeader de forma assíncrona.
        /// </summary>
        /// <returns>O ID do ReportHeader criado.</returns>
        Task<int> CreateAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza um ReportHeader existente de forma assíncrona.
        /// </summary>
        /// <returns>True se a atualização foi bem-sucedida, False caso contrário.</returns>
        Task<bool> UpdateAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deleta um ReportHeader pelo seu ID de forma assíncrona.
        /// </summary>
        /// <returns>True se a exclusão foi bem-sucedida, False caso contrário.</returns>
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        // Adicionar aqui métodos específicos para ReportHeader, se necessário.
        // Exemplo: Task<IEnumerable<ReportHeader>> GetReportsByProjectAsync(string projectId, CancellationToken cancellationToken = default);
    }
} 