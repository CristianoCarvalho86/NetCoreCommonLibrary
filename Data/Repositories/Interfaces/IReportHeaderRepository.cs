using NetCoreCommonLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Data.Repositories.Interfaces
{
    /// <summary>
    /// Interface that defines the operations for the ReportHeader entity repository.
    /// </summary>
    public interface IReportHeaderRepository
    {
        /// <summary>
        /// Asynchronously retrieves all ReportHeaders.
        /// </summary>
        Task<IEnumerable<ReportHeader>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves a ReportHeader by its ID.
        /// </summary>
        Task<ReportHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds a new ReportHeader.
        /// </summary>
        /// <returns>The ID of the added ReportHeader.</returns>
        Task<int> AddAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously updates an existing ReportHeader.
        /// </summary>
        Task<bool> UpdateAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously deletes a ReportHeader by its ID.
        /// </summary>
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}