using System.ComponentModel.DataAnnotations;

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\d{10,15}$",
        ErrorMessage = "Phone number must be 10 to 15 digits")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [MinLength(5, ErrorMessage = "Address must be at least 5 characters")]
    public string Address { get; set; }

    // 🔹 Required for DB persistence
    [Required(ErrorMessage = "OrganizationId is required")]
    public Guid OrganizationId { get; set; }
}
