using iTextSharp.text;
using iTextSharp.text.pdf;
using AuthenticationService.Models;
using AuthenticationService.Repositories;

namespace AuthenticationService.Services;

public class PdfService : IPdfService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PdfService> _logger;

    public PdfService(IUserRepository userRepository, ILogger<PdfService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<byte[]?> GenerateDelegationPdfAsync(Delegation delegation)
    {
        try
        {
            var delegator = await _userRepository.GetByIdAsync(delegation.DelegatorId);
            var delegatee = await _userRepository.GetByIdAsync(delegation.DelegateeId);

            if (delegator == null || delegatee == null)
            {
                return null;
            }

            using var ms = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(0, 0, 0));
            var title = new Paragraph("GIẤY ỦY QUYỀN", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            // Content
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, new BaseColor(0, 0, 0));
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(0, 0, 0));

            document.Add(new Paragraph("Tôi, người đại diện:", boldFont));
            document.Add(new Paragraph($"Họ và tên: {delegator.FullName ?? delegator.Username}", normalFont));
            document.Add(new Paragraph($"Email: {delegator.Email ?? "N/A"}", normalFont));
            document.Add(new Paragraph(" ", normalFont));

            document.Add(new Paragraph("Ủy quyền cho:", boldFont));
            document.Add(new Paragraph($"Họ và tên: {delegatee.FullName ?? delegatee.Username}", normalFont));
            document.Add(new Paragraph($"Email: {delegatee.Email ?? "N/A"}", normalFont));
            document.Add(new Paragraph(" ", normalFont));

            document.Add(new Paragraph("Thời gian ủy quyền:", boldFont));
            document.Add(new Paragraph($"Từ ngày: {delegation.StartDate:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph($"Đến ngày: {delegation.EndDate:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph(" ", normalFont));

            if (!string.IsNullOrEmpty(delegation.Reason))
            {
                document.Add(new Paragraph("Lý do ủy quyền:", boldFont));
                document.Add(new Paragraph(delegation.Reason, normalFont));
                document.Add(new Paragraph(" ", normalFont));
            }

            document.Add(new Paragraph($"Ngày tạo: {delegation.CreatedAt:dd/MM/yyyy HH:mm}", normalFont));
            document.Add(new Paragraph(" ", normalFont));
            document.Add(new Paragraph(" ", normalFont));

            // Signature section
            var signatureTable = new PdfPTable(2);
            signatureTable.WidthPercentage = 100;
            signatureTable.SetWidths(new float[] { 1, 1 });

            var delegatorCell = new PdfPCell(new Phrase("Người ủy quyền\n\n\n" + (delegator.FullName ?? delegator.Username), normalFont));
            delegatorCell.HorizontalAlignment = Element.ALIGN_CENTER;
            delegatorCell.Border = Rectangle.NO_BORDER;
            signatureTable.AddCell(delegatorCell);

            var delegateeCell = new PdfPCell(new Phrase("Người được ủy quyền\n\n\n" + (delegatee.FullName ?? delegatee.Username), normalFont));
            delegateeCell.HorizontalAlignment = Element.ALIGN_CENTER;
            delegateeCell.Border = Rectangle.NO_BORDER;
            signatureTable.AddCell(delegateeCell);

            document.Add(signatureTable);

            document.Close();
            writer.Close();

            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for delegation {DelegationId}", delegation.Id);
            return null;
        }
    }
}

