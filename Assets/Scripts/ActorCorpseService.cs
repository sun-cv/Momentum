using System.Collections.Generic;
using UnityEngine;



public class CorpseService : RegisteredService, IServiceLoop
{

    readonly List<CorpseRequest> queue  = new();

        // -----------------------------------

    readonly List<Actor> corpses        = new();

        // -----------------------------------
    
    public CorpseService()
    {
        Link.Global<CorpseRequest>(HandleCorpseSpawnRequest);
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessCorpseRequests();
    }

    // ===============================================================================

    void ProcessCorpseRequests()
    {
        if (queue.Count == 0)
            return;

        foreach (var request in queue)
            ProcessCorpseRequest(request);

        queue.Clear();
    }

    void ProcessCorpseRequest(CorpseRequest request)
    {
        if (!Validate.Asset(request.Context.Actor.Definition.Lifecycle.Spawn.Corpse))
                return;

        SpawnCorpse(request.Context);
    }

    void SpawnCorpse(CorpseContext context)
    {
        var factory  = Factories.Get<ICorpseFactory>(context.Actor.Definition.Name);
        var instance = factory.SpawnCorpse(context.Position);

        corpses.Add(instance);
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleCorpseSpawnRequest(CorpseRequest message)
    {
        queue.Add(message);
    }

    // ===============================================================================

    // readonly Logger Log = Logging.For(LogSystem.Corpse);

    public UpdatePriority Priority => ServiceUpdatePriority.CorpseService;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                     Classes
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseContext
{
    public Actor Actor                      { get; set; }                      
    public Vector3 Position                 { get; set; }
    public KillingBlow KillingBlow          { get; set; }
    public AnimationAPI DeathAnimation      { get; set; }
}

public class CorpseRequest : IMessage
{
    public CorpseContext Context            { get; init; }

    public CorpseRequest(CorpseContext context)
    {
        Context = context;
    }
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                      Enums
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum DeathType
{
    Standard,
    Dynamic
} 


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

