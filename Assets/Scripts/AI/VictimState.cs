namespace CBRN.AI
{
    /// <summary>
    /// Operational states for disaster victims in the simulation.
    /// Drives the NavMesh AI state machine.
    /// </summary>
    public enum VictimState
    {
        Idle_Injured,   // Trapped near hazard, unable to move autonomously
        Panic_Agitated, // Wandering erratically, disorientation from chemical exposure
        Following_Hero, // Trainee has calmed/triaged victim; victim follows trainee to safety
        Safe_Evacuated  // Successfully delivered to Triage / Decontamination point
    }
}

