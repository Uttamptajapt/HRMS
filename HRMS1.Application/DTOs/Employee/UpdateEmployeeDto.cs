using System.ComponentModel.DataAnnotations;

public class UpdateEmployeeDto
{
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
    public string FirstName { get; set; }

    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters")]
    public string LastName { get; set; }

    [RegularExpression(@"^\d{10,15}$",
        ErrorMessage = "Phone number must be 10 to 15 digits")]
    public string PhoneNumber { get; set; }

    [MinLength(5, ErrorMessage = "Address must be at least 5 characters")]
    public string Address { get; set; }
}
