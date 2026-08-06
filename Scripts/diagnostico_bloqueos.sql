-- Diagnóstico de bloqueos y transacciones abiertas en CRM_Base
-- Ejecutar en SQL Server Management Studio mientras el error de timeout se esté reproduciendo.

USE CRM_Base;
GO

-- 1) Solicitudes activas ordenadas por tiempo transcurrido.
--    Si hay una columna blocking_session_id con un número, esa sesión está bloqueando a la otra.
SELECT
    req.session_id,
    req.blocking_session_id,
    req.status,
    req.command,
    req.start_time,
    req.total_elapsed_time / 1000 AS elapsed_seconds,
    sqltext.text AS sql_text
FROM sys.dm_exec_requests req
CROSS APPLY sys.dm_exec_sql_text(req.sql_handle) sqltext
WHERE req.database_id = DB_ID('CRM_Base')
ORDER BY req.total_elapsed_time DESC;
GO

-- 2) Transacción abierta más antigua en la base de datos.
--    Si devuelve un SPID, esa sesión tiene una transacción sin confirmar.
DBCC OPENTRAN('CRM_Base');
GO

-- 3) Sesiones con transacciones abiertas.
SELECT
    s.session_id,
    s.login_name,
    s.program_name,
    s.host_name,
    t.open_transaction_count
FROM sys.dm_exec_sessions s
JOIN sys.dm_exec_requests r ON s.session_id = r.session_id
LEFT JOIN sys.dm_tran_session_transactions t ON s.session_id = t.session_id
WHERE t.open_transaction_count > 0;
GO

-- 4) Si identificas la sesión bloqueadora y es seguro matarla (no es un proceso de producción crítico):
--    KILL <session_id>;
