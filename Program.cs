using Core;
using Stateless.Graph;

var t = new ContratPublic("Test", null);

await t.Update(montant:12);

t.Envoyer();

var g = UmlDotGraph.Format(t.StatusMachine.GetInfo());

//Plante ! Pas possible d'update après une init
//await t.Update(montant:12);

Console.WriteLine($"Status : {t.Status.ToString()} {t.Name} {t.Montant}"); 