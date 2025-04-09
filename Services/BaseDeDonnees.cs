public class BaseDeDonnees
{
    private Dictionary<string, string> utilisateurs = new Dictionary<string, string>
    {
        { "AAAA00000000", "motdepasse123" }
    };
    private List<Client> clients = new List<Client>();

    public bool VerifierCredentials(string id, string motDePasse)
    {
        return utilisateurs.ContainsKey(id) && utilisateurs[id] == motDePasse;
    }

    public void AjouterClient(Client client)
    {
        clients.Add(client);
    }

    public bool SupprimerClient(string clientId)
    {
        var client = clients.FirstOrDefault(c => c.Id == clientId);
        if (client != null)
        {
            clients.Remove(client);
            return true;
        }
        return false;
    }

    public Client ObtenirClient(string clientId)
    {
        return clients.FirstOrDefault(c => c.Id == clientId);
    }

    public List<Client> ObtenirTousClients()
    {
        return clients;
    }
}