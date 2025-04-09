using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

public class GestionnaireDecodeurs
{
    private readonly HttpClient client;
    private readonly string codePermanent = "AAAA00000000"; // Remplace par ton code permanent
    private readonly BaseDeDonnees db;

    public GestionnaireDecodeurs(HttpClient httpClient, BaseDeDonnees db)
    {
        client = httpClient;
        this.db = db;
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // 4. Assigner un décodeur à un client
    public void AssignerDecodeurAClient(string clientId, string adresseIP)
    {
        var client = db.ObtenirClient(clientId);
        if (client != null && !client.Decodeurs.Any(d => d.AdresseIP == adresseIP))
        {
            client.Decodeurs.Add(new Decodeur(adresseIP));
        }
    }

    // 5. Afficher la liste des décodeurs (pour un client)
    public async Task<List<Decodeur>> ObtenirListeDecodeurs(string clientId)
    {
        var client = db.ObtenirClient(clientId);
        if (client == null) return new List<Decodeur>();

        List<Decodeur> decodeurs = new List<Decodeur>();
        foreach (var decodeur in client.Decodeurs)
        {
            var etatDecodeur = await ObtenirEtatDecodeur(decodeur.AdresseIP);
            if (etatDecodeur != null)
            {
                decodeur.Etat = etatDecodeur.Etat;
                decodeur.DernierRedemarrage = etatDecodeur.DernierRedemarrage;
                decodeur.DerniereReinitialisation = etatDecodeur.DerniereReinitialisation;
                decodeurs.Add(decodeur);
            }
        }
        return decodeurs;
    }

    // 6. Retirer un décodeur de la liste d’un client
    public bool RetirerDecodeurDeClient(string clientId, string adresseIP)
    {
        var client = db.ObtenirClient(clientId);
        if (client != null)
        {
            var decodeur = client.Decodeurs.FirstOrDefault(d => d.AdresseIP == adresseIP);
            if (decodeur != null)
            {
                client.Decodeurs.Remove(decodeur);
                return true;
            }
        }
        return false;
    }

    // 7. Obtenir l’état d’un décodeur
    public async Task<Decodeur> ObtenirEtatDecodeur(string adresseIP)
    {
        var postDataValues = new { id = codePermanent, address = adresseIP, action = "info" };
        var postData = new StringContent(JsonSerializer.Serialize(postDataValues));
        var response = await client.PostAsync("https://wflageol-uqtr.net/decoder", postData);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse);
            if (data["response"] == "OK")
            {
                return new Decodeur(adresseIP)
                {
                    Etat = data["state"],
                    DernierRedemarrage = DateTime.Parse(data["lastRestart"]),
                    DerniereReinitialisation = DateTime.Parse(data["lastReinit"])
                };
            }
        }
        return null;
    }

    // 8. Redémarrer un décodeur (avec notification)
    public async Task<(bool succes, string message)> RedemarrerDecodeur(string adresseIP)
    {
        var postDataValues = new { id = codePermanent, address = adresseIP, action = "reset" };
        var postData = new StringContent(JsonSerializer.Serialize(postDataValues));
        var response = await client.PostAsync("https://wflageol-uqtr.net/decoder", postData);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse);
            return (data["response"] == "OK", "Décodeur redémarré avec succès. Cela peut prendre 10 à 30 secondes.");
        }
        return (false, "Erreur lors du redémarrage du décodeur.");
    }

    // 9. Ajouter une chaîne à un décodeur (simulation)
    public bool AjouterChaineADecodeur(string clientId, string adresseIP, string nomChaine)
    {
        var client = db.ObtenirClient(clientId);
        if (client != null)
        {
            var decodeur = client.Decodeurs.FirstOrDefault(d => d.AdresseIP == adresseIP);
            if (decodeur != null && !decodeur.Chaines.Contains(nomChaine))
            {
                decodeur.Chaines.Add(nomChaine);
                return true;
            }
        }
        return false;
    }

    // 10. Retirer une chaîne d’un décodeur (simulation)
    public bool RetirerChaineDeDecodeur(string clientId, string adresseIP, string nomChaine)
    {
        var client = db.ObtenirClient(clientId);
        if (client != null)
        {
            var decodeur = client.Decodeurs.FirstOrDefault(d => d.AdresseIP == adresseIP);
            if (decodeur != null && decodeur.Chaines.Contains(nomChaine))
            {
                decodeur.Chaines.Remove(nomChaine);
                return true;
            }
        }
        return false;
    }
}