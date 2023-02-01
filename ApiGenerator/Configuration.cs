namespace ApiGenerator;

public class Configuration
{
    public bool SeparateClientFiles { get; set; } = false;

    public bool UseInterfacesForClients { get; set; } = true;

    public bool UsePartialClientClasses { get; set; } = true;
}
