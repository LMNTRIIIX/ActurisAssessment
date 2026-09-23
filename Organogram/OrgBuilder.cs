
using System.Security.Cryptography;

namespace Organogram;

public class OrganogramBuilder
{
    public List<OrganizationRecord> ParseData(string data)
    {
        List<OrganizationRecord> records = new List<OrganizationRecord>();

        string[] lines = data.Split
            ( new[] { '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries );

        foreach (string line in lines)
        {
            string[] fields = line.Split(',');

            if (fields.Length != 10)
            {
                throw new FormatException(
                    "Invalid record. Expected 10 fields.");
            }

            OrganizationRecord record = new OrganizationRecord();

            record.RowId = int.Parse(fields[0].Trim());
            record.ParentId = int.Parse(fields[1].Trim());
            record.FirstName = fields[2].Trim();
            record.LastName = fields[3].Trim();
            record.Company = fields[4].Trim();
            record.Location = fields[5].Trim();
            record.Role = fields[6].Trim();
            record.MobileNumber = fields[7].Trim();
            record.PhoneNumber1 = fields[8].Trim();
            record.PhoneNumber2 = fields[9].Trim();

            records.Add(record);
        }
        return records;
    }

    public List<OrganizationRecord> BuildHierarchy(
        List<OrganizationRecord> records)
    {
        Dictionary<int, OrganizationRecord> recordsById =
            new Dictionary<int, OrganizationRecord>();

        List<OrganizationRecord> roots =
            new List<OrganizationRecord>();

        // Lookup of RowID

        foreach (OrganizationRecord record in records)
        {
            recordsById.Add(record.RowId, record);
        }

        // Connect Record to Parent

        foreach (OrganizationRecord record in records)
        {
            if (record.ParentId == 0)
            {
                roots.Add(record);
            }
            else
            {
                if (!recordsById.ContainsKey(record.ParentId))
                {
                    throw new InvalidOperationException(
                        "Parent record not found for Row ID " + record.RowId);
                }

                OrganizationRecord parent =
                    recordsById[record.ParentId];

                parent.Children.Add(record);
            }
        }

        // Sort Root by Row ID
        roots.Sort(
            delegate (OrganizationRecord a, OrganizationRecord b)
            {
                return a.RowId.CompareTo(b.RowId);
            });

        // Sort Children by Row ID
        foreach (OrganizationRecord record in records)
        {
            record.Children.Sort(
                delegate (
                    OrganizationRecord a, OrganizationRecord b)
                {
                    return a.RowId.CompareTo(b.RowId);
                });
        }
        return roots;
    }

    public string CreateOrganogram(
        List<OrganizationRecord> roots)
    {
        List<string> lines = new List<string>();

        foreach (OrganizationRecord root in roots)
        {
            AddRecordToOutput(root, 0, lines);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private void AddRecordToOutput(
        OrganizationRecord record,
        int depth,
        List<string> lines)
    {
        string indentation = "";

        if (depth > 0)
        {
            for (int i = 0; i < depth * 3 + 1; i++)
            {
                indentation += " ";
            }

            indentation += "-> ";
        }

        lines.Add(indentation + record.DisplayName);

        foreach (OrganizationRecord child in record.Children)
        {
            AddRecordToOutput(child, depth + 1, lines);
        }
    }

}

