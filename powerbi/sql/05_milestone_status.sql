-- Q5 — Milestone status & roadmap (delivery)
-- Status relative to the snapshot: Achieved, or Overdue (pending & past due), or Upcoming.
SELECT  m.milestone_id,
        m.name              AS milestone_name,
        p.code              AS project_code,
        p.name              AS project_name,
        p.dept_id,
        m.due_date,
        m.status,
        CASE
            WHEN m.status = 'Achieved'                               THEN 'Achieved'
            WHEN m.due_date < DATE '2026-09-30' AND m.status <> 'Achieved' THEN 'Overdue'
            ELSE 'Upcoming'
        END                 AS roadmap_bucket
FROM    pmo_milestone m
JOIN    pmo_project p ON p.project_id = m.project_id
ORDER BY m.due_date
