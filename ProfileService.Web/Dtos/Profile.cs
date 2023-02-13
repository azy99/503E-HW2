using System.ComponentModel.DataAnnotations;

namespace ProfileService.Web.Dtos;

public record Profile(
    [Required] string username, 
    [Required] string firstName, 
    [Required] string lastName);