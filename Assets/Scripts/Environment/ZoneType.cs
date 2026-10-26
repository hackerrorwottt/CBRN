namespace CBRN.Environment
{
    /// <summary>
    /// CBRN disaster management operational zones based on standard responder protocols.
    /// </summary>
    public enum ZoneType
    {
        ColdZone_Staging,       // Safe area: Briefing, PPE selection, trainee start
        WarmZone_Perimeter,     // Buffer area: Decontamination checkpoint and access control
        HotZone_ChemicalLeak,   // Danger area: Chemical tanks, pipe rupture, trapped victims
        Evacuation_TriagePoint  // Safe extraction zone: Medical handover point
    }
}

