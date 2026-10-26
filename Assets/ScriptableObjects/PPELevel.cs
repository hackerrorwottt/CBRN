namespace CBRN.PPE
{
    /// <summary>
    /// Standard Hazmat Personal Protective Equipment (PPE) ensembles
    /// based on emergency responder classifications.
    /// </summary>
    public enum PPELevel
    {
        LevelD_WorkCoveralls,       // Basic work uniform, no respiratory or chemical protection
        LevelC_AirPurifyingResp,    // Chemical splash suit + Air Purifying Respirator (APR)
        LevelB_NonEncapsulating,    // Chemical splash suit + Self-Contained Breathing Apparatus (SCBA)
        LevelA_FullyEncapsulated    // Vapor-protective fully encapsulating suit + SCBA (Maximum protection)
    }
}

