using System.Diagnostics;
using System.Text.Json;
using APU.DataV2.Context;
using Microsoft.EntityFrameworkCore;

namespace APU.WebApp.Services.UserSession;

public class UserSessionService
{
    private IDbContextFactory<ApuDbContext> _contextFactory;

    #region Lifecycle

    /// <summary>
    /// UserId - UserSession
    /// </summary>
    private readonly Dictionary<Guid, DataV2.Entities.UserSession> sessions;

    public UserSessionService()
    {
        sessions = new Dictionary<Guid, DataV2.Entities.UserSession>();

        // Todo: Init from database?
    }

    public async void Initialize(IDbContextFactory<ApuDbContext> contextFactory)
    {
        _contextFactory = contextFactory;

        try
        {
            var dbContext = await _contextFactory.CreateDbContextAsync();
            var userSessions = await dbContext.UserSessions.ToListAsync();
            foreach (var userSession in userSessions)
                sessions.Add(userSession.UserId, userSession);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public void AddSession(DataV2.Entities.UserSession session)
    {
        if (sessions.ContainsKey(session.UserId))
            sessions.Add(session.UserId, session);
    }

    public void RemoveSession(Guid userId)
    {
        if (sessions.ContainsKey(userId))
            sessions.Remove(userId);
    }

    #endregion

    private async Task<bool> TryGetForUser(Guid userId)
    {
        try
        {
            var dbContext = await _contextFactory.CreateDbContextAsync();
            var userSession = await dbContext.UserSessions.FirstOrDefaultAsync(q => q.UserId == userId);
            if (userSession is not null)
            {
                sessions.Add(userSession.UserId, userSession);
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return false;
    }

    private async void UpdateUserSession(DataV2.Entities.UserSession session)
    {
        try
        {
            var dbContext = await _contextFactory.CreateDbContextAsync();
            var dbSession = await dbContext.UserSessions.FirstOrDefaultAsync(q => q.UserId == session.UserId);
            if (dbSession is null)
                return;

            dbSession.BlockProject = session.BlockProject;
            dbSession.SelectedProjectId = session.SelectedProjectId;
            dbSession.EstimatePageGridColumns = session.EstimatePageGridColumns;

            dbContext.UserSessions.Update(dbSession);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    #region ProjectBlock

    public async void SetProjectBlock(Guid userId, bool? blockProject)
    {
        if (!sessions.ContainsKey(userId))
        {
            if (!await TryGetForUser(userId))
                return;
        }

        sessions[userId].BlockProject = blockProject;
        UpdateUserSession(sessions[userId]);
    }

    public bool? GetProjectBlock(Guid userId)
    {
        if (!sessions.ContainsKey(userId))
            return false;

        return sessions[userId].BlockProject;
    }

    #endregion

    #region ProjectId

    public async void SetSelectedProject(Guid userId, Guid? projectId)
    {
        if (!sessions.ContainsKey(userId))
        {
            if (!await TryGetForUser(userId))
                return;
        }

        sessions[userId].SelectedProjectId = projectId;
        UpdateUserSession(sessions[userId]);
    }

    public Guid? GetSelectedProjectId(Guid userId)
    {
        if (!sessions.ContainsKey(userId))
            return null;

        return sessions[userId].SelectedProjectId;
    }

    #endregion

    #region EstimateGridColumns

    public async void SetEstimateGridColumns(Guid userId, Dictionary<string, bool> gridColumns)
    {
        gridColumns ??= new Dictionary<string, bool>();

        if (!sessions.ContainsKey(userId))
        {
            if (!await TryGetForUser(userId))
                return;
        }

        try
        {
            sessions[userId].EstimatePageGridColumns = JsonSerializer.Serialize(gridColumns);
            UpdateUserSession(sessions[userId]);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public Dictionary<string, bool> GetEstimateGridColumns(Guid userId)
    {
        if (!sessions.ContainsKey(userId))
            return new Dictionary<string, bool>();

        try
        {
            var gridColumns = JsonSerializer.Deserialize<Dictionary<string, bool>>(sessions[userId].EstimatePageGridColumns);
            return gridColumns;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new Dictionary<string, bool>();
        }
    }

    #endregion

}