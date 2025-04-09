using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly Authentificateur authentificateur;
    private readonly GestionnaireDecodeurs gestionnaireDecodeurs;
    private readonly BaseDeDonnees db;

    public HomeController(Authentificateur authentificateur, GestionnaireDecodeurs gestionnaireDecodeurs, BaseDeDonnees db)
    {
        this.authentificateur = authentificateur;
        this.gestionnaireDecodeurs = gestionnaireDecodeurs;
        this.db = db;
    }

    // 1. Authentification
    public IActionResult Index()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid && authentificateur.Valider(model.Id, model.MotDePasse))
        {
            HttpContext.Session.SetString("UserId", model.Id);
            return RedirectToAction("Clients");
        }
        ModelState.AddModelError("", "Identifiant ou mot de passe incorrect.");
        return View("Index", model);
    }

    // Page des clients (point d'entrée après connexion)
    public IActionResult Clients()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        return View(db.ObtenirTousClients());
    }

    // 2. Création d’un client
    public IActionResult CreerClient()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        return View();
    }

    [HttpPost]
    public IActionResult CreerClient(Client client)
    {
        if (ModelState.IsValid)
        {
            db.AjouterClient(client);
            return RedirectToAction("Clients");
        }
        return View(client);
    }

    // 3. Suppression d’un client
    [HttpPost]
    public IActionResult SupprimerClient(string clientId)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        db.SupprimerClient(clientId);
        return RedirectToAction("Clients");
    }

    // 4. Assigner un décodeur à un client
 public async Task<IActionResult> AssignerDecodeur(string clientId)
{
    if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
    
    var decodeursDisponibles = await gestionnaireDecodeurs.ObtenirTousDecodeursDisponibles();
    var model = new AssignerDecodeurViewModel
    {
        ClientId = clientId,
        DecodeursDisponibles = decodeursDisponibles
    };
    return View(model);
}

[HttpPost]
public IActionResult AssignerDecodeur(AssignerDecodeurViewModel model)
{
    gestionnaireDecodeurs.AssignerDecodeurAClient(model.ClientId, model.AdresseIP);
    return RedirectToAction("Decodeurs", new { clientId = model.ClientId });
}

    // 5. Afficher la liste des décodeurs (pour un client)
    public async Task<IActionResult> Decodeurs(string clientId)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        var decodeurs = await gestionnaireDecodeurs.ObtenirListeDecodeurs(clientId);
        ViewBag.ClientId = clientId;
        return View(decodeurs);
    }

    // 6. Retirer un décodeur de la liste d’un client
    [HttpPost]
    public IActionResult RetirerDecodeur(string clientId, string adresseIP)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        gestionnaireDecodeurs.RetirerDecodeurDeClient(clientId, adresseIP);
        return RedirectToAction("Decodeurs", new { clientId });
    }

    // 7. Obtenir l’état d’un décodeur
    public async Task<IActionResult> EtatDecodeur(string adresseIP)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        var decodeur = await gestionnaireDecodeurs.ObtenirEtatDecodeur(adresseIP);
        return View(decodeur);
    }

    // 8. Redémarrer un décodeur (avec notification)
    public async Task<IActionResult> RedemarrerDecodeur(string adresseIP)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        var (succes, message) = await gestionnaireDecodeurs.RedemarrerDecodeur(adresseIP);
        TempData["Message"] = message;
        return RedirectToAction("EtatDecodeur", new { adresseIP });
    }

    // 9. Ajouter une chaîne à un décodeur
    public IActionResult AjouterChaine(string clientId, string adresseIP)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        ViewBag.ClientId = clientId;
        ViewBag.AdresseIP = adresseIP;
        return View();
    }

    [HttpPost]
    public IActionResult AjouterChaine(string clientId, string adresseIP, string nomChaine)
    {
        gestionnaireDecodeurs.AjouterChaineADecodeur(clientId, adresseIP, nomChaine);
        return RedirectToAction("Decodeurs", new { clientId });
    }

    // 10. Retirer une chaîne d’un décodeur
    [HttpPost]
    public IActionResult RetirerChaine(string clientId, string adresseIP, string nomChaine)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction("Index");
        gestionnaireDecodeurs.RetirerChaineDeDecodeur(clientId, adresseIP, nomChaine);
        return RedirectToAction("Decodeurs", new { clientId });
    }
}