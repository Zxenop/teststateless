
using Core;

var t = new ContratPublic("Test", null);

await t.Update(montant:12);

t.Envoyer();

Console.WriteLine($"Status : {t.Status.ToString()} {t.Name} {t.Montant}"); 



