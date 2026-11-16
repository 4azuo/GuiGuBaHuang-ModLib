namespace ModLib.Enum
{
    /// <summary>
    /// Tri-state flag: True, False or Ignore (used by EventConditionAttribute).
    /// </summary>
    public enum HandleEnum
    {
        Ignore = -1,
        False = 0,
        True = 1,
    }
}
