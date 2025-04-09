public class Decodeur
{
    public string AdresseIP { get; set; }
    public string Etat { get; set; }
    public DateTime? DernierRedemarrage { get; set; }
    public DateTime? DerniereReinitialisation { get; set; }
    public List<string> Chaines { get; set; } = new List<string>();

    public Decodeur(string adresseIP)
    {
        AdresseIP = adresseIP;
    }
}