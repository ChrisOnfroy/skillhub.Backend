using ClosedXML.Excel;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces.Services;

namespace SkillHub.Infrastructure.Services;

public class ExcelEnrollmentReader : IExcelEnrollmentReader
{
    public Task<IReadOnlyList<BulkEnrollmentRowDto>> ReadRowsAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        var rows = new List<BulkEnrollmentRowDto>();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
            return Task.FromResult<IReadOnlyList<BulkEnrollmentRowDto>>(rows);

        var firstRowUsed = worksheet.FirstRowUsed();
        if (firstRowUsed is null)
            return Task.FromResult<IReadOnlyList<BulkEnrollmentRowDto>>(rows);

        // Find headers
        int emailCol = -1;
        int codeCol = -1;

        foreach (var cell in firstRowUsed.CellsUsed())
        {
            var headerValue = cell.GetString().Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "");
            if (headerValue.Contains("participantemail") || headerValue == "email" || headerValue == "correo")
            {
                emailCol = cell.Address.ColumnNumber;
            }
            else if (headerValue.Contains("workshopcode") || headerValue == "code" || headerValue == "codigo" || headerValue == "codigotaller")
            {
                codeCol = cell.Address.ColumnNumber;
            }
        }

        // Default to columns 1 and 2 if header not found
        if (emailCol == -1) emailCol = 1;
        if (codeCol == -1) codeCol = 2;

        var dataRows = worksheet.RowsUsed().Skip(1);
        foreach (var row in dataRows)
        {
            var email = row.Cell(emailCol).GetString().Trim();
            var code = row.Cell(codeCol).GetString().Trim();

            // Skip empty rows
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(code))
                continue;

            rows.Add(new BulkEnrollmentRowDto(row.RowNumber(), email, code));
        }

        return Task.FromResult<IReadOnlyList<BulkEnrollmentRowDto>>(rows);
    }
}
