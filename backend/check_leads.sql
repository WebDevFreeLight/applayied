SELECT 
    leadid,
    brandname,
    ownername,
    ownerphone,
    assignedto,
    leadstatus,
    create_time
FROM bp_lead
ORDER BY create_time DESC
LIMIT 10;
