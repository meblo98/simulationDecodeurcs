public class Authentificateur
{
    private readonly BaseDeDonnees db;

    public Authentificateur(BaseDeDonnees db)
    {
        this.db = db;
    }

    public bool Valider(string id, string motDePasse)
    {
        return db.VerifierCredentials(id, motDePasse);
    }
}