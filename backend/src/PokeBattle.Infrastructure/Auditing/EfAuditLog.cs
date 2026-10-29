using Microsoft.EntityFrameworkCore;

using PokeBattle.Application.Auditing;
using PokeBattle.Infrastructure.Persistence;

namespace PokeBattle.Infrastructure.Auditing;

/// <summary>Writes each entry right away; a failed write fails the request instead of losing the entry silently.</summary>
public sealed class EfAuditLog(IDbContextFactory<PokeBattleDbContext> contexts) : IAuditLog
{
    public async Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var db = await contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            db.AuditLog.Add(new AuditLogEntry
            {
                Event = entry.Event,
                UserId = entry.UserId,
                Target = entry.Target,
                Area = entry.Area,
                Method = entry.Method,
                Version = entry.Version,
                Milestone = entry.Milestone,
                Objective = entry.Objective,
                Candidates = [.. entry.Candidates],
                OccurredAt = entry.OccurredAt,
            });
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}