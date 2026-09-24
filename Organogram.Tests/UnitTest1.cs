using NUnit.Framework;
using Organogram;

namespace Organogram.Tests;

public class OrganogramTests
{
    private OrganogramBuilder service = null!;

    [SetUp]
    public void Setup()
    {
        service = new OrganogramBuilder();
    }

    [Test]
    public void ParseData_ShouldReadAllRecords()
    {
        string data =
            "1,0,Peter,Ndoro,IBM,Johannesburg,Managing Director,123,456,789";

        List<OrganizationRecord> records =
            service.ParseData(data);

        Assert.That(records.Count, Is.EqualTo(1));

        Assert.That(records[0].RowId, Is.EqualTo(1));
        Assert.That(records[0].ParentId, Is.EqualTo(0));
        Assert.That(records[0].FirstName, Is.EqualTo("Peter"));
        Assert.That(records[0].LastName, Is.EqualTo("Ndoro"));
        Assert.That(records[0].Company, Is.EqualTo("IBM"));
        Assert.That(
            records[0].Role,
            Is.EqualTo("Managing Director"));
    }

    [Test]
    public void BuildHierarchy_ShouldConnectChildToParent()
    {
        string data = """
        2,1,Jackie,Smith,IBM,Johannesburg,Assistant Director,1,2,3
        1,0,Peter,Ndoro,IBM,Johannesburg,Managing Director,4,5,6
        """;

        List<OrganizationRecord> records =
            service.ParseData(data);

        List<OrganizationRecord> roots =
            service.BuildHierarchy(records);

        Assert.That(roots.Count, Is.EqualTo(1));
        Assert.That(roots[0].RowId, Is.EqualTo(1));

        Assert.That(
            roots[0].Children.Count,
            Is.EqualTo(1));

        Assert.That(
            roots[0].Children[0].RowId,
            Is.EqualTo(2));
    }

    [Test]
    public void BuildHierarchy_ShouldSortChildrenByRowId()
    {
        string data = """
        3,1,Chris,Thorpe,IBM,Johannesburg,Director,1,2,3
        5,1,James,McDonald,IBM,Johannesburg,Developer,4,5,6
        2,1,Jackie,Smith,IBM,Johannesburg,Assistant,7,8,9
        1,0,Peter,Ndoro,IBM,Johannesburg,Managing Director,10,11,12
        """;

        List<OrganizationRecord> records =
            service.ParseData(data);

        List<OrganizationRecord> roots =
            service.BuildHierarchy(records);

        List<OrganizationRecord> children =
            roots[0].Children;

        Assert.That(children[0].RowId, Is.EqualTo(2));
        Assert.That(children[1].RowId, Is.EqualTo(3));
        Assert.That(children[2].RowId, Is.EqualTo(5));
    }

    [Test]
    public void BuildHierarchy_ShouldSupportMultipleLevels()
    {
        string data = """
        4,3,Person4,Test,IBM,Location,Level 4,1,2,3
        1,0,Person1,Test,IBM,Location,Level 1,1,2,3
        3,2,Person3,Test,IBM,Location,Level 3,1,2,3
        2,1,Person2,Test,IBM,Location,Level 2,1,2,3
        """;

        List<OrganizationRecord> records =
            service.ParseData(data);

        List<OrganizationRecord> roots =
            service.BuildHierarchy(records);

        OrganizationRecord level1 = roots[0];
        OrganizationRecord level2 = level1.Children[0];
        OrganizationRecord level3 = level2.Children[0];
        OrganizationRecord level4 = level3.Children[0];

        Assert.That(level1.RowId, Is.EqualTo(1));
        Assert.That(level2.RowId, Is.EqualTo(2));
        Assert.That(level3.RowId, Is.EqualTo(3));
        Assert.That(level4.RowId, Is.EqualTo(4));
    }

    [Test]
    public void CreateOrganogram_ShouldPrintHierarchy()
    {
        string data = """
        3,2,Chris,Thorpe,IBM,Johannesburg,Director,1,2,3
        1,0,Peter,Ndoro,IBM,Johannesburg,Managing Director,4,5,6
        2,1,Jackie,Smith,IBM,Johannesburg,Assistant Director,7,8,9
        """;

        List<OrganizationRecord> records =
            service.ParseData(data);

        List<OrganizationRecord> roots =
            service.BuildHierarchy(records);

        string result =
            service.CreateOrganogram(roots);

        string expected =
            "Peter Ndoro, IBM, Managing Director" +
            Environment.NewLine +
            " -> Jackie Smith, IBM, Assistant Director" +
            Environment.NewLine +
            "    -> Chris Thorpe, IBM, Director";

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void CreateOrganogram_ShouldMatchBriefExample()
    {
        string data = """
    1,0,Peter,Ndoro,IBM,Johannesburg,Managing Director,1,2,3
    2,1,Jackie,Smith,IBM,Johannesburg,Assistant Director,1,2,3
    3,1,Chris,Thorpe,IBM,Johannesburg,Technical Director,1,2,3
    4,3,John,Major,IBM,Johannesburg,Lead Developer,1,2,3
    5,4,Peter,South,IBM,Johannesburg,Senior Developer,1,2,3
    6,4,James,McDonald,IBM,Johannesburg,Developer,1,2,3
    """;

        List<OrganizationRecord> records = service.ParseData(data);
        List<OrganizationRecord> roots = service.BuildHierarchy(records);
        string result = service.CreateOrganogram(roots);

        string expected =
            "Peter Ndoro, IBM, Managing Director" + Environment.NewLine +
            " -> Jackie Smith, IBM, Assistant Director" + Environment.NewLine +
            " -> Chris Thorpe, IBM, Technical Director" + Environment.NewLine +
            "    -> John Major, IBM, Lead Developer" + Environment.NewLine +
            "       -> Peter South, IBM, Senior Developer" + Environment.NewLine +
            "       -> James McDonald, IBM, Developer";

        Assert.That(result, Is.EqualTo(expected));
    }
}