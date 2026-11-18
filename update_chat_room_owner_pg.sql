START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110092202_AddChatRoomOwner') THEN
    ALTER TABLE chat_room ADD owner_id integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261110092202_AddChatRoomOwner') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261110092202_AddChatRoomOwner', '9.0.3');
    END IF;
END $EF$;
COMMIT;

