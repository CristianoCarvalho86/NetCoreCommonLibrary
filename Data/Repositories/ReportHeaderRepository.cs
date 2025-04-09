using Dapper;
using NetCoreCommonLibrary.Data.Context;
using NetCoreCommonLibrary.Data.Entities;
using NetCoreCommonLibrary.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreCommonLibrary.Data.Repositories
{
    public class ReportHeaderRepository : IReportHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        // Manter a conexão Dapper opcional ou configurá-la separadamente se necessário
        // private readonly IDbConnection _connection; 

        public ReportHeaderRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            // _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<IEnumerable<ReportHeader>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // Usando Entity Framework Core (preferível para CRUD básico)
            return await _context.ReportHeaders
                             .AsNoTracking() // Bom para consultas somente leitura
                             .ToListAsync(cancellationToken);
        }

        public async Task<ReportHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // Usando Entity Framework Core
            return await _context.ReportHeaders.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<int> AddAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default)
        {
             ArgumentNullException.ThrowIfNull(reportHeader);

            _context.ReportHeaders.Add(reportHeader);
            await _context.SaveChangesAsync(cancellationToken);
            return reportHeader.Id;
        }

        public async Task<bool> UpdateAsync(ReportHeader reportHeader, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(reportHeader);

            _context.Entry(reportHeader).State = EntityState.Modified;
            
            try
            {
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Tratar conflito de concorrência se a entidade foi modificada ou deletada por outro processo
                // Você pode recarregar a entidade, logar, ou lançar uma exceção customizada
                return false; 
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var reportHeader = await _context.ReportHeaders.FindAsync(new object[] { id }, cancellationToken);
            if (reportHeader != null)
            {
                _context.ReportHeaders.Remove(reportHeader);
                try
                {
                    return await _context.SaveChangesAsync(cancellationToken) > 0;
                }
                catch (DbUpdateConcurrencyException)
                {
                     // Tratar conflito de concorrência
                    return false;
                }
            }
            return false; // Entidade não encontrada
        }

        // Exemplo com Dapper mantido, mas a conexão precisa ser gerenciada
        // Considere injetar IDbConnectionFactory ou gerenciar a conexão aqui
        /*
        public async Task<IEnumerable<ReportHeader>> GetReportHeadersByClientAsync(string client, IDbConnection connection, CancellationToken cancellationToken = default)
        {
             ArgumentNullException.ThrowIfNull(connection);
             if (string.IsNullOrWhiteSpace(client)) 
                 return Enumerable.Empty<ReportHeader>();

            var parameters = new { Client = client };
            var command = new CommandDefinition(
                "SELECT * FROM report_header WHERE client LIKE CONCAT('%', @Client, '%')", 
                parameters, 
                cancellationToken: cancellationToken);

            return await connection.QueryAsync<ReportHeader>(command);
        }
        */
    }
} 