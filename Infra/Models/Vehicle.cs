using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Vehicle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string? Mark { get; set; }

    [Required]
    [StringLength(150)]
    public string? Model { get; set; }

    [Required]
    public int Year { get; set; }
}
