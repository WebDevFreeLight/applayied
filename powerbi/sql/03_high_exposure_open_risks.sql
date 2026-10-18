-- Q3 — High-exposure OPEN risks (risk & governance)
-- Exposure score = probability * impact; "high" = score >= 12. Closed/Mitigated risks are
-- excluded so a closed high risk never counts as open (edge case in the data).
SELECT  r.risk_id,
        r.title                                   AS risk_title,
        r.kind,                                   -- Risk | Issue
        p.code                                    AS project_code,
        p.name                                    AS project_name,
        p.dept_id,
        COALESCE(u.nickname, u.username)          AS owner,
        r.probability,
        r.impact,
        r.score                                   AS exposure_score,
        r.status
FROM    pmo_risk r
JOIN    pmo_project p ON p.project_id = r.project_id
LEFT JOIN sys_user u  ON u.userid     = r.owner_user_id
WHERE   r.status = 'Open'
  AND   r.score >= 12
ORDER BY r.score DESC, p.name
