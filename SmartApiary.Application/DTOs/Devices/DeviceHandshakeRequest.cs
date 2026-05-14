namespace SmartApiary.Application.DTOs.Devices;

public class DeviceHandshakeRequest
{
    public string UniqueDeviceId { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
}
