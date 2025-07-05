USE master
GO

-- ***NOTE***: Change `<secure_password>` to a secure password

CREATE Login contacts_user
WITH PASSWORD='<secure_password>'

USE Contacts
GO

CREATE USER contacts_user 
FOR LOGIN myconferenceevent_user WITH DEFAULT_SCHEMA=dbo;
GO

ALTER ROLE db_datareader ADD MEMBER contacts_user;
ALTER ROLE db_datawriter ADD MEMBER contacts_user;
GO