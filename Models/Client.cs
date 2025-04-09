public class Client
{
    public string Id { get; set; }
    public string Nom { get; set; }
    public List<Decodeur> Decodeurs { get; set; } = new List<Decodeur>();
}