namespace Organogram;

public class OrganizationRecord
{
    public int RowId { get; set; }
    public int ParentId { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string Role { get; set; } = "";
    public string MobileNumber { get; set; } = "";
    public string PhoneNumber1 { get; set; } = "";
    public string PhoneNumber2 { get; set; } = "";

    public List<OrganizationRecord> Children { get; } =
        new List<OrganizationRecord>();

    public string DisplayName
    {
        get
        {
            return FirstName + " " + LastName + ", " +
                   Company + ", " +
                   Role;
        }
    }
}