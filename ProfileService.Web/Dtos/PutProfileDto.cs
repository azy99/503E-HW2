using System.ComponentModel.DataAnnotations;

namespace ProfileService.Web.Dtos;

public record PutProfileRequest([Required] string firstName, [Required] string lastName);