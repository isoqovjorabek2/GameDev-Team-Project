namespace CircuitSolver.Data
{
    public enum ComponentType
    {
        Resistor = 0,
        Battery = 1,
        VoltageSource = 2,
        Wire = 3
    }

    public enum TargetKind
    {
        CurrentThroughComponent = 0,
        VoltageAcrossComponent = 1,
        VoltageAtNode = 2
    }

    public enum DifficultyTier
    {
        Intro = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Expert = 4
    }
}
