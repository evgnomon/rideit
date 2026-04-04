using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace rideit.Core.Models;

public class WeatherForecast
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string PartitionKey { get; set; } = "forecast";

    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }

    [MaxLength(50)]
    public string Summary { get; set; } = default!;

    [JsonIgnore]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class CreateForecastRequest
{
    [Required]
    [Range(-60, 60, ErrorMessage = "Temperature must be between -60 and 60")]
    public int TemperatureC { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Summary { get; init; } = default!;

    public DateTime? Date { get; init; }
}

public class UpdateForecastRequest
{
    [Range(-60, 60)]
    public int? TemperatureC { get; init; }

    [StringLength(50, MinimumLength = 2)]
    public string? Summary { get; init; }
}

public class WeatherOptions
{
    public const string SectionName = "Weather";
    public string DefaultSummary { get; set; } = "Mild";
    public int MaxForecastDays { get; set; } = 5;
}
