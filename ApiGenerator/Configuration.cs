namespace ApiGenerator;

public class Configuration
{
    public bool SeparateClientFiles { get; set; } = false;

    public bool UseInterfacesForClients { get; set; } = true;

    public bool UsePartialClientClasses { get; set; } = true;

    // TODO
    // statt exception ein object returnen, dass den response code beinhaltet (200 usw), ein IsError, error message, exception, und natürlich TResult
}
