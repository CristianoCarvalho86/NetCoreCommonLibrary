using NetCoreCommonLibrary.Data.Entities;
using GeoseaManager.Models;

namespace NetCoreCommonLibrary.Util
{
    /// <summary>
    /// Classe estática para mapeamento manual entre entidades e DTOs.
    /// Considerar usar AutoMapper ou similar para projetos maiores.
    /// </summary>
    public static class Mapper
    {
        // TODO: Avaliar se ReportHeaderDTO deve estar em GeoseaManager ou BlazorCommonLibrary

        /// <summary>
        /// Mapeia uma entidade ReportHeader para seu DTO correspondente.
        /// </summary>
        public static ReportHeaderDTO ToDTO(ReportHeader entity)
        {
            // Verificação de nulo para evitar NullReferenceException
            ArgumentNullException.ThrowIfNull(entity);

            return new ReportHeaderDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                LogoPath = entity.LogoPath,
                HorizontalMargin = entity.HorizontalMargin,
                VerticalMargin = entity.VerticalMargin,
                Client = entity.Client,
                Project = entity.Project,
                Address = entity.Address,
                Location = entity.Location,
                SptId = entity.SptId,
                CoordinatesNorth = entity.CoordinatesNorth,
                CoordinatesEast = entity.CoordinatesEast,
                CoordinatesZone = entity.CoordinatesZone,
                DatumHorizontal = entity.DatumHorizontal,
                DatumVertical = entity.DatumVertical,
                SamplerType = entity.SamplerType,
                SamplerExternalDiameter = entity.SamplerExternalDiameter,
                SamplerInternalDiameter = entity.SamplerInternalDiameter,
                HammerHeight = entity.HammerHeight,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Elevation = entity.Elevation,
                WaterLevel = entity.WaterLevel,
                LiningDiameter = entity.LiningDiameter,
                LiningDepth = entity.LiningDepth,
                ScaleVertical = entity.ScaleVertical,
                ScaleInclination = entity.ScaleInclination,
                HammerWeight = entity.HammerWeight,
                ContractNumber = entity.ContractNumber,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        /// <summary>
        /// Mapeia um ReportHeaderDTO para a entidade ReportHeader correspondente.
        /// </summary>
        public static ReportHeader ToEntity(ReportHeaderDTO dto)
        {
            // Verificação de nulo
            ArgumentNullException.ThrowIfNull(dto);
            
            return new ReportHeader
            {
                Id = dto.Id,
                Title = dto.Title,
                LogoPath = dto.LogoPath,
                HorizontalMargin = dto.HorizontalMargin,
                VerticalMargin = dto.VerticalMargin,
                Client = dto.Client,
                Project = dto.Project,
                Address = dto.Address,
                Location = dto.Location,
                SptId = dto.SptId,
                CoordinatesNorth = dto.CoordinatesNorth,
                CoordinatesEast = dto.CoordinatesEast,
                CoordinatesZone = dto.CoordinatesZone,
                DatumHorizontal = dto.DatumHorizontal,
                DatumVertical = dto.DatumVertical,
                SamplerType = dto.SamplerType,
                SamplerExternalDiameter = dto.SamplerExternalDiameter,
                SamplerInternalDiameter = dto.SamplerInternalDiameter,
                HammerHeight = dto.HammerHeight,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Elevation = dto.Elevation,
                WaterLevel = dto.WaterLevel,
                LiningDiameter = dto.LiningDiameter,
                LiningDepth = dto.LiningDepth,
                ScaleVertical = dto.ScaleVertical,
                ScaleInclination = dto.ScaleInclination,
                HammerWeight = dto.HammerWeight,
                ContractNumber = dto.ContractNumber,
                // CreatedAt e UpdatedAt geralmente são gerenciados pelo banco de dados ou ORM
                // Não mapear do DTO para a entidade, a menos que seja intencional.
                // CreatedAt = dto.CreatedAt, 
                // UpdatedAt = dto.UpdatedAt 
            };
        }
        
        // Adicionar aqui outros métodos de mapeamento conforme necessário
    }
} 