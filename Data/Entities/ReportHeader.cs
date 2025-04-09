using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCoreCommonLibrary.Data.Entities
{
    [Table("report_header")]
    public class ReportHeader
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("title")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("logo_path")]
        [Display(Name = "Caminho do Logo")]
        public string? LogoPath { get; set; }

        [Column("horizontal_margin", TypeName = "decimal(10, 2)")]
        [Display(Name = "Margem Horizontal")]
        public decimal? HorizontalMargin { get; set; }

        [Column("vertical_margin", TypeName = "decimal(10, 2)")]
        [Display(Name = "Margem Vertical")]
        public decimal? VerticalMargin { get; set; }

        [Required]
        [StringLength(100)]
        [Column("client")]
        [Display(Name = "Cliente")]
        public string Client { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Column("project")]
        [Display(Name = "Projeto")]
        public string Project { get; set; } = string.Empty;

        [Required]
        [Column("address")]
        [Display(Name = "Endereço")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("location")]
        [Display(Name = "Localização")]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("spt_id")]
        [Display(Name = "ID SPT")]
        public string SptId { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("coordinates_north")]
        [Display(Name = "Coordenadas Norte")]
        public string? CoordinatesNorth { get; set; }

        [StringLength(20)]
        [Column("coordinates_east")]
        [Display(Name = "Coordenadas Leste")]
        public string? CoordinatesEast { get; set; }

        [StringLength(10)]
        [Column("coordinates_zone")]
        [Display(Name = "Zona de Coordenadas")]
        public string? CoordinatesZone { get; set; }

        [StringLength(50)]
        [Column("datum_horizontal")]
        [Display(Name = "Datum Horizontal")]
        public string? DatumHorizontal { get; set; }

        [StringLength(50)]
        [Column("datum_vertical")]
        [Display(Name = "Datum Vertical")]
        public string? DatumVertical { get; set; }

        [StringLength(50)]
        [Column("sampler_type")]
        [Display(Name = "Tipo de Amostrador")]
        public string? SamplerType { get; set; }

        [StringLength(20)]
        [Column("sampler_external_diameter")]
        [Display(Name = "Diâmetro Externo do Amostrador")]
        public string? SamplerExternalDiameter { get; set; }

        [StringLength(20)]
        [Column("sampler_internal_diameter")]
        [Display(Name = "Diâmetro Interno do Amostrador")]
        public string? SamplerInternalDiameter { get; set; }

        [StringLength(20)]
        [Column("hammer_height")]
        [Display(Name = "Altura do Martelo")]
        public string? HammerHeight { get; set; }

        [Column("start_date")]
        [Display(Name = "Data de Início")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        [Display(Name = "Data de Término")]
        public DateTime? EndDate { get; set; }

        [Column("elevation", TypeName = "decimal(10, 2)")]
        [Display(Name = "Elevação")]
        public decimal? Elevation { get; set; }

        [Column("water_level", TypeName = "decimal(10, 2)")]
        [Display(Name = "Nível da Água")]
        public decimal? WaterLevel { get; set; }

        [StringLength(20)]
        [Column("lining_diameter")]
        [Display(Name = "Diâmetro do Revestimento")]
        public string? LiningDiameter { get; set; }

        [StringLength(20)]
        [Column("lining_depth")]
        [Display(Name = "Profundidade do Revestimento")]
        public string? LiningDepth { get; set; }

        [StringLength(20)]
        [Column("scale_vertical")]
        [Display(Name = "Escala Vertical")]
        public string? ScaleVertical { get; set; }

        [StringLength(10)]
        [Column("scale_inclination")]
        [Display(Name = "Inclinação da Escala")]
        public string? ScaleInclination { get; set; }

        [Column("hammer_weight", TypeName = "decimal(10, 2)")]
        [Display(Name = "Peso do Martelo")]
        public decimal? HammerWeight { get; set; }

        [StringLength(50)]
        [Column("contract_number")]
        [Display(Name = "Número do Contrato")]
        public string? ContractNumber { get; set; }

        [Column("created_at")]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        [Display(Name = "Atualizado em")]
        public DateTime UpdatedAt { get; set; }
    }
} 