-- Ready-made queries for the demonstration (database: ElevatorDb on (localdb)\MSSQLLocalDB)

-- 1. Everything that happened, newest first
SELECT l.LogId, l.LoggedAt, e.ElevatorName, f.FloorName, l.EventType, l.Message
FROM dbo.ElevatorLog l
LEFT JOIN dbo.Elevators e ON e.ElevatorId = l.ElevatorId
LEFT JOIN dbo.Floors f ON f.FloorId = l.FloorId
ORDER BY l.LogId DESC;

-- 2. How many events each lift has had, by type
SELECT e.ElevatorName, l.EventType, COUNT(*) AS Times
FROM dbo.ElevatorLog l
JOIN dbo.Elevators e ON e.ElevatorId = l.ElevatorId
GROUP BY e.ElevatorName, l.EventType
ORDER BY e.ElevatorName, Times DESC;

-- 3. Which lift answered each outside call
SELECT l.LoggedAt, e.ElevatorName, l.Message
FROM dbo.ElevatorLog l
JOIN dbo.Elevators e ON e.ElevatorId = l.ElevatorId
WHERE l.EventType = N'HallCallAssigned'
ORDER BY l.LogId;

-- 4. Emergencies
SELECT l.LoggedAt, e.ElevatorName, l.EventType, l.Message
FROM dbo.ElevatorLog l
JOIN dbo.Elevators e ON e.ElevatorId = l.ElevatorId
WHERE l.EventType IN (N'EmergencyOn', N'EmergencyOff')
ORDER BY l.LogId;

-- 5. Delete the log (the same thing the Clear Log button does)
-- DELETE FROM dbo.ElevatorLog;
