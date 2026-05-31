namespace ProBeacon.Api.Services;

// Singleton — caches whether setup has been completed so the guard middleware
// doesn't hit the DB on every request after first run.
public class SetupState
{
    public bool? IsConfigured { get; set; }

    public void MarkConfigured() => IsConfigured = true;
}
