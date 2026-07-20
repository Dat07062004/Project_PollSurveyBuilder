using System;
using PollSurveyBuilder.Application.IServices;
using QRCoder;

namespace PollSurveyBuilder.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] GeneratePng(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(20);
    }

    public string GenerateBase64(string content)
    {
        byte[] bytes = GeneratePng(content);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}
