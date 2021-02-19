using System;
using System.Collections.Generic;

[Serializable]public class AutoKeyAndPathSettings
{
    public bool saveKeysForHealing;
    public int teamHPHealPercent;
    private List<AltPathResult> allowedKeyPaths;
    public int percentageChanceToPickUnknownPath;
    public int teamMPHealPercent;

	public AutoKeyAndPathSettings()
    {
        saveKeysForHealing = true;
        teamHPHealPercent = 60;
        allowedKeyPaths = new List<AltPathResult>();
        allowedKeyPaths.Add(AltPathResult.HEAL);
        percentageChanceToPickUnknownPath = 0;
        teamMPHealPercent = 40;
    }

    public bool IsAllowedPath(AltPathResult inPath)
    {
        return allowedKeyPaths.Contains(inPath);
    }

    public void SetAllowedPaths(List<AltPathResult> inAllowedPaths)
    {
        allowedKeyPaths.Clear();
        allowedKeyPaths.AddRange(inAllowedPaths);
    }
}
