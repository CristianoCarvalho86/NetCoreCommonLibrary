using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCoreCommonLibrary.Data.Entities;

namespace NetCoreCommonLibrary.Util
{
    /// <summary>
    /// Utility class for mapping between entity and DTO objects.
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Maps a ReportHeader entity to a DTO.
        /// </summary>
        /// <param name="entity">The entity to map.</param>
        /// <returns>A DTO representing the entity.</returns>
        public static object ToDto(ReportHeader entity)
        {
            if (entity == null)
                return null;

            return new
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        /// <summary>
        /// Maps a collection of ReportHeader entities to DTOs.
        /// </summary>
        /// <param name="entities">The entities to map.</param>
        /// <returns>A collection of DTOs.</returns>
        public static IEnumerable<object> ToDtos(IEnumerable<ReportHeader> entities)
        {
            return entities?.Select(ToDto);
        }

        /// <summary>
        /// Maps a DTO to a ReportHeader entity.
        /// </summary>
        /// <param name="dto">The DTO to map.</param>
        /// <returns>An entity representing the DTO.</returns>
        public static ReportHeader ToEntity(object dto)
        {
            if (dto == null)
                return null;

            // Use reflection to get properties
            var dtoType = dto.GetType();
            var entity = new ReportHeader();

            var idProperty = dtoType.GetProperty("Id");
            if (idProperty != null)
                entity.Id = (int)idProperty.GetValue(dto);

            var nameProperty = dtoType.GetProperty("Name");
            if (nameProperty != null)
                entity.Name = (string)nameProperty.GetValue(dto);

            var descriptionProperty = dtoType.GetProperty("Description");
            if (descriptionProperty != null)
                entity.Description = (string)descriptionProperty.GetValue(dto);

            return entity;
        }
    }
} 