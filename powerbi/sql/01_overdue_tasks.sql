-- Q1 — Overdue incomplete tasks (delivery)
-- Definition: due before the reporting snapshot and not Done. The snapshot (:snapshot) is a
-- Power Query parameter (default 2026-09-30); production passes today() or a user-selected date.
-- Source tables: pmo_task -> pmo_project (project name) -> sys_user (assignee display name).
SELECT  t.task_id,
        t.title                                   AS task_title,
        p.code                                    AS project_code,
        p.name                                    AS project_name,
        p.dept_id,
        COALESCE(u.nickname, u.username, '(unassigned)') AS assignee,
        t.status,
        t.priority,
        t.due_date,
        (DATE '2026-09-30' - t.due_date)          AS days_overdue
FROM    pmo_task  t
JOIN    pmo_project p ON p.project_id = t.project_id
LEFT JOIN sys_user u  ON u.userid     = t.assignee_user_id
WHERE   t.due_date < DATE '2026-09-30'          -- strictly before the snapshot
  AND   t.status <> 'Done'
ORDER BY days_overdue DESC, p.name
