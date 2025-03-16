CREATE TABLE admin_chats
(
    chat_id    INTEGER        NOT NULL,
    username   TEXT           NOT NULL,
    phone_nr   TEXT           NOT NULL,
    CONSTRAINT PK_admin_chats PRIMARY KEY (chat_id, username, phone_nr)
);
