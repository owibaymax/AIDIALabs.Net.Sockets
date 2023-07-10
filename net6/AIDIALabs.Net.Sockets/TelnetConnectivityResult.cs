namespace AIDIALabs.Net.Sockets
{
    public class TelnetConnectivityResult
    {
        public bool IsPortConnectivityOK { get; set; }
        public bool? IsAlternativePortConnectivityOK { get; set; }
        public Exception Exception { get; set; } = null;
    }
}
