namespace PMS.Domain.Entities;

public class Kra : BaseEntity
{
    public string WorkingYearId { get; set; } = string.Empty;
    public string AppraisalPeriodId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Weight { get; set; }

    public ICollection<KraOrgUnitAssignment> AssignedOrgUnits { get; set; } = new List<KraOrgUnitAssignment>();
    public ICollection<Objective> Objectives { get; set; } = new List<Objective>();
}

public class KraOrgUnitAssignment : BaseEntity
{
    public long KraId { get; set; }
    public Kra? Kra { get; set; }

    public long OrgUnitId { get; set; }
    public string OrgUnitType { get; set; } = string.Empty; // DEPT | DIV | BRANCH
    public string? OrgUnitName { get; set; }
}

public class Objective : BaseEntity
{
    public long KraId { get; set; }
    public Kra? Kra { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Weight { get; set; }

    public ICollection<ObjectiveOrgUnitAssignment> AssignedOrgUnits { get; set; } = new List<ObjectiveOrgUnitAssignment>();
    public ICollection<ObjectiveAssignedWeight> AssignedWeights { get; set; } = new List<ObjectiveAssignedWeight>();
    public ICollection<Kpi> Kpis { get; set; } = new List<Kpi>();
}

public class ObjectiveOrgUnitAssignment : BaseEntity
{
    public long ObjectiveId { get; set; }
    public Objective? Objective { get; set; }

    public long OrgUnitId { get; set; }
    public string OrgUnitType { get; set; } = string.Empty; // DEPT | DIV | BRANCH
    public string? OrgUnitName { get; set; }
}

public class ObjectiveAssignedWeight : BaseEntity
{
    public long ObjectiveId { get; set; }
    public Objective? Objective { get; set; }

    public long UnitId { get; set; }
    public string Scope { get; set; } = string.Empty; // DEPT | DIV | BRANCH
    public int Weight { get; set; }
}

public class Kpi : BaseEntity
{
    public long ObjectiveId { get; set; }
    public Objective? Objective { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Weight { get; set; }
    public decimal Target { get; set; }
    public string Unit { get; set; } = string.Empty; // %, days, count, etc
    public string MeasurementType { get; set; } = string.Empty; // Number | Percentage | Amount | Days | Hours | Rating
    public string? DataSource { get; set; }
}


