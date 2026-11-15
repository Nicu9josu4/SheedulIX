using AcademicManagement.Data;
using AcademicManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AcademicManagement.Services;

/// <summary>Ciclul de viata al modelului de calcul al unui an: Draft → Active → Archived, plus copierea din anul anterior.</summary>
public class WorkloadModelService(AppDbContext context)
{
    public async Task<WorkloadModel> CreateAsync(string academicYear, string? name)
    {
        WorkloadService.AcademicYearRange(academicYear);
        if (await context.WorkloadModels.AnyAsync(m => m.AcademicYear == academicYear))
            throw new InvalidOperationException($"Există deja un model pentru {academicYear}.");

        var model = new WorkloadModel { AcademicYear = academicYear, Name = name ?? $"Model {academicYear}" };
        context.WorkloadModels.Add(model);
        await context.SaveChangesAsync();
        return model;
    }

    /// <summary>Copiaza normele si coeficientii unui model intr-un model nou (Draft) pentru alt an.</summary>
    public async Task<WorkloadModel> CloneAsync(Guid sourceId, string newAcademicYear, string? name)
    {
        var source = await context.WorkloadModels
            .Include(m => m.Norms).Include(m => m.Coefficients)
            .FirstOrDefaultAsync(m => m.Id == sourceId)
            ?? throw new KeyNotFoundException("Modelul sursă nu a fost găsit.");

        var copy = await CreateAsync(newAcademicYear, name);
        foreach (var n in source.Norms)
            context.PositionNorms.Add(new PositionNorm
            {
                WorkloadModelId = copy.Id, Position = n.Position,
                BaseNormHours = n.BaseNormHours, MinHours = n.MinHours, MaxHours = n.MaxHours
            });
        foreach (var c in source.Coefficients)
            context.ConversionCoefficients.Add(new ConversionCoefficient
            {
                WorkloadModelId = copy.Id, ActivityType = c.ActivityType, Coefficient = c.Coefficient
            });
        await context.SaveChangesAsync();
        return copy;
    }

    public async Task<PositionNorm> AddNormAsync(Guid modelId, PositionNorm norm)
    {
        var model = await RequireDraftAsync(modelId);
        if (string.IsNullOrWhiteSpace(norm.Position))
            throw new InvalidOperationException("Funcția este obligatorie.");
        if (norm.MinHours < 0 || norm.MinHours > norm.BaseNormHours || norm.BaseNormHours > norm.MaxHours)
            throw new InvalidOperationException("Trebuie respectat: 0 ≤ minim ≤ normă de bază ≤ maxim.");
        if (await context.PositionNorms.AnyAsync(n => n.WorkloadModelId == modelId && n.Position == norm.Position))
            throw new InvalidOperationException("Norma pentru această funcție există deja în model.");

        norm.Id = Guid.NewGuid();
        norm.WorkloadModelId = model.Id;
        context.PositionNorms.Add(norm);
        await context.SaveChangesAsync();
        return norm;
    }

    public async Task<ConversionCoefficient> AddCoefficientAsync(Guid modelId, ConversionCoefficient coefficient)
    {
        var model = await RequireDraftAsync(modelId);
        if (coefficient.Coefficient <= 0)
            throw new InvalidOperationException("Coeficientul trebuie să fie pozitiv.");
        if (await context.ConversionCoefficients.AnyAsync(c => c.WorkloadModelId == modelId && c.ActivityType == coefficient.ActivityType))
            throw new InvalidOperationException("Coeficientul pentru acest tip de activitate există deja în model.");

        coefficient.Id = Guid.NewGuid();
        coefficient.WorkloadModelId = model.Id;
        context.ConversionCoefficients.Add(coefficient);
        await context.SaveChangesAsync();
        return coefficient;
    }

    /// <summary>Activarea blocheaza modelul si cere ca toate cele 3 tipuri de activitate sa aiba coeficient si sa existe cel putin o norma.</summary>
    public async Task<WorkloadModel> ActivateAsync(Guid modelId)
    {
        var model = await RequireDraftAsync(modelId);
        await context.Entry(model).Collection(m => m.Norms).LoadAsync();
        await context.Entry(model).Collection(m => m.Coefficients).LoadAsync();

        var missing = Enum.GetValues<ActivityType>().Where(t => model.Coefficients.All(c => c.ActivityType != t)).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException("Lipsesc coeficienți pentru: " + string.Join(", ", missing) + ".");
        if (model.Norms.Count == 0)
            throw new InvalidOperationException("Modelul nu conține nicio normă pe funcții.");

        model.Status = WorkloadModelStatus.Active;
        await context.SaveChangesAsync();
        return model;
    }

    public async Task<WorkloadModel> ArchiveAsync(Guid modelId)
    {
        var model = await context.WorkloadModels.FindAsync(modelId)
            ?? throw new KeyNotFoundException("Modelul nu a fost găsit.");
        if (model.Status != WorkloadModelStatus.Active)
            throw new InvalidOperationException("Doar un model activ poate fi arhivat.");
        model.Status = WorkloadModelStatus.Archived;
        await context.SaveChangesAsync();
        return model;
    }

    private async Task<WorkloadModel> RequireDraftAsync(Guid modelId)
    {
        var model = await context.WorkloadModels.FindAsync(modelId)
            ?? throw new KeyNotFoundException("Modelul nu a fost găsit.");
        if (model.Status != WorkloadModelStatus.Draft)
            throw new InvalidOperationException("Modelul este blocat. Creează o copie pentru anul următor ca să-l modifici.");
        return model;
    }
}
