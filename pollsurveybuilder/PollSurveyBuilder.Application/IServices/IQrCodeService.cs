namespace PollSurveyBuilder.Application.IServices;

public interface IQrCodeService
{
    byte[] GeneratePng(string content);
    string GenerateBase64(string content);
}
