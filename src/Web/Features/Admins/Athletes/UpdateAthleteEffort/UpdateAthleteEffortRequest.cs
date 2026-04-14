using System;

namespace Web.Features.Admins.Athletes.UpdateAthleteEffort;

public class UpdateAthleteEffortRequest
{
    public Guid AthleteId { get; set; }
    public Guid EffortId { get; set; }
    public int Effort { get; set; }
    public int DurationMinutes { get; set; }
    public int? Pleasure { get; set; }
    public DateTime? TrainingDate { get; set; }
}
