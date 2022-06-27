using Stateless;

namespace Core;

public enum MarcheState
{
    Brouillon,
    Initialisé,
    DemandeDeComplément,
    Validé,
    Refusé,
}

public enum MarcheActions
{
    Modification,
    Envois,
    DemandeDeComplément,
    Validation,
    Refus,
}


public class ContratPublic
{

    public Guid Id { get; private set; }
    public string? Name { get; private set; }
    public decimal? Montant { get; private set; }
    public bool Terminé { get; private set; }
    public DateTime? DateDeValidation { get; private set; }
    public bool DemandeDeComplément { get; private set; }

    public MarcheState Status => StatusMachine.State;

    public StateMachine<MarcheState, MarcheActions> StatusMachine { get; set; }

    public ContratPublic()
    {
        InitState();
    }

    public ContratPublic(string? name, decimal? montant)
    {
        Id = Guid.NewGuid();
        Name = name;
        Montant = montant;

        InitState();
    }

    protected StateMachine<MarcheState, MarcheActions>.TriggerWithParameters<string?, decimal?> ModificationTrigger;

    public async Task Update(string? name = null, decimal? montant = null)
    {
        await StatusMachine.FireAsync(ModificationTrigger, name, montant);
    }

    protected void OnUpdate(string? name, decimal? montant)
    {
        if (name != null)
        {
            Name = name;
        }

        if (montant != null)
        {
            Montant = montant;
        }
    }

    public void Envoyer()
    {
        StatusMachine.Fire(MarcheActions.Envois);
    }


    protected void InitState()
    {
        StatusMachine = new StateMachine<MarcheState, MarcheActions>(
            () =>
            DateDeValidation.HasValue ?
                MarcheState.Validé
            : DemandeDeComplément ?
                MarcheState.DemandeDeComplément
            : Terminé ?
                MarcheState.Initialisé
            : MarcheState.Brouillon, 
            s => { /* Pas de sauvegarde du status */ });


        ModificationTrigger = StatusMachine.SetTriggerParameters<string?, decimal?>(MarcheActions.Modification);

        StatusMachine.Configure(MarcheState.Brouillon)
            .PermitReentry(MarcheActions.Modification)
            .OnEntryFrom(ModificationTrigger,
                (name, montant) => OnUpdate(name, montant))
            .PermitIf(MarcheActions.Envois, MarcheState.Initialisé,
                () => !string.IsNullOrWhiteSpace(Name) && Montant.HasValue)
            .OnExit(t =>
            {
                if (t.Destination == MarcheState.Initialisé)
                {
                    Terminé = true;
                }
            });

        StatusMachine.Configure(MarcheState.Initialisé)
            .Permit(MarcheActions.DemandeDeComplément, MarcheState.DemandeDeComplément)
            .Permit(MarcheActions.Validation, MarcheState.Validé)
            .Permit(MarcheActions.Refus, MarcheState.Refusé);

        StatusMachine.Configure(MarcheState.DemandeDeComplément)
            .PermitReentry(MarcheActions.DemandeDeComplément)
            .Permit(MarcheActions.Envois, MarcheState.Initialisé);
    }
}