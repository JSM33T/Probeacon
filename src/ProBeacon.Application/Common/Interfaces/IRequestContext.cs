namespace ProBeacon.Application.Common.Interfaces;

public interface IRequestContext
{
    string UserAgent { get; }
    string IpAddress { get; }
}
