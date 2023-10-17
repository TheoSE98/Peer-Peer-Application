namespace WebDBServer.Models
{
    public class ClientModel
    {
        public int ID { get; set; } // Might not be needed 
        public string? IP { get; set; }
        public int Port { get; set; }
    }
}